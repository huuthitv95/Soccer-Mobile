using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoccerMobilePro.Catalog;
using SoccerMobilePro.PlayerItems;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class PlayerItemsTransactionTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 7, 16, 7, 0, 0, TimeSpan.Zero);
        private InMemoryInventoryStore store;
        private FixedProgressionRuleSet rules;
        private DeterministicProgressionPreviewService previews;
        private InventoryTransactionService transactions;

        [SetUp]
        public void SetUp()
        {
            store = new InMemoryInventoryStore();
            rules = new FixedProgressionRuleSet(
                "000000000101",
                1000,
                2,
                100,
                new[]
                {
                    new SkillDefinition { SkillId = "skill-pass", ModifierBasisPoints = 250, EligibleItemDefinitionIds = new List<string> { "item-def-01" } },
                    new SkillDefinition { SkillId = "skill-press", ModifierBasisPoints = 300, EligibleItemDefinitionIds = new List<string> { "item-def-01" } },
                    new SkillDefinition { SkillId = "skill-third", ModifierBasisPoints = 200, EligibleItemDefinitionIds = new List<string> { "item-def-01" } }
                },
                new Dictionary<string, IEnumerable<string>> { ["item-def-01"] = new[] { "CM", "DM" } });
            previews = new DeterministicProgressionPreviewService(rules);
            transactions = new InventoryTransactionService(store, store, rules);
            store.Seed(CreateInventory());
        }

        [Test]
        public void Preview_SameInputAndClock_IsDeterministic()
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            var intent = new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 };

            ProgressionPreviewResult first = previews.Build(snapshot, "item-a", intent, Now);
            ProgressionPreviewResult second = previews.Build(snapshot, "item-a", intent, Now);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(second.Preview.PreviewHash, Is.EqualTo(first.Preview.PreviewHash));
            Assert.That(first.Preview.AfterItem.LevelXp, Is.EqualTo(200));
        }

        [Test]
        public void LockedItem_CannotCreatePreview()
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            snapshot.Find("item-a").LockState = PlayerItemLockState.Locked;

            ProgressionPreviewResult result = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 1 }, Now);

            Assert.That(result.FailureCode, Is.EqualTo(TransactionFailureCode.Locked));
        }

        [Test]
        public void AssignSkill_EnforcesEligibilityAndCap()
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            ProgressionPreviewResult first = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.AssignSkill, SelectionId = "skill-pass" }, Now);
            Assert.That(first.Succeeded, Is.True);
            snapshot.Items[0] = first.Preview.AfterItem;
            ProgressionPreviewResult second = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.AssignSkill, SelectionId = "skill-press" }, Now);
            Assert.That(second.Succeeded, Is.True);
            snapshot.Items[0] = second.Preview.AfterItem;

            ProgressionPreviewResult capped = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.AssignSkill, SelectionId = "skill-third" }, Now);

            Assert.That(capped.FailureCode, Is.EqualTo(TransactionFailureCode.CapExceeded));
        }

        [Test]
        public void PositionChoice_RejectsIneligiblePosition()
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            ProgressionPreviewResult result = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.ChoosePosition, SelectionId = "GK" }, Now);
            Assert.That(result.FailureCode, Is.EqualTo(TransactionFailureCode.Ineligible));
        }

        [Test]
        public void Execute_CommitsExactlyOneRevisionAndBalancedLedger()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionCommand command = Command(preview, "idem-allocate");

            TransactionReceipt receipt = transactions.Execute(command, preview, Now.AddSeconds(1));

            Assert.That(receipt.Status, Is.EqualTo(TransactionStatus.Committed));
            Assert.That(receipt.InventoryDelta.BaseRevision, Is.EqualTo(7));
            Assert.That(receipt.InventoryDelta.TargetRevision, Is.EqualTo(8));
            Assert.That(store.IsBalanced(receipt.TransactionId), Is.True);
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Find("item-a").LevelXp, Is.EqualTo(200));
        }

        [Test]
        public void RetryWithSameKeyAndPayload_ReturnsSameReceiptWithoutMutation()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionCommand command = Command(preview, "idem-retry");

            TransactionReceipt first = transactions.Execute(command, preview, Now.AddSeconds(1));
            TransactionReceipt second = transactions.Execute(command, preview, Now.AddSeconds(2));

            Assert.That(second.TransactionId, Is.EqualTo(first.TransactionId));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Revision, Is.EqualTo(8));
        }

        [Test]
        public void RetryWithSameKeyDifferentPayload_IsRejected()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionCommand first = Command(preview, "idem-conflict");
            Assert.That(transactions.Execute(first, preview, Now.AddSeconds(1)).Status, Is.EqualTo(TransactionStatus.Committed));
            ProgressionCommand conflicting = Command(preview, "idem-conflict");
            conflicting.Intent.Amount = 200;

            TransactionReceipt receipt = transactions.Execute(conflicting, preview, Now.AddSeconds(2));

            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.IdempotencyConflict));
        }

        [Test]
        public void StaleCommand_DoesNotMutateInventory()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionCommand command = Command(preview, "idem-stale");
            command.ExpectedInventoryRevision--;

            TransactionReceipt receipt = transactions.Execute(command, preview, Now.AddSeconds(1));

            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.StaleRevision));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Revision, Is.EqualTo(7));
        }

        [Test]
        public void TamperedPreview_IsRejected()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionCommand command = Command(preview, "idem-tamper");
            preview.AfterItem.LevelXp = 999;

            TransactionReceipt receipt = transactions.Execute(command, preview, Now.AddSeconds(1));

            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.PreviewMismatch));
        }

        [Test]
        public void ExpiredPreview_IsRejectedWithoutMutation()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            TransactionReceipt receipt = transactions.Execute(Command(preview, "idem-expired"), preview, preview.ExpiresAt.AddTicks(1));
            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.PreviewExpired));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Revision, Is.EqualTo(7));
        }

        [Test]
        public void ConcurrentCommandsFromSameRevision_CommitOnlyOnce()
        {
            ProgressionPreview firstPreview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 100 });
            ProgressionPreview secondPreview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.ChoosePosition, SelectionId = "CM" });

            TransactionReceipt first = transactions.Execute(Command(firstPreview, "idem-concurrent-a"), firstPreview, Now.AddSeconds(1));
            TransactionReceipt second = transactions.Execute(Command(secondPreview, "idem-concurrent-b"), secondPreview, Now.AddSeconds(1));

            Assert.That(first.Status, Is.EqualTo(TransactionStatus.Committed));
            Assert.That(second.FailureCode, Is.EqualTo(TransactionFailureCode.StaleRevision));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Revision, Is.EqualTo(8));
        }

        [Test]
        public void Fixture_UsesStableCatalogItemDefinitionIds()
        {
            CatalogSnapshot catalog = CatalogFixtureFactory.Create();
            FixedProgressionRuleSet fixtureRules = PlayerItemsFixtureFactory.CreateRules(catalog, "000000000101");
            InventorySnapshot fixture = PlayerItemsFixtureFactory.CreateInventory(catalog, "fixture-owner", fixtureRules.Version, Now);

            Assert.That(fixture.Items, Has.Count.EqualTo(3));
            Assert.That(fixture.Items[0].ItemDefinitionId, Is.EqualTo(catalog.Items[0].ItemDefinitionId));
            Assert.That(fixture.CatalogVersion, Is.EqualTo(catalog.CatalogVersion));
        }

        [Test]
        public void Fusion_ConsumesSourceAndUpdatesTargetAtomically()
        {
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Fuse, SourceItemId = "item-source" });

            TransactionReceipt receipt = transactions.Execute(Command(preview, "idem-fusion"), preview, Now.AddSeconds(1));

            Assert.That(receipt.Status, Is.EqualTo(TransactionStatus.Committed));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Find("item-source"), Is.Null);
            Assert.That(current.Find("item-a").LevelXp, Is.EqualTo(250));
        }

        [Test]
        public void FusionRejectsSourceThatIsInSquad()
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            snapshot.Find("item-source").State = PlayerItemState.InSquad;

            ProgressionPreviewResult result = previews.Build(snapshot, "item-a", new ProgressionIntent { Operation = ProgressionOperation.Fuse, SourceItemId = "item-source" }, Now);

            Assert.That(result.FailureCode, Is.EqualTo(TransactionFailureCode.InvalidState));
        }

        [Test]
        public void DirectGrant_IsAtomicAndIdempotent()
        {
            var item = NewItem("item-granted", 0);
            var command = new GrantCommand { Item = item, IdempotencyKey = "idem-grant" };

            TransactionReceipt first = transactions.Grant(command, Now);
            TransactionReceipt second = transactions.Grant(command, Now.AddSeconds(1));

            Assert.That(first.Status, Is.EqualTo(TransactionStatus.Committed));
            Assert.That(second.TransactionId, Is.EqualTo(first.TransactionId));
            store.TryLoad("owner-a", out InventorySnapshot current);
            Assert.That(current.Items.FindAll(entry => entry.ItemId == "item-granted"), Has.Count.EqualTo(1));
        }

        [Test]
        public void ReadOnlyService_RejectsMutation()
        {
            var readOnly = new InventoryTransactionService(store, store, rules, true);
            ProgressionPreview preview = Build("item-a", new ProgressionIntent { Operation = ProgressionOperation.Allocate, Amount = 1 });
            TransactionReceipt receipt = readOnly.Execute(Command(preview, "idem-readonly"), preview, Now);
            Assert.That(receipt.FailureCode, Is.EqualTo(TransactionFailureCode.ReadOnly));
        }

        private ProgressionPreview Build(string itemId, ProgressionIntent intent)
        {
            store.TryLoad("owner-a", out InventorySnapshot snapshot);
            ProgressionPreviewResult result = previews.Build(snapshot, itemId, intent, Now);
            Assert.That(result.Succeeded, Is.True, result.FailureCode.ToString());
            return result.Preview;
        }

        private static ProgressionCommand Command(ProgressionPreview preview, string key)
        {
            return new ProgressionCommand
            {
                OwnerId = preview.OwnerId,
                ItemId = preview.ItemId,
                ExpectedInventoryRevision = preview.InventoryRevision,
                ExpectedItemRevision = preview.ItemRevision,
                PreviewHash = preview.PreviewHash,
                IdempotencyKey = key,
                Intent = new ProgressionIntent
                {
                    Operation = preview.Intent.Operation,
                    Amount = preview.Intent.Amount,
                    SelectionId = preview.Intent.SelectionId,
                    SourceItemId = preview.Intent.SourceItemId
                }
            };
        }

        private static InventorySnapshot CreateInventory()
        {
            return new InventorySnapshot
            {
                OwnerId = "owner-a",
                Revision = 7,
                CatalogVersion = "000000000201",
                RulesVersion = "000000000101",
                Items = new List<OwnedPlayerItem> { NewItem("item-a", 100), NewItem("item-source", 50) }
            };
        }

        private static OwnedPlayerItem NewItem(string itemId, int xp)
        {
            return new OwnedPlayerItem
            {
                ItemId = itemId,
                OwnerId = "owner-a",
                ItemDefinitionId = "item-def-01",
                CatalogVersion = "000000000201",
                AcquiredAt = Now,
                LevelXp = xp,
                LockState = PlayerItemLockState.Unlocked,
                State = PlayerItemState.Available,
                Revision = 3,
                RulesVersion = "000000000101"
            };
        }
    }
}
