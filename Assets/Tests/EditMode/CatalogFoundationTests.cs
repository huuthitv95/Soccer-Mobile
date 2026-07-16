using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SoccerMobilePro.Catalog;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class CatalogFoundationTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 7, 16, 0, 0, 0, TimeSpan.Zero);
        private NewtonsoftCatalogCodec codec;
        private DefaultCatalogValidator validator;

        [SetUp]
        public void SetUp()
        {
            codec = new NewtonsoftCatalogCodec();
            validator = new DefaultCatalogValidator();
        }

        [Test]
        public void Fixture_ContainsExpectedEntitiesAndValidReferences()
        {
            CatalogSnapshot fixture = CatalogFixtureFactory.Create();

            Assert.That(fixture.Leagues, Has.Count.EqualTo(2));
            Assert.That(fixture.Clubs, Has.Count.EqualTo(4));
            Assert.That(fixture.Players, Has.Count.EqualTo(44));
            Assert.That(fixture.Registrations, Has.Count.EqualTo(44));
            Assert.That(fixture.Models, Has.Count.EqualTo(2));
            Assert.That(validator.ValidateSnapshot(fixture).Succeeded, Is.True);
        }

        [TestCase("202607160001", true)]
        [TestCase("000000000001", true)]
        [TestCase("20260716001", false)]
        [TestCase("20260716A001", false)]
        public void CatalogVersion_RequiresFixedWidthDigits(string value, bool expected)
        {
            Assert.That(CatalogVersion.IsCanonical(value), Is.EqualTo(expected));
        }

        [Test]
        public void Codec_RoundTripsAndIgnoresUnknownFields()
        {
            string payload = codec.SerializeSnapshot(CatalogFixtureFactory.Create());
            string withUnknown = payload.Substring(0, payload.Length - 1) + ",\"futureField\":{\"value\":1}}";

            CatalogSnapshot decoded = codec.DeserializeSnapshot(withUnknown);

            Assert.That(decoded.CatalogVersion, Is.EqualTo(CatalogFixtureFactory.InitialVersion));
            Assert.That(decoded.Players, Has.Count.EqualTo(44));
        }

        [Test]
        public void Validator_RejectsDuplicateAndDanglingReferences()
        {
            CatalogSnapshot snapshot = CatalogFixtureFactory.Create();
            snapshot.Clubs.Add(snapshot.Clubs[0]);
            snapshot.Ratings[0].PlayerId = "missing-player";

            CatalogValidationResult result = validator.ValidateSnapshot(snapshot);

            Assert.That(result.Errors.Any(error => error.Code == CatalogValidationCode.DuplicateId), Is.True);
            Assert.That(result.Errors.Any(error => error.Code == CatalogValidationCode.MissingReference), Is.True);
        }

        [Test]
        public void Validator_RejectsFallbackCycle()
        {
            CatalogSnapshot snapshot = CatalogFixtureFactory.Create();
            snapshot.Models[0].FallbackModelAssetId = CatalogFixtureFactory.MissingModelId;

            CatalogValidationResult result = validator.ValidateSnapshot(snapshot);

            Assert.That(result.Errors.Any(error => error.Code == CatalogValidationCode.FallbackCycle), Is.True);
        }

        [Test]
        public void DeltaApplier_AppliesValidUpsertWithCanonicalHash()
        {
            CatalogSnapshot active = CatalogFixtureFactory.Create();
            CatalogDelta delta = CreateDelta(active.CatalogVersion, "202607160002");
            delta.Upserts.Players.Add(new PlayerIdentity
            {
                PlayerId = active.Players[0].PlayerId,
                DisplayName = "Updated Fixture Player",
                BirthYear = active.Players[0].BirthYear,
                NationalityIds = active.Players[0].NationalityIds,
                PreferredFoot = active.Players[0].PreferredFoot,
                RightsVersion = active.Players[0].RightsVersion,
                ProvenanceId = active.Players[0].ProvenanceId
            });
            delta.PayloadHash = CatalogDeltaIntegrity.ComputePayloadHash(delta, codec);

            CatalogSnapshot target = new CatalogDeltaApplier(codec, validator).Apply(active, delta);

            Assert.That(target.CatalogVersion, Is.EqualTo("202607160002"));
            Assert.That(target.Players.Single(player => player.PlayerId == active.Players[0].PlayerId).DisplayName, Is.EqualTo("Updated Fixture Player"));
        }

        [Test]
        public void DeltaValidator_RejectsBaseMismatchAndCorruptInnerHash()
        {
            CatalogDelta delta = CreateDelta("202607160000", "202607160002");
            delta.PayloadHash = "corrupt";

            CatalogValidationResult result = validator.ValidateDelta(delta, CatalogFixtureFactory.InitialVersion, codec);

            Assert.That(result.Errors.Any(error => error.Code == CatalogValidationCode.InvalidDeltaBase), Is.True);
            Assert.That(result.Errors.Any(error => error.Code == CatalogValidationCode.InvalidDeltaHash), Is.True);
        }

        [Test]
        public void Installer_ActivatesSnapshotAndPreservesLastKnownGoodOnCorruptPayload()
        {
            WithStore((store, installer) =>
            {
                CatalogSnapshot fixture = CatalogFixtureFactory.Create();
                string payload = codec.SerializeSnapshot(fixture);
                CatalogInstallResult installed = installer.InstallSnapshot(CreateManifest(fixture.CatalogVersion, payload), payload, 1, Now);
                CatalogInstallResult corrupt = installer.InstallSnapshot(CreateManifest("202607160002", "expected"), "corrupt", 1, Now);

                Assert.That(installed.Status, Is.EqualTo(CatalogInstallStatus.Installed));
                Assert.That(corrupt.Status, Is.EqualTo(CatalogInstallStatus.InvalidHash));
                Assert.That(store.ActiveVersion, Is.EqualTo(fixture.CatalogVersion));
                Assert.That(store.TryReadActive(out CatalogSnapshot active), Is.True);
                Assert.That(active.Players, Has.Count.EqualTo(44));
            });
        }

        [Test]
        public void Installer_RejectsMissingRollbackTarget()
        {
            WithStore((store, installer) =>
            {
                CatalogSnapshot fixture = CatalogFixtureFactory.Create();
                string payload = codec.SerializeSnapshot(fixture);
                CatalogManifest manifest = CreateManifest(fixture.CatalogVersion, payload, "202607150001");

                CatalogInstallResult result = installer.InstallSnapshot(manifest, payload, 1, Now);

                Assert.That(result.Status, Is.EqualTo(CatalogInstallStatus.MissingRollback));
                Assert.That(store.ActiveVersion, Is.Empty);
            });
        }

        [Test]
        public void Installer_AppliesDeltaThenRollsBackAtomically()
        {
            WithStore((store, installer) =>
            {
                CatalogSnapshot fixture = CatalogFixtureFactory.Create();
                string payload = codec.SerializeSnapshot(fixture);
                Assert.That(installer.InstallSnapshot(CreateManifest(fixture.CatalogVersion, payload), payload, 1, Now).Succeeded, Is.True);

                CatalogDelta delta = CreateDelta(fixture.CatalogVersion, "202607160002");
                delta.Upserts.Clubs.Add(new ClubDefinition
                {
                    ClubId = fixture.Clubs[0].ClubId,
                    LeagueId = fixture.Clubs[0].LeagueId,
                    NameKey = fixture.Clubs[0].NameKey,
                    ShortName = "UPDATED",
                    CrestAddress = fixture.Clubs[0].CrestAddress,
                    RightsVersion = fixture.Clubs[0].RightsVersion,
                    ProvenanceId = fixture.Clubs[0].ProvenanceId
                });
                delta.PayloadHash = CatalogDeltaIntegrity.ComputePayloadHash(delta, codec);
                string deltaPayload = codec.SerializeDelta(delta);
                CatalogManifest targetManifest = CreateManifest(delta.TargetVersion, deltaPayload, fixture.CatalogVersion);

                CatalogInstallResult updated = installer.InstallDelta(targetManifest, deltaPayload, 1, Now);
                CatalogInstallResult rolledBack = installer.Rollback(fixture.CatalogVersion);

                Assert.That(updated.Status, Is.EqualTo(CatalogInstallStatus.Installed));
                Assert.That(rolledBack.Status, Is.EqualTo(CatalogInstallStatus.RolledBack));
                Assert.That(store.ActiveVersion, Is.EqualTo(fixture.CatalogVersion));
            });
        }

        [Test]
        public void FileStore_ReturnsFalseForCorruptActiveSnapshot()
        {
            string root = TemporaryRoot();
            try
            {
                var store = new FileCatalogStore(root, codec, validator);
                CatalogSnapshot fixture = CatalogFixtureFactory.Create();
                store.Stage(fixture);
                store.Activate(fixture.CatalogVersion);
                File.WriteAllText(Path.Combine(root, fixture.CatalogVersion + ".catalog.json"), "{broken");

                Assert.That(store.TryReadActive(out _), Is.False);
            }
            finally { DeleteDirectory(root); }
        }

        [Test]
        public void Installer_AcceptsSchemaNMinusOneAndRejectsNMinusTwo()
        {
            WithStore((store, installer) =>
            {
                CatalogSnapshot fixture = CatalogFixtureFactory.Create();
                string payload = codec.SerializeSnapshot(fixture);
                CatalogManifest nMinusOne = CreateManifest(fixture.CatalogVersion, payload, string.Empty, 1);
                CatalogInstallResult accepted = installer.InstallSnapshot(nMinusOne, payload, 2, Now);

                CatalogSnapshot older = CatalogFixtureFactory.Create();
                older.CatalogVersion = "202607150001";
                string olderPayload = codec.SerializeSnapshot(older);
                CatalogManifest nMinusTwo = CreateManifest(older.CatalogVersion, olderPayload, string.Empty, 1);
                CatalogInstallResult rejected = installer.InstallSnapshot(nMinusTwo, olderPayload, 3, Now);

                Assert.That(accepted.Status, Is.EqualTo(CatalogInstallStatus.Installed));
                Assert.That(rejected.Status, Is.EqualTo(CatalogInstallStatus.InvalidManifest));
                Assert.That(store.ActiveVersion, Is.EqualTo(fixture.CatalogVersion));
            });
        }

        private CatalogDelta CreateDelta(string baseVersion, string targetVersion)
        {
            return new CatalogDelta
            {
                BaseVersion = baseVersion,
                TargetVersion = targetVersion,
                Upserts = new CatalogSnapshot { SchemaVersion = 1, CatalogVersion = targetVersion },
                Removals = new CatalogRemovalSet()
            };
        }

        private static CatalogManifest CreateManifest(string version, string payload, string rollbackVersion = "", int schemaVersion = 1)
        {
            string hash = CatalogIntegrity.ComputeSha256(payload);
            return new CatalogManifest(schemaVersion, version, "fixture", "fixture-2026", Now.AddDays(-1), Now.AddDays(30), "0.1.0", hash, "test-signed:" + hash, rollbackVersion);
        }

        private void WithStore(Action<FileCatalogStore, CatalogInstaller> action)
        {
            string root = TemporaryRoot();
            try
            {
                var store = new FileCatalogStore(root, codec, validator);
                var installer = new CatalogInstaller(store, codec, validator, new FakeCatalogSignatureVerifier());
                action(store, installer);
            }
            finally { DeleteDirectory(root); }
        }

        private static string TemporaryRoot() => Path.Combine(Path.GetTempPath(), "SoccerMobilePro-CatalogTests-" + Guid.NewGuid().ToString("N"));
        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }
}
