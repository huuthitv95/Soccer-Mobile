using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SoccerMobilePro.Platform
{
    public sealed class FileSettingsRepository : ISettingsRepository
    {
        private const string Header = "SMPSETTINGS1";
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly string path;
        private readonly ISettingsRegistry registry;
        private SettingsSnapshot snapshot;

        public FileSettingsRepository(
            string path,
            ISettingsRegistry registry,
            ISettingsMigrator migrator,
            SettingsSnapshot safeInitialSnapshot)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Settings path is required.", nameof(path));
            this.path = Path.GetFullPath(path);
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            if (migrator == null) throw new ArgumentNullException(nameof(migrator));
            if (safeInitialSnapshot == null) throw new ArgumentNullException(nameof(safeInitialSnapshot));

            SettingsSnapshot safe = Prepare(safeInitialSnapshot, migrator);
            snapshot = TryLoadCandidate(this.path, migrator, out SettingsSnapshot stored) ||
                TryLoadCandidate(this.path + ".bak", migrator, out stored)
                ? Prepare(stored, migrator)
                : safe;
            TryDeleteStaleTemporaryFile(this.path + ".tmp");
        }

        public SettingsSnapshot Load() => snapshot;

        public SettingsWriteStatus TryWrite(string key, string value, long expectedRevision, DateTimeOffset nowUtc)
        {
            return TryWriteBatch(new Dictionary<string, string> { [key] = value }, expectedRevision, nowUtc);
        }

        public SettingsWriteStatus TryWriteBatch(IReadOnlyDictionary<string, string> updates, long expectedRevision, DateTimeOffset nowUtc)
        {
            if (updates == null || updates.Count == 0) return SettingsWriteStatus.InvalidKey;
            if (snapshot.Revision != expectedRevision) return SettingsWriteStatus.RevisionConflict;

            var normalizedUpdates = new List<KeyValuePair<SettingDefinition, string>>(updates.Count);
            foreach (KeyValuePair<string, string> update in updates)
            {
                if (!registry.TryGet(update.Key, out SettingDefinition definition)) return SettingsWriteStatus.InvalidKey;
                if (definition.IsReadOnly) return SettingsWriteStatus.ReadOnly;
                if (!definition.TryNormalize(update.Value, out string normalized)) return SettingsWriteStatus.InvalidValue;
                normalizedUpdates.Add(new KeyValuePair<SettingDefinition, string>(definition, normalized));
            }

            var values = new Dictionary<string, string>(snapshot.Values, StringComparer.Ordinal);
            var device = new Dictionary<string, string>(snapshot.DeviceOverrides, StringComparer.Ordinal);
            foreach (KeyValuePair<SettingDefinition, string> update in normalizedUpdates)
                (update.Key.Scope == SettingScope.Device ? device : values)[update.Key.Key] = update.Value;
            var next = new SettingsSnapshot(snapshot.AccountId, snapshot.SchemaVersion, snapshot.Revision + 1, values, device, nowUtc);
            WriteAtomically(next);
            snapshot = next;
            return SettingsWriteStatus.Saved;
        }

        private SettingsSnapshot Prepare(SettingsSnapshot candidate, ISettingsMigrator migrator)
        {
            return registry.Sanitize(migrator.Migrate(candidate)).Snapshot;
        }

        private static bool TryLoadCandidate(string candidatePath, ISettingsMigrator migrator, out SettingsSnapshot stored)
        {
            return TryRead(candidatePath, out stored) && migrator.CanMigrate(stored.SchemaVersion);
        }

        private static void TryDeleteStaleTemporaryFile(string temporaryPath)
        {
            try
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
            catch (IOException)
            {
                // A locked stale file is harmless; the next successful write replaces it.
            }
            catch (UnauthorizedAccessException)
            {
                // Read access remains available even if cleanup is denied.
            }
        }

        private void WriteAtomically(SettingsSnapshot value)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string temporaryPath = path + ".tmp";
            string backupPath = path + ".bak";
            File.WriteAllText(temporaryPath, Serialize(value), Utf8);

            if (!File.Exists(path))
            {
                File.Move(temporaryPath, path);
                return;
            }

            try
            {
                File.Replace(temporaryPath, path, backupPath, true);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceWithBackupFallback(temporaryPath, backupPath);
            }
            catch (IOException)
            {
                ReplaceWithBackupFallback(temporaryPath, backupPath);
            }
        }

        private void ReplaceWithBackupFallback(string temporaryPath, string backupPath)
        {
            File.Copy(path, backupPath, true);
            File.Delete(path);
            File.Move(temporaryPath, path);
        }

        private static string Serialize(SettingsSnapshot value)
        {
            var lines = new List<string>
            {
                Header,
                "schema\t" + value.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                "account\t" + Encode(value.AccountId),
                "revision\t" + value.Revision.ToString(CultureInfo.InvariantCulture),
                "updated\t" + value.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture)
            };
            foreach (KeyValuePair<string, string> item in value.Values.OrderBy(item => item.Key, StringComparer.Ordinal))
                lines.Add("value\t" + Encode(item.Key) + "\t" + Encode(item.Value));
            foreach (KeyValuePair<string, string> item in value.DeviceOverrides.OrderBy(item => item.Key, StringComparer.Ordinal))
                lines.Add("device\t" + Encode(item.Key) + "\t" + Encode(item.Value));
            return string.Join("\n", lines) + "\n";
        }

        private static bool TryRead(string sourcePath, out SettingsSnapshot result)
        {
            result = null;
            if (!File.Exists(sourcePath)) return false;
            try
            {
                string[] lines = File.ReadAllLines(sourcePath, Utf8);
                if (lines.Length < 5 || lines[0] != Header) return false;
                if (!TryScalar(lines[1], "schema", out string schemaText) ||
                    !int.TryParse(schemaText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int schema) || schema < 1) return false;
                if (!TryScalar(lines[2], "account", out string accountText) || !TryDecode(accountText, out string account)) return false;
                if (!TryScalar(lines[3], "revision", out string revisionText) ||
                    !long.TryParse(revisionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long revision) || revision < 0) return false;
                if (!TryScalar(lines[4], "updated", out string updatedText) ||
                    !DateTimeOffset.TryParseExact(updatedText, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset updated)) return false;

                var values = new Dictionary<string, string>(StringComparer.Ordinal);
                var device = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int index = 5; index < lines.Length; index++)
                {
                    if (string.IsNullOrEmpty(lines[index])) continue;
                    string[] fields = lines[index].Split('\t');
                    if (fields.Length != 3 || !TryDecode(fields[1], out string key) || !TryDecode(fields[2], out string settingValue) || string.IsNullOrEmpty(key)) return false;
                    IDictionary<string, string> target = fields[0] == "value" ? values : fields[0] == "device" ? device : null;
                    if (target == null || target.ContainsKey(key)) return false;
                    target.Add(key, settingValue);
                }

                result = new SettingsSnapshot(account, schema, revision, values, device, updated);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is FormatException || exception is DecoderFallbackException)
            {
                return false;
            }
        }

        private static bool TryScalar(string line, string expectedKey, out string value)
        {
            string[] fields = line.Split('\t');
            value = fields.Length == 2 ? fields[1] : null;
            return fields.Length == 2 && fields[0] == expectedKey;
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Utf8.GetBytes(value ?? string.Empty));
        }

        private static bool TryDecode(string value, out string decoded)
        {
            try
            {
                decoded = Utf8.GetString(Convert.FromBase64String(value));
                return true;
            }
            catch (Exception exception) when (exception is FormatException || exception is DecoderFallbackException)
            {
                decoded = null;
                return false;
            }
        }
    }
}
