using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SoccerMobilePro.Platform
{
    public enum SettingValueType
    {
        Boolean = 0,
        Integer = 1,
        Decimal = 2,
        String = 3,
        Enumeration = 4
    }

    public enum SettingScope
    {
        Account = 0,
        Device = 1,
        MatchPolicy = 2
    }

    public sealed class SettingDefinition
    {
        private readonly HashSet<string> allowedValues;

        public SettingDefinition(
            string key,
            SettingValueType valueType,
            SettingScope scope,
            string defaultValue,
            bool cloudSync,
            IEnumerable<string> allowedValues = null,
            decimal? minimum = null,
            decimal? maximum = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Setting key is required.", nameof(key));
            if (cloudSync && scope != SettingScope.Account) throw new ArgumentException("Only account settings may cloud sync.", nameof(cloudSync));

            Key = key;
            ValueType = valueType;
            Scope = scope;
            CloudSync = cloudSync;
            Minimum = minimum;
            Maximum = maximum;
            if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
                throw new ArgumentException("Minimum cannot exceed maximum.", nameof(minimum));
            this.allowedValues = allowedValues == null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(allowedValues, StringComparer.Ordinal);
            if (!TryNormalize(defaultValue, out string normalizedDefault))
                throw new ArgumentException("Default value does not satisfy its definition.", nameof(defaultValue));
            DefaultValue = normalizedDefault;
        }

        public string Key { get; }
        public SettingValueType ValueType { get; }
        public SettingScope Scope { get; }
        public string DefaultValue { get; }
        public bool CloudSync { get; }
        public decimal? Minimum { get; }
        public decimal? Maximum { get; }
        public bool IsReadOnly => Scope == SettingScope.MatchPolicy;

        public bool TryNormalize(string value, out string normalized)
        {
            normalized = null;
            switch (ValueType)
            {
                case SettingValueType.Boolean:
                    if (!bool.TryParse(value, out bool boolean)) return false;
                    normalized = boolean ? "true" : "false";
                    return true;
                case SettingValueType.Integer:
                    if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer)) return false;
                    if (!IsWithinRange(integer)) return false;
                    normalized = integer.ToString(CultureInfo.InvariantCulture);
                    return true;
                case SettingValueType.Decimal:
                    if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal number)) return false;
                    if (!IsWithinRange(number)) return false;
                    normalized = number.ToString(CultureInfo.InvariantCulture);
                    return true;
                case SettingValueType.String:
                    normalized = value ?? string.Empty;
                    return true;
                case SettingValueType.Enumeration:
                    if (value == null || !allowedValues.Contains(value)) return false;
                    normalized = value;
                    return true;
                default:
                    return false;
            }
        }

        private bool IsWithinRange(decimal value)
        {
            return (!Minimum.HasValue || value >= Minimum.Value) && (!Maximum.HasValue || value <= Maximum.Value);
        }
    }

    public interface ISettingsRegistry
    {
        IEnumerable<SettingDefinition> Definitions { get; }
        bool TryGet(string key, out SettingDefinition definition);
        SettingsSanitizationResult Sanitize(SettingsSnapshot snapshot);
    }

    public sealed class SettingsSanitizationResult
    {
        public SettingsSanitizationResult(SettingsSnapshot snapshot, IEnumerable<string> repairedKeys)
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            RepairedKeys = new ReadOnlyCollection<string>(new List<string>(repairedKeys ?? Array.Empty<string>()));
        }

        public SettingsSnapshot Snapshot { get; }
        public IReadOnlyList<string> RepairedKeys { get; }
    }

    public sealed class DefaultSettingsRegistry : ISettingsRegistry
    {
        private readonly Dictionary<string, SettingDefinition> definitions;

        public DefaultSettingsRegistry(IEnumerable<SettingDefinition> definitions)
        {
            this.definitions = new Dictionary<string, SettingDefinition>(StringComparer.Ordinal);
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            foreach (SettingDefinition definition in definitions)
            {
                if (definition == null) throw new ArgumentException("Definitions cannot contain null.", nameof(definitions));
                if (this.definitions.ContainsKey(definition.Key)) throw new ArgumentException("Duplicate setting key: " + definition.Key, nameof(definitions));
                this.definitions.Add(definition.Key, definition);
            }
        }

        public IEnumerable<SettingDefinition> Definitions => definitions.Values;
        public bool TryGet(string key, out SettingDefinition definition) => definitions.TryGetValue(key, out definition);

        public SettingsSanitizationResult Sanitize(SettingsSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var values = new Dictionary<string, string>(snapshot.Values, StringComparer.Ordinal);
            var device = new Dictionary<string, string>(snapshot.DeviceOverrides, StringComparer.Ordinal);
            var repaired = new List<string>();

            foreach (SettingDefinition definition in definitions.Values)
            {
                if (definition.Scope == SettingScope.MatchPolicy)
                {
                    bool removed = values.Remove(definition.Key) | device.Remove(definition.Key);
                    if (removed) repaired.Add(definition.Key);
                    continue;
                }
                IDictionary<string, string> target = definition.Scope == SettingScope.Device ? device : values;
                if (!target.TryGetValue(definition.Key, out string raw) || !definition.TryNormalize(raw, out string normalized))
                {
                    target[definition.Key] = definition.DefaultValue;
                    repaired.Add(definition.Key);
                }
                else
                {
                    target[definition.Key] = normalized;
                }
            }

            var sanitized = new SettingsSnapshot(snapshot.AccountId, snapshot.SchemaVersion, snapshot.Revision, values, device, snapshot.UpdatedAtUtc);
            return new SettingsSanitizationResult(sanitized, repaired);
        }
    }

    public static class SoccerMobileSettingsRegistry
    {
        public static DefaultSettingsRegistry CreateDefault()
        {
            return new DefaultSettingsRegistry(new[]
            {
                new SettingDefinition("locale.text", SettingValueType.Enumeration, SettingScope.Account, "vi-VN", true, new[] { "vi-VN", "en" }),
                new SettingDefinition("locale.confirmed", SettingValueType.Boolean, SettingScope.Account, "false", true),
                new SettingDefinition("audio.music-volume", SettingValueType.Decimal, SettingScope.Account, "1", true, null, 0m, 1m),
                new SettingDefinition("audio.sfx-volume", SettingValueType.Decimal, SettingScope.Account, "1", true, null, 0m, 1m),
                new SettingDefinition("graphics.quality", SettingValueType.Enumeration, SettingScope.Device, "auto", false, new[] { "low", "medium", "high", "auto" }),
                new SettingDefinition("controls.handedness", SettingValueType.Enumeration, SettingScope.Account, "right", true, new[] { "left", "right" }),
                new SettingDefinition("controls.scale", SettingValueType.Decimal, SettingScope.Device, "1", false, null, 0.75m, 1.5m),
                new SettingDefinition("controls.opacity", SettingValueType.Decimal, SettingScope.Device, "1", false, null, 0.25m, 1m),
                new SettingDefinition("controls.dead-zone", SettingValueType.Decimal, SettingScope.Device, "0.15", false, null, 0m, 0.5m),
                new SettingDefinition("accessibility.reduced-motion", SettingValueType.Boolean, SettingScope.Account, "false", true),
                new SettingDefinition("accessibility.high-contrast", SettingValueType.Boolean, SettingScope.Account, "false", true),
                new SettingDefinition("match.assist", SettingValueType.Enumeration, SettingScope.MatchPolicy, "balanced", false, new[] { "off", "balanced", "assisted" })
            });
        }
    }

    public interface ISettingsMigrator
    {
        int CurrentSchemaVersion { get; }
        bool CanMigrate(int schemaVersion);
        SettingsSnapshot Migrate(SettingsSnapshot snapshot);
    }

    public sealed class VersionedSettingsMigrator : ISettingsMigrator
    {
        private readonly IReadOnlyDictionary<string, string> versionOneRenames;

        public VersionedSettingsMigrator(int currentSchemaVersion, IReadOnlyDictionary<string, string> versionOneRenames = null)
        {
            if (currentSchemaVersion < 2) throw new ArgumentOutOfRangeException(nameof(currentSchemaVersion));
            CurrentSchemaVersion = currentSchemaVersion;
            versionOneRenames = versionOneRenames ?? new Dictionary<string, string>();
            this.versionOneRenames = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(versionOneRenames, StringComparer.Ordinal));
        }

        public int CurrentSchemaVersion { get; }
        public bool CanMigrate(int schemaVersion) => schemaVersion == CurrentSchemaVersion || schemaVersion == CurrentSchemaVersion - 1;

        public SettingsSnapshot Migrate(SettingsSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (!CanMigrate(snapshot.SchemaVersion)) throw new InvalidOperationException("Only schema N and N-1 are supported.");
            if (snapshot.SchemaVersion == CurrentSchemaVersion) return snapshot;

            var values = Rename(snapshot.Values);
            var device = Rename(snapshot.DeviceOverrides);
            return new SettingsSnapshot(snapshot.AccountId, CurrentSchemaVersion, snapshot.Revision, values, device, snapshot.UpdatedAtUtc);
        }

        private Dictionary<string, string> Rename(IReadOnlyDictionary<string, string> source)
        {
            var result = new Dictionary<string, string>(source, StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> rename in versionOneRenames)
            {
                if (!result.TryGetValue(rename.Key, out string value)) continue;
                if (!result.ContainsKey(rename.Value)) result[rename.Value] = value;
                result.Remove(rename.Key);
            }
            return result;
        }
    }

    public enum SettingsWriteStatus
    {
        Saved = 0,
        InvalidKey = 1,
        InvalidValue = 2,
        ReadOnly = 3,
        RevisionConflict = 4
    }

    public interface ISettingsRepository
    {
        SettingsSnapshot Load();
        SettingsWriteStatus TryWrite(string key, string value, long expectedRevision, DateTimeOffset nowUtc);
        SettingsWriteStatus TryWriteBatch(IReadOnlyDictionary<string, string> updates, long expectedRevision, DateTimeOffset nowUtc);
    }

    public sealed class InMemorySettingsRepository : ISettingsRepository
    {
        private readonly ISettingsRegistry registry;
        private SettingsSnapshot snapshot;

        public InMemorySettingsRepository(ISettingsRegistry registry, ISettingsMigrator migrator, SettingsSnapshot initial)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            if (migrator == null) throw new ArgumentNullException(nameof(migrator));
            snapshot = registry.Sanitize(migrator.Migrate(initial ?? throw new ArgumentNullException(nameof(initial)))).Snapshot;
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
            snapshot = new SettingsSnapshot(snapshot.AccountId, snapshot.SchemaVersion, snapshot.Revision + 1, values, device, nowUtc);
            return SettingsWriteStatus.Saved;
        }
    }

    public readonly struct SettingsMergeResult
    {
        public SettingsMergeResult(SettingsSnapshot snapshot, bool hadConflict)
        {
            Snapshot = snapshot;
            HadConflict = hadConflict;
        }

        public SettingsSnapshot Snapshot { get; }
        public bool HadConflict { get; }
    }

    public sealed class SettingsCloudMerger
    {
        private readonly ISettingsRegistry registry;

        public SettingsCloudMerger(ISettingsRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public SettingsMergeResult Merge(SettingsSnapshot local, SettingsSnapshot remote)
        {
            if (local == null) throw new ArgumentNullException(nameof(local));
            if (remote == null) throw new ArgumentNullException(nameof(remote));
            if (!string.Equals(local.AccountId, remote.AccountId, StringComparison.Ordinal))
                throw new InvalidOperationException("Cloud settings snapshots must belong to the same account.");
            if (Math.Abs(local.SchemaVersion - remote.SchemaVersion) > 1)
                throw new InvalidOperationException("Cloud settings merge only supports schema N and N-1.");
            bool conflict = local.Revision != remote.Revision;
            SettingsSnapshot winner = remote.Revision > local.Revision ||
                (remote.Revision == local.Revision && remote.UpdatedAtUtc > local.UpdatedAtUtc) ? remote : local;
            var values = new Dictionary<string, string>(local.Values, StringComparer.Ordinal);
            var device = new Dictionary<string, string>(local.DeviceOverrides, StringComparer.Ordinal);

            foreach (SettingDefinition definition in registry.Definitions)
            {
                if (definition.Scope == SettingScope.MatchPolicy)
                {
                    values.Remove(definition.Key);
                    device.Remove(definition.Key);
                }
            }

            foreach (SettingDefinition definition in registry.Definitions)
            {
                if (!definition.CloudSync) continue;
                if (winner.Values.TryGetValue(definition.Key, out string value) && definition.TryNormalize(value, out string normalized))
                    values[definition.Key] = normalized;
                else
                    values[definition.Key] = definition.DefaultValue;
            }

            var merged = new SettingsSnapshot(local.AccountId, Math.Max(local.SchemaVersion, remote.SchemaVersion), Math.Max(local.Revision, remote.Revision), values, device, winner.UpdatedAtUtc);
            return new SettingsMergeResult(merged, conflict);
        }
    }

    public static class SettingsValueResolver
    {
        public static string Resolve(string key, SettingsSnapshot snapshot, IReadOnlyDictionary<string, string> matchPolicy, ISettingsRegistry registry)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (!registry.TryGet(key, out SettingDefinition definition)) throw new KeyNotFoundException(key);

            if (definition.Scope == SettingScope.MatchPolicy && matchPolicy != null &&
                matchPolicy.TryGetValue(key, out string policyValue) && definition.TryNormalize(policyValue, out string normalizedPolicy))
                return normalizedPolicy;

            if (definition.Scope == SettingScope.MatchPolicy) return definition.DefaultValue;

            IReadOnlyDictionary<string, string> source = definition.Scope == SettingScope.Device ? snapshot.DeviceOverrides : snapshot.Values;
            return source.TryGetValue(key, out string value) && definition.TryNormalize(value, out string normalized)
                ? normalized
                : definition.DefaultValue;
        }
    }
}
