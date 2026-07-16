using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SoccerMobilePro.PlayerItems;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class PlayerItemsPersistenceTests
    {
        private string directory;
        private string path;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "soccer-mobile-player-items", Guid.NewGuid().ToString("N"));
            path = Path.Combine(directory, "inventory.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void Codec_RoundTripsUnknownFields()
        {
            const string payload = "{\"schemaVersion\":2,\"inventories\":[],\"receipts\":[],\"ledgerEntries\":[],\"futurePolicy\":{\"value\":7}}";
            var codec = new InventoryCodec();

            InventoryEnvelope envelope = codec.Deserialize(payload, out bool migrated);
            string serialized = codec.Serialize(envelope);

            Assert.That(migrated, Is.False);
            Assert.That(serialized, Does.Contain("\"futurePolicy\""));
            Assert.That(serialized, Does.Contain("\"value\": 7"));
        }

        [Test]
        public void Codec_MigratesNMinusOneAndRejectsNMinusTwo()
        {
            var codec = new InventoryCodec();
            InventoryEnvelope migrated = codec.Deserialize("{\"schemaVersion\":1}", out bool didMigrate);
            Assert.That(didMigrate, Is.True);
            Assert.That(migrated.SchemaVersion, Is.EqualTo(2));
            Assert.Throws<InventoryPersistenceException>(() => codec.Deserialize("{\"schemaVersion\":0}", out _));
        }

        [Test]
        public void FileStore_PersistsReceiptInventoryAndBalancedLedger()
        {
            FixedProgressionRuleSet rules = Rules();
            var store = new FileInventoryStore(path);
            var service = new InventoryTransactionService(store, store, rules);

            TransactionReceipt receipt = service.Grant(new GrantCommand { Item = Item("file-item"), IdempotencyKey = "file-grant" }, Now());
            var reloaded = new FileInventoryStore(path);

            Assert.That(receipt.Status, Is.EqualTo(TransactionStatus.Committed));
            Assert.That(reloaded.TryLoad("owner-file", out InventorySnapshot snapshot), Is.True);
            Assert.That(snapshot.Find("file-item"), Is.Not.Null);
            Assert.That(reloaded.TryGetReceipt("file-grant", out TransactionReceipt persisted), Is.True);
            Assert.That(reloaded.IsBalanced(persisted.TransactionId), Is.True);
        }

        [Test]
        public void CorruptActive_UsesBackupAsReadOnlyLastKnownGood()
        {
            FixedProgressionRuleSet rules = Rules();
            var store = new FileInventoryStore(path);
            var service = new InventoryTransactionService(store, store, rules);
            Assert.That(service.Grant(new GrantCommand { Item = Item("first"), IdempotencyKey = "grant-first" }, Now()).Status, Is.EqualTo(TransactionStatus.Committed));
            Assert.That(service.Grant(new GrantCommand { Item = Item("second"), IdempotencyKey = "grant-second" }, Now().AddSeconds(1)).Status, Is.EqualTo(TransactionStatus.Committed));
            File.WriteAllText(path, "{corrupt");

            var recovered = new FileInventoryStore(path);

            Assert.That(recovered.LoadedFromBackup, Is.True);
            Assert.That(recovered.IsReadOnly, Is.True);
            Assert.That(recovered.TryLoad("owner-file", out InventorySnapshot snapshot), Is.True);
            Assert.That(snapshot.Find("first"), Is.Not.Null);
            Assert.That(snapshot.Find("second"), Is.Null, "Backup is the previous last-known-good revision.");
        }

        [Test]
        public void ForcedReadOnlyStore_RejectsCommit()
        {
            var store = new FileInventoryStore(path, forceReadOnly: true);
            var service = new InventoryTransactionService(store, store, Rules(), true);
            TransactionReceipt receipt = service.Grant(new GrantCommand { Item = Item("blocked"), IdempotencyKey = "grant-blocked" }, Now());
            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.ReadOnly));
            Assert.That(File.Exists(path), Is.False);
        }

        [Test]
        public void RetryAfterReload_ReturnsPersistedReceiptWithoutDuplicateGrant()
        {
            FixedProgressionRuleSet rules = Rules();
            var firstStore = new FileInventoryStore(path);
            var command = new GrantCommand { Item = Item("reconnect-item"), IdempotencyKey = "grant-reconnect" };
            TransactionReceipt first = new InventoryTransactionService(firstStore, firstStore, rules).Grant(command, Now());

            var reloadedStore = new FileInventoryStore(path);
            TransactionReceipt retry = new InventoryTransactionService(reloadedStore, reloadedStore, rules).Grant(command, Now().AddMinutes(1));

            Assert.That(retry.TransactionId, Is.EqualTo(first.TransactionId));
            reloadedStore.TryLoad("owner-file", out InventorySnapshot snapshot);
            Assert.That(snapshot.Items.FindAll(item => item.ItemId == "reconnect-item"), Has.Count.EqualTo(1));
        }

        private static FixedProgressionRuleSet Rules()
            => new FixedProgressionRuleSet("000000000101", 1000, 2, 100, Array.Empty<SkillDefinition>(), new Dictionary<string, IEnumerable<string>>());

        private static OwnedPlayerItem Item(string itemId)
            => new OwnedPlayerItem
            {
                ItemId = itemId,
                OwnerId = "owner-file",
                ItemDefinitionId = "item-def-01",
                CatalogVersion = "000000000201",
                RulesVersion = "000000000101",
                AcquiredAt = Now(),
                State = PlayerItemState.Available
            };

        private static DateTimeOffset Now() => new DateTimeOffset(2026, 7, 16, 8, 0, 0, TimeSpan.Zero);
    }
}
