using System;
using System.Collections.Generic;
using System.Linq;

namespace SoccerMobilePro.PlayerItems
{
    public sealed class InMemoryInventoryStore : IInventoryRepository, ITransactionReceiptRepository, ILedger
    {
        private readonly object sync = new object();
        private readonly Dictionary<string, InventorySnapshot> inventories = new Dictionary<string, InventorySnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, TransactionReceipt> receipts = new Dictionary<string, TransactionReceipt>(StringComparer.Ordinal);
        private readonly List<LedgerEntry> ledger = new List<LedgerEntry>();

        public void Seed(InventorySnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.OwnerId)) throw new ArgumentException("A seeded inventory needs an owner.", nameof(snapshot));
            lock (sync) inventories[snapshot.OwnerId] = snapshot.Clone();
        }

        public bool TryLoad(string ownerId, out InventorySnapshot snapshot)
        {
            lock (sync)
            {
                if (inventories.TryGetValue(ownerId ?? string.Empty, out InventorySnapshot stored))
                {
                    snapshot = stored.Clone();
                    return true;
                }

                snapshot = null;
                return false;
            }
        }

        public bool TryCommit(string ownerId, long expectedRevision, InventorySnapshot next, TransactionReceipt receipt, IReadOnlyList<LedgerEntry> entries)
        {
            if (next == null || receipt == null || entries == null) return false;
            lock (sync)
            {
                bool exists = inventories.TryGetValue(ownerId ?? string.Empty, out InventorySnapshot current);
                if ((exists ? current.Revision : -1L) != expectedRevision) return false;
                if (receipts.ContainsKey(receipt.IdempotencyKey)) return false;
                if (entries.Sum(entry => entry.Amount) != 0L) return false;

                inventories[ownerId] = next.Clone();
                receipts[receipt.IdempotencyKey] = receipt;
                ledger.AddRange(entries.Select(CloneEntry));
                return true;
            }
        }

        public bool TryGetReceipt(string idempotencyKey, out TransactionReceipt receipt)
        {
            lock (sync) return receipts.TryGetValue(idempotencyKey ?? string.Empty, out receipt);
        }

        public IReadOnlyList<LedgerEntry> EntriesForTransaction(string transactionId)
        {
            lock (sync) return ledger.Where(entry => string.Equals(entry.TransactionId, transactionId, StringComparison.Ordinal)).Select(CloneEntry).ToList().AsReadOnly();
        }

        public bool IsBalanced(string transactionId) => EntriesForTransaction(transactionId).Sum(entry => entry.Amount) == 0L;

        private static LedgerEntry CloneEntry(LedgerEntry entry)
            => new LedgerEntry { TransactionId = entry.TransactionId, AccountId = entry.AccountId, ResourceId = entry.ResourceId, Amount = entry.Amount };
    }

    public sealed class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IInventoryRepository inventory;
        private readonly ITransactionReceiptRepository receipts;
        private readonly IProgressionRuleSet rules;
        private readonly bool readOnly;

        public InventoryTransactionService(IInventoryRepository inventory, ITransactionReceiptRepository receipts, IProgressionRuleSet rules, bool readOnly = false)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.receipts = receipts ?? throw new ArgumentNullException(nameof(receipts));
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            this.readOnly = readOnly;
        }

        public TransactionReceipt Grant(GrantCommand command, DateTimeOffset nowUtc)
        {
            string payload = command?.CanonicalPayload() ?? string.Empty;
            string payloadHash = ProgressionHash.ComputePayload(payload);
            if (command == null || command.Item == null || string.IsNullOrWhiteSpace(command.IdempotencyKey) || string.IsNullOrWhiteSpace(command.Item.ItemId) || string.IsNullOrWhiteSpace(command.Item.OwnerId))
                return Reject(command?.IdempotencyKey, payloadHash, TransactionFailureCode.InvalidRequest, nowUtc);
            if (readOnly) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.ReadOnly, nowUtc);
            if (TryReplay(command.IdempotencyKey, payloadHash, nowUtc, out TransactionReceipt replay)) return replay;

            bool exists = inventory.TryLoad(command.Item.OwnerId, out InventorySnapshot current);
            if (exists && current.Find(command.Item.ItemId) != null) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.DuplicateItem, nowUtc);
            if (!string.Equals(command.Item.RulesVersion, rules.Version, StringComparison.Ordinal)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleRules, nowUtc);

            InventorySnapshot next = exists
                ? current.Clone()
                : new InventorySnapshot { OwnerId = command.Item.OwnerId, Revision = -1, CatalogVersion = command.Item.CatalogVersion, RulesVersion = command.Item.RulesVersion };
            if (!string.Equals(next.CatalogVersion, command.Item.CatalogVersion, StringComparison.Ordinal)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleCatalog, nowUtc);
            long baseRevision = next.Revision;
            OwnedPlayerItem granted = command.Item.Clone();
            granted.Revision = 0;
            next.Items.Add(granted);
            next.Revision = baseRevision + 1;

            InventoryDelta delta = Delta(baseRevision, next.Revision, new[] { granted }, Array.Empty<string>());
            return Commit(command.Item.OwnerId, baseRevision, command.IdempotencyKey, payloadHash, ProgressionOperation.DirectGrant, next, delta, nowUtc);
        }

        public TransactionReceipt Execute(ProgressionCommand command, ProgressionPreview preview, DateTimeOffset nowUtc)
        {
            string payload = command?.CanonicalPayload() ?? string.Empty;
            string payloadHash = ProgressionHash.ComputePayload(payload);
            if (command == null || preview == null || string.IsNullOrWhiteSpace(command.IdempotencyKey)) return Reject(command?.IdempotencyKey, payloadHash, TransactionFailureCode.InvalidRequest, nowUtc);
            if (readOnly) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.ReadOnly, nowUtc);
            if (TryReplay(command.IdempotencyKey, payloadHash, nowUtc, out TransactionReceipt replay)) return replay;
            if (!inventory.TryLoad(command.OwnerId, out InventorySnapshot current)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.ItemNotFound, nowUtc);
            if (current.Revision != command.ExpectedInventoryRevision || current.Revision != preview.InventoryRevision) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleRevision, nowUtc);

            OwnedPlayerItem currentItem = current.Find(command.ItemId);
            if (currentItem == null) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.ItemNotFound, nowUtc);
            if (currentItem.Revision != command.ExpectedItemRevision || currentItem.Revision != preview.ItemRevision) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleRevision, nowUtc);
            if (!string.Equals(current.CatalogVersion, preview.CatalogVersion, StringComparison.Ordinal)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleCatalog, nowUtc);
            if (!string.Equals(current.RulesVersion, rules.Version, StringComparison.Ordinal) || !string.Equals(preview.RulesVersion, rules.Version, StringComparison.Ordinal)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleRules, nowUtc);
            if (preview.ExpiresAt < nowUtc) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.PreviewExpired, nowUtc);
            if (!string.Equals(command.PreviewHash, preview.PreviewHash, StringComparison.Ordinal) || !string.Equals(preview.PreviewHash, ProgressionHash.ComputePreview(preview), StringComparison.Ordinal))
                return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.PreviewMismatch, nowUtc);
            if (!string.Equals(command.Intent?.CanonicalValue(), preview.Intent?.CanonicalValue(), StringComparison.Ordinal)) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.PreviewMismatch, nowUtc);

            InventorySnapshot next = current.Clone();
            int targetIndex = next.Items.FindIndex(item => string.Equals(item.ItemId, command.ItemId, StringComparison.Ordinal));
            next.Items[targetIndex] = preview.AfterItem.Clone();
            var removed = new List<string>();
            if (!string.IsNullOrEmpty(preview.SourceItemId))
            {
                int sourceIndex = next.Items.FindIndex(item => string.Equals(item.ItemId, preview.SourceItemId, StringComparison.Ordinal));
                if (sourceIndex < 0) return Reject(command.IdempotencyKey, payloadHash, TransactionFailureCode.StaleRevision, nowUtc);
                next.Items.RemoveAt(sourceIndex);
                removed.Add(preview.SourceItemId);
            }

            next.Revision = current.Revision + 1;
            InventoryDelta delta = Delta(current.Revision, next.Revision, new[] { preview.AfterItem }, removed);
            return Commit(command.OwnerId, current.Revision, command.IdempotencyKey, payloadHash, command.Intent.Operation, next, delta, nowUtc);
        }

        private bool TryReplay(string idempotencyKey, string payloadHash, DateTimeOffset nowUtc, out TransactionReceipt receipt)
        {
            if (!receipts.TryGetReceipt(idempotencyKey, out TransactionReceipt existing))
            {
                receipt = null;
                return false;
            }

            receipt = string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal)
                ? existing
                : Reject(idempotencyKey, payloadHash, TransactionFailureCode.IdempotencyConflict, nowUtc);
            return true;
        }

        private TransactionReceipt Commit(string ownerId, long expectedRevision, string idempotencyKey, string payloadHash, ProgressionOperation operation, InventorySnapshot next, InventoryDelta delta, DateTimeOffset nowUtc)
        {
            string transactionId = ProgressionHash.ComputePayload(string.Join("|", idempotencyKey, payloadHash)).Substring(0, 24);
            string resource = "inventory:" + operation.ToString().ToLowerInvariant();
            var entries = new List<LedgerEntry>
            {
                new LedgerEntry { TransactionId = transactionId, AccountId = ownerId, ResourceId = resource, Amount = 1 },
                new LedgerEntry { TransactionId = transactionId, AccountId = "system", ResourceId = resource, Amount = -1 }
            };
            var receipt = new TransactionReceipt
            {
                TransactionId = transactionId,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                Status = TransactionStatus.Committed,
                FailureCode = TransactionFailureCode.None,
                InventoryDelta = delta,
                LedgerEntries = entries,
                CreatedAt = nowUtc
            };
            return inventory.TryCommit(ownerId, expectedRevision, next, receipt, entries)
                ? receipt
                : Reject(idempotencyKey, payloadHash, TransactionFailureCode.AtomicCommitFailed, nowUtc);
        }

        private static InventoryDelta Delta(long baseRevision, long targetRevision, IEnumerable<OwnedPlayerItem> upserted, IEnumerable<string> removed)
        {
            return new InventoryDelta
            {
                BaseRevision = baseRevision,
                TargetRevision = targetRevision,
                UpsertedItems = (upserted ?? Array.Empty<OwnedPlayerItem>()).Select(item => item.Clone()).ToList(),
                RemovedItemIds = new List<string>(removed ?? Array.Empty<string>())
            };
        }

        private static TransactionReceipt Reject(string idempotencyKey, string payloadHash, TransactionFailureCode code, DateTimeOffset nowUtc)
        {
            return new TransactionReceipt
            {
                IdempotencyKey = idempotencyKey ?? string.Empty,
                PayloadHash = payloadHash ?? string.Empty,
                Status = TransactionStatus.Rejected,
                FailureCode = code,
                CreatedAt = nowUtc
            };
        }
    }
}
