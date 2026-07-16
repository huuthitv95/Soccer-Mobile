using System;
using System.IO;
using NUnit.Framework;
using SoccerMobilePro.Competition;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class CompetitionPersistenceTests
    {
        private string directory;
        private string path;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "soccer-mobile-competition", Guid.NewGuid().ToString("N"));
            path = Path.Combine(directory, "competitions.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void Codec_RoundTripsUnknownFields()
        {
            var codec = new CompetitionCodec();
            CompetitionEnvelope envelope = codec.Deserialize("{\"schemaVersion\":2,\"tournaments\":[],\"futurePolicy\":{\"value\":7}}", out bool migrated);
            string serialized = codec.Serialize(envelope);
            Assert.That(migrated, Is.False);
            Assert.That(serialized, Does.Contain("futurePolicy"));
            Assert.That(serialized, Does.Contain("\"value\": 7"));
        }

        [Test]
        public void Codec_MigratesNMinusOneAndRejectsNMinusTwo()
        {
            var codec = new CompetitionCodec();
            CompetitionEnvelope envelope = codec.Deserialize("{\"schemaVersion\":1}", out bool migrated);
            Assert.That(migrated, Is.True);
            Assert.That(envelope.SchemaVersion, Is.EqualTo(2));
            Assert.Throws<CompetitionPersistenceException>(() => codec.Deserialize("{\"schemaVersion\":0}", out _));
        }

        [Test]
        public void FileRepository_PersistsAtomicRevisionAndReloads()
        {
            var store = new FileCompetitionRepository(path);
            TournamentSnapshot snapshot = Snapshot(0);
            Assert.That(store.TryCommit("cup", -1, snapshot), Is.True);
            var reloaded = new FileCompetitionRepository(path);
            Assert.That(reloaded.TryLoad("cup", out TournamentSnapshot loaded), Is.True);
            Assert.That(loaded.Revision, Is.EqualTo(0));
            Assert.That(loaded.Definition.CatalogVersion, Is.EqualTo("000000000201"));
        }

        [Test]
        public void CorruptActive_UsesBackupAsReadOnlyLastKnownGood()
        {
            var store = new FileCompetitionRepository(path);
            Assert.That(store.TryCommit("cup", -1, Snapshot(0)), Is.True);
            Assert.That(store.TryCommit("cup", 0, Snapshot(1)), Is.True);
            File.WriteAllText(path, "{corrupt");

            var recovered = new FileCompetitionRepository(path);
            Assert.That(recovered.LoadedFromBackup, Is.True);
            Assert.That(recovered.IsReadOnly, Is.True);
            Assert.That(recovered.TryLoad("cup", out TournamentSnapshot loaded), Is.True);
            Assert.That(loaded.Revision, Is.EqualTo(0));
        }

        [Test]
        public void StaleOrReadOnlyCommit_DoesNotMutateStore()
        {
            var store = new FileCompetitionRepository(path);
            Assert.That(store.TryCommit("cup", -1, Snapshot(0)), Is.True);
            Assert.That(store.TryCommit("cup", -1, Snapshot(1)), Is.False);
            var readOnly = new FileCompetitionRepository(path, forceReadOnly: true);
            Assert.That(readOnly.TryCommit("cup", 0, Snapshot(1)), Is.False);
        }

        private static TournamentSnapshot Snapshot(long revision) => new TournamentSnapshot
        {
            TournamentId = "cup", Revision = revision, State = TournamentState.Registration,
            Definition = new TournamentDefinition
            {
                TournamentId = "cup", CatalogVersion = "000000000201",
                Rules = new CompetitionRules { RulesVersion = "000000000101", RosterSize = 2 }
            }
        };
    }
}
