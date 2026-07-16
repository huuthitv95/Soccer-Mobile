using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class LocalizationSettingsTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 7, 16, 0, 0, 0, TimeSpan.Zero);

        [Test]
        public void LocaleResolver_UsesAccountThenLocalThenDeviceThenFallback()
        {
            var resolver = new DefaultLocaleResolver();
            var account = new LocalePreference("en", "en", "saved", Now, 3);
            var local = new LocalePreference("vi-VN", "vi-VN", "saved", Now, 2);

            Assert.That(resolver.Resolve(account, local, "vi-VN", Now).Preference.Locale, Is.EqualTo("en"));
            Assert.That(resolver.Resolve(null, local, "en-US", Now).Preference.Locale, Is.EqualTo("vi-VN"));
            Assert.That(resolver.Resolve(null, null, "en", Now).Preference.Source, Is.EqualTo("device-exact"));
            Assert.That(resolver.Resolve(null, null, "en-US", Now).Preference.Source, Is.EqualTo("device-language"));
            Assert.That(resolver.Resolve(null, null, "fr-FR", Now).Preference.Locale, Is.EqualTo("vi-VN"));
        }

        [Test]
        public void DeviceOrFallbackLocale_RequiresFirstLaunchConfirmation()
        {
            var resolver = new DefaultLocaleResolver();

            Assert.That(resolver.Resolve(null, null, "en-US", Now).RequiresConfirmation, Is.True);
            Assert.That(resolver.Resolve(new LocalePreference("en", null, "account", Now, 1), null, "vi-VN", Now).RequiresConfirmation, Is.False);
        }

        [Test]
        public void LocalePreference_PreservesVietnameseTextAndRejectsUnsupportedVoiceFallback()
        {
            var resolver = new DefaultLocaleResolver();
            const string source = "Tùy chọn người chơi";
            var account = new LocalePreference("vi-VN", "ja-JP", source, Now, 1);

            LocaleResolutionResult result = resolver.Resolve(account, null, "en", Now);

            Assert.That(result.Preference.Source, Is.EqualTo("account"));
            Assert.That(result.Preference.VoiceLocale, Is.EqualTo("vi-VN"));
            Assert.That(account.Source, Is.EqualTo(source));
        }

        [Test]
        public void Registry_RejectsDuplicateAndInvalidDefinitions()
        {
            SettingDefinition definition = Boolean("audio.music", SettingScope.Account, true);

            Assert.Throws<ArgumentException>(() => new DefaultSettingsRegistry(new[] { definition, definition }));
            Assert.Throws<ArgumentException>(() => new SettingDefinition("graphics.quality", SettingValueType.Enumeration, SettingScope.Device, "ultra", false, new[] { "low" }));
            Assert.Throws<ArgumentException>(() => new SettingDefinition("device.only", SettingValueType.String, SettingScope.Device, "", true));
        }

        [Test]
        public void ProductRegistry_CoversAllSettingDomainsAndNumericBounds()
        {
            DefaultSettingsRegistry registry = SoccerMobileSettingsRegistry.CreateDefault();

            Assert.That(registry.TryGet("locale.text", out _), Is.True);
            Assert.That(registry.TryGet("locale.confirmed", out SettingDefinition confirmed), Is.True);
            Assert.That(confirmed.CloudSync, Is.True);
            Assert.That(registry.TryGet("audio.music-volume", out SettingDefinition volume), Is.True);
            Assert.That(registry.TryGet("graphics.quality", out _), Is.True);
            Assert.That(registry.TryGet("controls.handedness", out _), Is.True);
            Assert.That(registry.TryGet("accessibility.reduced-motion", out _), Is.True);
            Assert.That(volume.TryNormalize("1.25", out _), Is.False);
        }

        [Test]
        public void Sanitize_RepairsOnlyCorruptFieldsAndPreservesUnknownKeys()
        {
            DefaultSettingsRegistry registry = CreateRegistry();
            var values = new Dictionary<string, string>
            {
                ["audio.music"] = "not-a-boolean",
                ["future.account-key"] = "round-trip"
            };
            var device = new Dictionary<string, string>
            {
                ["graphics.quality"] = "high",
                ["future.device-key"] = "round-trip-device"
            };
            var snapshot = new SettingsSnapshot("account", 2, 4, values, device, Now);

            SettingsSanitizationResult result = registry.Sanitize(snapshot);

            Assert.That(result.Snapshot.Values["audio.music"], Is.EqualTo("true"));
            Assert.That(result.Snapshot.Values["future.account-key"], Is.EqualTo("round-trip"));
            Assert.That(result.Snapshot.DeviceOverrides["graphics.quality"], Is.EqualTo("high"));
            Assert.That(result.Snapshot.DeviceOverrides["future.device-key"], Is.EqualTo("round-trip-device"));
            Assert.That(result.RepairedKeys, Is.EquivalentTo(new[] { "audio.music" }));
        }

        [TestCase(2, true)]
        [TestCase(1, true)]
        [TestCase(3, false)]
        [TestCase(0, false)]
        public void Migrator_AcceptsOnlySchemaNAndNMinusOne(int schema, bool expected)
        {
            var migrator = new VersionedSettingsMigrator(2);
            Assert.That(migrator.CanMigrate(schema), Is.EqualTo(expected));
        }

        [Test]
        public void Migrator_RenamesKnownKeyAndPreservesUnknownKey()
        {
            var migrator = new VersionedSettingsMigrator(2, new Dictionary<string, string> { ["music.enabled"] = "audio.music" });
            var snapshot = Snapshot(1, 1, new Dictionary<string, string> { ["music.enabled"] = "false", ["future"] = "value" });

            SettingsSnapshot migrated = migrator.Migrate(snapshot);

            Assert.That(migrated.SchemaVersion, Is.EqualTo(2));
            Assert.That(migrated.Values["audio.music"], Is.EqualTo("false"));
            Assert.That(migrated.Values["future"], Is.EqualTo("value"));
            Assert.That(migrated.Values.ContainsKey("music.enabled"), Is.False);
        }

        [Test]
        public void Repository_EnforcesTypedValuesRevisionAndReadOnlyPolicy()
        {
            DefaultSettingsRegistry registry = CreateRegistry();
            var repository = new InMemorySettingsRepository(registry, new VersionedSettingsMigrator(2), Snapshot(2, 5));

            Assert.That(repository.TryWrite("audio.music", "invalid", 5, Now), Is.EqualTo(SettingsWriteStatus.InvalidValue));
            Assert.That(repository.TryWrite("match.assist", "off", 5, Now), Is.EqualTo(SettingsWriteStatus.ReadOnly));
            Assert.That(repository.TryWrite("audio.music", "false", 4, Now), Is.EqualTo(SettingsWriteStatus.RevisionConflict));
            Assert.That(repository.TryWrite("audio.music", "false", 5, Now), Is.EqualTo(SettingsWriteStatus.Saved));
            Assert.That(repository.Load().Values["audio.music"], Is.EqualTo("false"));
            Assert.That(repository.Load().Revision, Is.EqualTo(6));
        }

        [Test]
        public void InMemoryBatchWrite_IsAllOrNothingAndUsesOneRevision()
        {
            DefaultSettingsRegistry registry = SoccerMobileSettingsRegistry.CreateDefault();
            var repository = new InMemorySettingsRepository(registry, new VersionedSettingsMigrator(2), Snapshot(2, 8));
            var invalid = new Dictionary<string, string>
            {
                ["locale.text"] = "en",
                ["locale.confirmed"] = "not-boolean"
            };

            Assert.That(repository.TryWriteBatch(invalid, 8, Now), Is.EqualTo(SettingsWriteStatus.InvalidValue));
            Assert.That(repository.Load().Values["locale.text"], Is.EqualTo("vi-VN"));
            Assert.That(repository.Load().Values["locale.confirmed"], Is.EqualTo("false"));
            Assert.That(repository.Load().Revision, Is.EqualTo(8));

            var valid = new Dictionary<string, string> { ["locale.text"] = "en", ["locale.confirmed"] = "true" };
            Assert.That(repository.TryWriteBatch(valid, 8, Now), Is.EqualTo(SettingsWriteStatus.Saved));
            Assert.That(repository.Load().Values["locale.text"], Is.EqualTo("en"));
            Assert.That(repository.Load().Values["locale.confirmed"], Is.EqualTo("true"));
            Assert.That(repository.Load().Revision, Is.EqualTo(9));
        }

        [Test]
        public void MatchPolicy_OverridesSnapshotWithoutBecomingWritable()
        {
            DefaultSettingsRegistry registry = CreateRegistry();
            var persistedPolicy = new Dictionary<string, string> { ["match.assist"] = "off" };
            SettingsSnapshot unsafeSnapshot = Snapshot(2, 1, persistedPolicy);
            SettingsSnapshot snapshot = registry.Sanitize(unsafeSnapshot).Snapshot;
            var policy = new Dictionary<string, string> { ["match.assist"] = "off" };

            Assert.That(SettingsValueResolver.Resolve("match.assist", snapshot, policy, registry), Is.EqualTo("off"));
            Assert.That(snapshot.Values.ContainsKey("match.assist"), Is.False);
            Assert.That(SettingsValueResolver.Resolve("match.assist", unsafeSnapshot, null, registry), Is.EqualTo("balanced"));
            Assert.That(SettingsValueResolver.Resolve("match.assist", unsafeSnapshot, new Dictionary<string, string> { ["match.assist"] = "invalid" }, registry), Is.EqualTo("balanced"));
        }

        [Test]
        public void CloudMerge_UsesAllowlistAndKeepsDeviceSettingsLocal()
        {
            DefaultSettingsRegistry registry = CreateRegistry();
            var local = new SettingsSnapshot(
                "account", 2, 2,
                new Dictionary<string, string> { ["audio.music"] = "true", ["local.private"] = "keep" },
                new Dictionary<string, string> { ["graphics.quality"] = "low" }, Now);
            var remote = new SettingsSnapshot(
                "account", 2, 3,
                new Dictionary<string, string> { ["audio.music"] = "false", ["local.private"] = "replace-attempt" },
                new Dictionary<string, string> { ["graphics.quality"] = "high" }, Now.AddMinutes(1));

            SettingsMergeResult result = new SettingsCloudMerger(registry).Merge(local, remote);

            Assert.That(result.HadConflict, Is.True);
            Assert.That(result.Snapshot.Values["audio.music"], Is.EqualTo("false"));
            Assert.That(result.Snapshot.Values["local.private"], Is.EqualTo("keep"));
            Assert.That(result.Snapshot.DeviceOverrides["graphics.quality"], Is.EqualTo("low"));
        }

        [Test]
        public void CloudMerge_RejectsAccountAndUnsupportedSchemaBoundaries()
        {
            DefaultSettingsRegistry registry = CreateRegistry();
            var merger = new SettingsCloudMerger(registry);
            SettingsSnapshot local = Snapshot(2, 1);
            var otherAccount = new SettingsSnapshot("other", 2, 1, null, null, Now);
            var futureSchema = new SettingsSnapshot(local.AccountId, 4, 1, null, null, Now);

            Assert.Throws<InvalidOperationException>(() => merger.Merge(local, otherAccount));
            Assert.Throws<InvalidOperationException>(() => merger.Merge(local, futureSchema));
        }

        [Test]
        public void FileRepository_RoundTripsDeterministicSnapshot()
        {
            WithTemporarySettingsPath(path =>
            {
                DefaultSettingsRegistry registry = CreateRegistry();
                var initial = Snapshot(2, 0);
                var first = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), initial);

                Assert.That(first.TryWrite("audio.music", "false", 0, Now), Is.EqualTo(SettingsWriteStatus.Saved));
                var second = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), initial);

                Assert.That(second.Load().Values["audio.music"], Is.EqualTo("false"));
                Assert.That(second.Load().Revision, Is.EqualTo(1));
                Assert.That(File.ReadAllText(path).StartsWith("SMPSETTINGS1\n", StringComparison.Ordinal), Is.True);
            });
        }

        [Test]
        public void FileRepository_CorruptFileFallsBackToSafeInitialSnapshot()
        {
            WithTemporarySettingsPath(path =>
            {
                File.WriteAllText(path, "not-settings");
                DefaultSettingsRegistry registry = CreateRegistry();
                var safeValues = new Dictionary<string, string> { ["audio.music"] = "false", ["safe.unknown"] = "kept" };

                var repository = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), Snapshot(2, 7, safeValues));

                Assert.That(repository.Load().Revision, Is.EqualTo(7));
                Assert.That(repository.Load().Values["audio.music"], Is.EqualTo("false"));
                Assert.That(repository.Load().Values["safe.unknown"], Is.EqualTo("kept"));
            });
        }

        [Test]
        public void FileRepository_CorruptPrimaryRecoversLastAtomicBackup()
        {
            WithTemporarySettingsPath(path =>
            {
                DefaultSettingsRegistry registry = CreateRegistry();
                var initial = Snapshot(2, 0);
                var writer = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), initial);
                Assert.That(writer.TryWrite("audio.music", "false", 0, Now), Is.EqualTo(SettingsWriteStatus.Saved));
                Assert.That(writer.TryWrite("audio.music", "true", 1, Now.AddMinutes(1)), Is.EqualTo(SettingsWriteStatus.Saved));
                Assert.That(File.Exists(path + ".bak"), Is.True);
                File.WriteAllText(path, "corrupt-primary");
                File.WriteAllText(path + ".tmp", "stale-temporary");

                var recovered = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), initial);

                Assert.That(recovered.Load().Revision, Is.EqualTo(1));
                Assert.That(recovered.Load().Values["audio.music"], Is.EqualTo("false"));
                Assert.That(File.Exists(path + ".tmp"), Is.False);
            });
        }

        [Test]
        public void FileRepository_SanitizesOnlyInvalidStoredField()
        {
            WithTemporarySettingsPath(path =>
            {
                DefaultSettingsRegistry registry = CreateRegistry();
                var writer = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), Snapshot(2, 0));
                Assert.That(writer.TryWrite("audio.music", "false", 0, Now), Is.EqualTo(SettingsWriteStatus.Saved));
                string serialized = File.ReadAllText(path);
                string encodedFalse = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("false"));
                string encodedInvalid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("invalid"));
                File.WriteAllText(path, serialized.Replace(encodedFalse, encodedInvalid));

                var reader = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), Snapshot(2, 0));

                Assert.That(reader.Load().Values["audio.music"], Is.EqualTo("true"));
                Assert.That(reader.Load().Revision, Is.EqualTo(1));
                Assert.That(reader.Load().Values.ContainsKey("match.assist"), Is.False);
            });
        }

        [Test]
        public void FileRepository_BatchWriteIsAllOrNothingAndPersistsOnce()
        {
            WithTemporarySettingsPath(path =>
            {
                DefaultSettingsRegistry registry = SoccerMobileSettingsRegistry.CreateDefault();
                var unsafeInitial = Snapshot(2, 0, new Dictionary<string, string> { ["match.assist"] = "off" });
                var repository = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), unsafeInitial);
                var invalid = new Dictionary<string, string> { ["locale.text"] = "en", ["locale.confirmed"] = "invalid" };

                Assert.That(repository.TryWriteBatch(invalid, 0, Now), Is.EqualTo(SettingsWriteStatus.InvalidValue));
                Assert.That(File.Exists(path), Is.False);
                Assert.That(repository.Load().Revision, Is.EqualTo(0));
                Assert.That(repository.Load().Values.ContainsKey("match.assist"), Is.False);

                var valid = new Dictionary<string, string> { ["locale.text"] = "en", ["locale.confirmed"] = "true" };
                Assert.That(repository.TryWriteBatch(valid, 0, Now), Is.EqualTo(SettingsWriteStatus.Saved));
                var reloaded = new FileSettingsRepository(path, registry, new VersionedSettingsMigrator(2), Snapshot(2, 0)).Load();
                Assert.That(reloaded.Values["locale.text"], Is.EqualTo("en"));
                Assert.That(reloaded.Values["locale.confirmed"], Is.EqualTo("true"));
                Assert.That(reloaded.Values.ContainsKey("match.assist"), Is.False);
                Assert.That(reloaded.Revision, Is.EqualTo(1));
            });
        }

        private static DefaultSettingsRegistry CreateRegistry()
        {
            return new DefaultSettingsRegistry(new[]
            {
                Boolean("audio.music", SettingScope.Account, true),
                new SettingDefinition("graphics.quality", SettingValueType.Enumeration, SettingScope.Device, "low", false, new[] { "low", "high" }),
                new SettingDefinition("match.assist", SettingValueType.Enumeration, SettingScope.MatchPolicy, "balanced", false, new[] { "off", "balanced" })
            });
        }

        private static SettingDefinition Boolean(string key, SettingScope scope, bool defaultValue)
        {
            return new SettingDefinition(key, SettingValueType.Boolean, scope, defaultValue ? "true" : "false", scope == SettingScope.Account);
        }

        private static SettingsSnapshot Snapshot(int schema, long revision, IReadOnlyDictionary<string, string> values = null)
        {
            return new SettingsSnapshot("account", schema, revision, values, null, Now);
        }

        private static void WithTemporarySettingsPath(Action<string> assertion)
        {
            string directory = Path.Combine(Path.GetTempPath(), "SoccerMobilePro-SettingsTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                assertion(Path.Combine(directory, "settings.dat"));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
