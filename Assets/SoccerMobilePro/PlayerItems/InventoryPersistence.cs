using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoccerMobilePro.PlayerItems
{
    public sealed class InventoryEnvelope
    {
        public int SchemaVersion { get; set; } = InventoryCodec.CurrentSchemaVersion;
        public List<InventorySnapshot> Inventories { get; set; } = new List<InventorySnapshot>();
        public List<TransactionReceipt> Receipts { get; set; } = new List<TransactionReceipt>();
        public List<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);
    }

    public sealed class InventoryCodec
    {
        public const int CurrentSchemaVersion = 2;
        public const int BackwardSchemaWindow = 1;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            DateParseHandling = DateParseHandling.DateTimeOffset
        };

        public string Serialize(InventoryEnvelope envelope)
            => JsonConvert.SerializeObject(envelope ?? throw new ArgumentNullException(nameof(envelope)), Settings);

        public InventoryEnvelope Deserialize(string payload, out bool migrated)
        {
            if (string.IsNullOrWhiteSpace(payload)) throw new InventoryPersistenceException("Inventory payload is empty.");
            InventoryEnvelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<InventoryEnvelope>(payload, Settings)
                    ?? throw new InventoryPersistenceException("Inventory payload decoded to null.");
            }
            catch (JsonException exception)
            {
                throw new InventoryPersistenceException("Inventory payload is invalid JSON.", exception);
            }

            if (envelope.SchemaVersion > CurrentSchemaVersion || envelope.SchemaVersion < CurrentSchemaVersion - BackwardSchemaWindow)
                throw new InventoryPersistenceException("Inventory schema is outside the N/N-1 compatibility window.");

            migrated = envelope.SchemaVersion == CurrentSchemaVersion - 1;
            if (migrated) envelope.SchemaVersion = CurrentSchemaVersion;
            envelope.Inventories = envelope.Inventories ?? new List<InventorySnapshot>();
            envelope.Receipts = envelope.Receipts ?? new List<TransactionReceipt>();
            envelope.LedgerEntries = envelope.LedgerEntries ?? new List<LedgerEntry>();
            foreach (InventorySnapshot snapshot in envelope.Inventories)
            {
                snapshot.SchemaVersion = CurrentSchemaVersion;
                snapshot.Items = snapshot.Items ?? new List<OwnedPlayerItem>();
            }

            return envelope;
        }
    }

    public sealed class FileInventoryStore : IInventoryRepository, ITransactionReceiptRepository, ILedger
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly object sync = new object();
        private readonly string path;
        private readonly string backupPath;
        private readonly InventoryCodec codec;
        private InventoryEnvelope envelope;

        public FileInventoryStore(string path, InventoryCodec codec = null, bool forceReadOnly = false)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Inventory path is required.", nameof(path));
            this.path = Path.GetFullPath(path);
            backupPath = this.path + ".bak";
            this.codec = codec ?? new InventoryCodec();
            IsReadOnly = forceReadOnly;
            envelope = LoadInitial();
        }

        public bool IsReadOnly { get; private set; }
        public bool LoadedFromBackup { get; private set; }
        public bool MigratedFromPreviousSchema { get; private set; }

        public bool TryLoad(string ownerId, out InventorySnapshot snapshot)
        {
            lock (sync)
            {
                InventorySnapshot stored = envelope.Inventories.FirstOrDefault(item => string.Equals(item.OwnerId, ownerId, StringComparison.Ordinal));
                snapshot = stored?.Clone();
                return snapshot != null;
            }
        }

        public bool TryCommit(string ownerId, long expectedRevision, InventorySnapshot next, TransactionReceipt receipt, IReadOnlyList<LedgerEntry> entries)
        {
            if (IsReadOnly || next == null || receipt == null || entries == null || entries.Sum(entry => entry.Amount) != 0L) return false;
            lock (sync)
            {
                InventorySnapshot current = envelope.Inventories.FirstOrDefault(item => string.Equals(item.OwnerId, ownerId, StringComparison.Ordinal));
                if ((current?.Revision ?? -1L) != expectedRevision) return false;
                if (envelope.Receipts.Any(item => string.Equals(item.IdempotencyKey, receipt.IdempotencyKey, StringComparison.Ordinal))) return false;

                InventoryEnvelope nextEnvelope = CloneEnvelope(envelope);
                int index = nextEnvelope.Inventories.FindIndex(item => string.Equals(item.OwnerId, ownerId, StringComparison.Ordinal));
                if (index >= 0) nextEnvelope.Inventories[index] = next.Clone();
                else nextEnvelope.Inventories.Add(next.Clone());
                nextEnvelope.Receipts.Add(receipt);
                nextEnvelope.LedgerEntries.AddRange(entries.Select(CloneEntry));

                if (!Persist(nextEnvelope)) return false;
                envelope = nextEnvelope;
                return true;
            }
        }

        public bool TryGetReceipt(string idempotencyKey, out TransactionReceipt receipt)
        {
            lock (sync)
            {
                receipt = envelope.Receipts.FirstOrDefault(item => string.Equals(item.IdempotencyKey, idempotencyKey, StringComparison.Ordinal));
                return receipt != null;
            }
        }

        public IReadOnlyList<LedgerEntry> EntriesForTransaction(string transactionId)
        {
            lock (sync) return envelope.LedgerEntries.Where(item => string.Equals(item.TransactionId, transactionId, StringComparison.Ordinal)).Select(CloneEntry).ToList().AsReadOnly();
        }

        public bool IsBalanced(string transactionId) => EntriesForTransaction(transactionId).Sum(entry => entry.Amount) == 0L;

        private InventoryEnvelope LoadInitial()
        {
            if (!File.Exists(path)) return new InventoryEnvelope();
            if (TryRead(path, out InventoryEnvelope active, out bool migrated))
            {
                MigratedFromPreviousSchema = migrated;
                return active;
            }

            IsReadOnly = true;
            if (File.Exists(backupPath) && TryRead(backupPath, out InventoryEnvelope backup, out bool backupMigrated))
            {
                LoadedFromBackup = true;
                MigratedFromPreviousSchema = backupMigrated;
                return backup;
            }

            return new InventoryEnvelope();
        }

        private bool TryRead(string candidate, out InventoryEnvelope result, out bool migrated)
        {
            try
            {
                result = codec.Deserialize(File.ReadAllText(candidate, Utf8), out migrated);
                return true;
            }
            catch (Exception) when (candidate != null)
            {
                result = null;
                migrated = false;
                return false;
            }
        }

        private bool Persist(InventoryEnvelope value)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string staging = path + ".tmp";
            try
            {
                File.WriteAllText(staging, codec.Serialize(value), Utf8);
                bool ignored;
                codec.Deserialize(File.ReadAllText(staging, Utf8), out ignored);
                if (File.Exists(path)) File.Replace(staging, path, backupPath, true);
                else File.Move(staging, path);
                return true;
            }
            catch
            {
                if (File.Exists(staging)) File.Delete(staging);
                return false;
            }
        }

        private InventoryEnvelope CloneEnvelope(InventoryEnvelope source)
        {
            bool ignored;
            return codec.Deserialize(codec.Serialize(source), out ignored);
        }

        private static LedgerEntry CloneEntry(LedgerEntry entry)
            => new LedgerEntry { TransactionId = entry.TransactionId, AccountId = entry.AccountId, ResourceId = entry.ResourceId, Amount = entry.Amount };
    }

    public sealed class InventoryPersistenceException : Exception
    {
        public InventoryPersistenceException(string message) : base(message) { }
        public InventoryPersistenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
