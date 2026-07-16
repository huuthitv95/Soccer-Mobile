using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoccerMobilePro.Competition
{
    public sealed class CompetitionEnvelope
    {
        public int SchemaVersion { get; set; } = CompetitionCodec.CurrentSchemaVersion;
        public List<TournamentSnapshot> Tournaments { get; set; } = new List<TournamentSnapshot>();
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);
    }

    public sealed class CompetitionCodec
    {
        public const int CurrentSchemaVersion = 2;
        public const int BackwardSchemaWindow = 1;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented, MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include, DateParseHandling = DateParseHandling.DateTimeOffset,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public string Serialize(CompetitionEnvelope envelope) => JsonConvert.SerializeObject(envelope ?? throw new ArgumentNullException(nameof(envelope)), Settings);

        public CompetitionEnvelope Deserialize(string payload, out bool migrated)
        {
            if (string.IsNullOrWhiteSpace(payload)) throw new CompetitionPersistenceException("Competition payload is empty.");
            CompetitionEnvelope envelope;
            try { envelope = JsonConvert.DeserializeObject<CompetitionEnvelope>(payload, Settings) ?? throw new CompetitionPersistenceException("Competition payload decoded to null."); }
            catch (JsonException exception) { throw new CompetitionPersistenceException("Competition payload is invalid JSON.", exception); }
            if (envelope.SchemaVersion > CurrentSchemaVersion || envelope.SchemaVersion < CurrentSchemaVersion - BackwardSchemaWindow)
                throw new CompetitionPersistenceException("Competition schema is outside the N/N-1 compatibility window.");
            migrated = envelope.SchemaVersion == CurrentSchemaVersion - 1;
            if (migrated) envelope.SchemaVersion = CurrentSchemaVersion;
            envelope.Tournaments = envelope.Tournaments ?? new List<TournamentSnapshot>();
            foreach (TournamentSnapshot snapshot in envelope.Tournaments)
            {
                snapshot.SchemaVersion = CurrentSchemaVersion;
                snapshot.Rosters = snapshot.Rosters ?? new List<RosterSubmission>();
                snapshot.Matches = snapshot.Matches ?? new List<TournamentMatch>();
                snapshot.Standings = snapshot.Standings ?? new List<StandingEntry>();
                snapshot.Receipts = snapshot.Receipts ?? new List<AuthoritativeResultReceipt>();
                snapshot.ReconnectTimelines = snapshot.ReconnectTimelines ?? new List<ReconnectTimeline>();
                snapshot.Disputes = snapshot.Disputes ?? new List<DisputeRecord>();
            }
            return envelope;
        }
    }

    public sealed class FileCompetitionRepository : ICompetitionRepository
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly object sync = new object();
        private readonly string path;
        private readonly string backupPath;
        private readonly CompetitionCodec codec;
        private CompetitionEnvelope envelope;

        public FileCompetitionRepository(string path, CompetitionCodec codec = null, bool forceReadOnly = false)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Competition path is required.", nameof(path));
            this.path = Path.GetFullPath(path);
            backupPath = this.path + ".bak";
            this.codec = codec ?? new CompetitionCodec();
            IsReadOnly = forceReadOnly;
            envelope = LoadInitial();
        }

        public bool IsReadOnly { get; private set; }
        public bool LoadedFromBackup { get; private set; }
        public bool MigratedFromPreviousSchema { get; private set; }

        public bool TryLoad(string tournamentId, out TournamentSnapshot snapshot)
        {
            lock (sync)
            {
                TournamentSnapshot stored = envelope.Tournaments.FirstOrDefault(item => item.TournamentId == tournamentId);
                snapshot = stored == null ? null : CompetitionClone.Snapshot(stored);
                return snapshot != null;
            }
        }

        public bool TryCommit(string tournamentId, long expectedRevision, TournamentSnapshot next)
        {
            if (IsReadOnly || next == null) return false;
            lock (sync)
            {
                TournamentSnapshot current = envelope.Tournaments.FirstOrDefault(item => item.TournamentId == tournamentId);
                if ((current?.Revision ?? -1L) != expectedRevision) return false;
                CompetitionEnvelope nextEnvelope = Clone(envelope);
                int index = nextEnvelope.Tournaments.FindIndex(item => item.TournamentId == tournamentId);
                if (index >= 0) nextEnvelope.Tournaments[index] = CompetitionClone.Snapshot(next);
                else nextEnvelope.Tournaments.Add(CompetitionClone.Snapshot(next));
                if (!Persist(nextEnvelope)) return false;
                envelope = nextEnvelope;
                return true;
            }
        }

        private CompetitionEnvelope LoadInitial()
        {
            if (!File.Exists(path)) return new CompetitionEnvelope();
            if (TryRead(path, out CompetitionEnvelope active, out bool migrated)) { MigratedFromPreviousSchema = migrated; return active; }
            IsReadOnly = true;
            if (File.Exists(backupPath) && TryRead(backupPath, out CompetitionEnvelope backup, out bool backupMigrated))
            { LoadedFromBackup = true; MigratedFromPreviousSchema = backupMigrated; return backup; }
            return new CompetitionEnvelope();
        }

        private bool TryRead(string candidate, out CompetitionEnvelope result, out bool migrated)
        {
            try { result = codec.Deserialize(File.ReadAllText(candidate, Utf8), out migrated); return true; }
            catch { result = null; migrated = false; return false; }
        }

        private bool Persist(CompetitionEnvelope value)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string staging = path + ".tmp";
            try
            {
                File.WriteAllText(staging, codec.Serialize(value), Utf8);
                codec.Deserialize(File.ReadAllText(staging, Utf8), out _);
                if (File.Exists(path)) File.Replace(staging, path, backupPath, true); else File.Move(staging, path);
                return true;
            }
            catch { if (File.Exists(staging)) File.Delete(staging); return false; }
        }

        private CompetitionEnvelope Clone(CompetitionEnvelope source)
        {
            return codec.Deserialize(codec.Serialize(source), out _);
        }
    }

    public sealed class CompetitionPersistenceException : Exception
    {
        public CompetitionPersistenceException(string message) : base(message) { }
        public CompetitionPersistenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
