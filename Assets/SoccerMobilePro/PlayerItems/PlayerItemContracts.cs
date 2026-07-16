using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoccerMobilePro.PlayerItems
{
    public enum PlayerItemLockState { Unlocked = 0, Locked = 1 }
    public enum PlayerItemState { Available = 0, InSquad = 1, Reserved = 2, Consumed = 3 }
    public enum ProgressionOperation { Allocate = 0, Reset = 1, AssignSkill = 2, ChoosePosition = 3, Fuse = 4, DirectGrant = 5 }
    public enum TransactionStatus { Committed = 0, Rejected = 1 }

    public enum TransactionFailureCode
    {
        None = 0,
        InvalidRequest = 1,
        OwnerMismatch = 2,
        ItemNotFound = 3,
        DuplicateItem = 4,
        Locked = 5,
        InvalidState = 6,
        StaleRevision = 7,
        StaleCatalog = 8,
        StaleRules = 9,
        PreviewMismatch = 10,
        PreviewExpired = 11,
        Ineligible = 12,
        CapExceeded = 13,
        IdempotencyConflict = 14,
        AtomicCommitFailed = 15,
        ReadOnly = 16
    }

    public sealed class OwnedPlayerItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string ItemDefinitionId { get; set; } = string.Empty;
        public string CatalogVersion { get; set; } = string.Empty;
        public DateTimeOffset AcquiredAt { get; set; }
        public int LevelXp { get; set; }
        public Dictionary<string, int> ProgressionAllocation { get; set; } = new Dictionary<string, int>(StringComparer.Ordinal);
        public List<string> AdditionalSkills { get; set; } = new List<string>();
        public List<string> PositionProficiencies { get; set; } = new List<string>();
        public PlayerItemLockState LockState { get; set; }
        public PlayerItemState State { get; set; }
        public long Revision { get; set; }
        public string RulesVersion { get; set; } = string.Empty;
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);

        public OwnedPlayerItem Clone()
        {
            var clone = new OwnedPlayerItem
            {
                ItemId = ItemId,
                OwnerId = OwnerId,
                ItemDefinitionId = ItemDefinitionId,
                CatalogVersion = CatalogVersion,
                AcquiredAt = AcquiredAt,
                LevelXp = LevelXp,
                ProgressionAllocation = new Dictionary<string, int>(ProgressionAllocation ?? new Dictionary<string, int>(), StringComparer.Ordinal),
                AdditionalSkills = new List<string>(AdditionalSkills ?? new List<string>()),
                PositionProficiencies = new List<string>(PositionProficiencies ?? new List<string>()),
                LockState = LockState,
                State = State,
                Revision = Revision,
                RulesVersion = RulesVersion
            };
            clone.ExtensionData = CloneExtensionData(ExtensionData);
            return clone;
        }

        private static IDictionary<string, JToken> CloneExtensionData(IDictionary<string, JToken> source)
            => (source ?? new Dictionary<string, JToken>()).ToDictionary(pair => pair.Key, pair => pair.Value?.DeepClone(), StringComparer.Ordinal);
    }

    public sealed class InventorySnapshot
    {
        public int SchemaVersion { get; set; } = 2;
        public string OwnerId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CatalogVersion { get; set; } = string.Empty;
        public string RulesVersion { get; set; } = string.Empty;
        public List<OwnedPlayerItem> Items { get; set; } = new List<OwnedPlayerItem>();
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);

        public OwnedPlayerItem Find(string itemId)
            => (Items ?? new List<OwnedPlayerItem>()).FirstOrDefault(item => string.Equals(item.ItemId, itemId, StringComparison.Ordinal));

        public InventorySnapshot Clone()
        {
            var clone = new InventorySnapshot
            {
                SchemaVersion = SchemaVersion,
                OwnerId = OwnerId,
                Revision = Revision,
                CatalogVersion = CatalogVersion,
                RulesVersion = RulesVersion,
                Items = (Items ?? new List<OwnedPlayerItem>()).Select(item => item.Clone()).ToList()
            };
            clone.ExtensionData = (ExtensionData ?? new Dictionary<string, JToken>()).ToDictionary(pair => pair.Key, pair => pair.Value?.DeepClone(), StringComparer.Ordinal);
            return clone;
        }
    }

    public sealed class InventoryDelta
    {
        public long BaseRevision { get; set; }
        public long TargetRevision { get; set; }
        public List<OwnedPlayerItem> UpsertedItems { get; set; } = new List<OwnedPlayerItem>();
        public List<string> RemovedItemIds { get; set; } = new List<string>();
    }

    public sealed class ProgressionIntent
    {
        public ProgressionOperation Operation { get; set; }
        public int Amount { get; set; }
        public string SelectionId { get; set; } = string.Empty;
        public string SourceItemId { get; set; } = string.Empty;

        public string CanonicalValue()
            => string.Join("|", (int)Operation, Amount, SelectionId ?? string.Empty, SourceItemId ?? string.Empty);
    }

    public sealed class ProgressionPreview
    {
        public string OwnerId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public long InventoryRevision { get; set; }
        public long ItemRevision { get; set; }
        public string CatalogVersion { get; set; } = string.Empty;
        public string RulesVersion { get; set; } = string.Empty;
        public ProgressionIntent Intent { get; set; } = new ProgressionIntent();
        public OwnedPlayerItem BeforeItem { get; set; }
        public OwnedPlayerItem AfterItem { get; set; }
        public string SourceItemId { get; set; } = string.Empty;
        public string PreviewHash { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public readonly struct ProgressionPreviewResult
    {
        public ProgressionPreviewResult(ProgressionPreview preview, TransactionFailureCode failureCode)
        {
            Preview = preview;
            FailureCode = failureCode;
        }

        public ProgressionPreview Preview { get; }
        public TransactionFailureCode FailureCode { get; }
        public bool Succeeded => FailureCode == TransactionFailureCode.None && Preview != null;
    }

    public sealed class ProgressionCommand
    {
        public string OwnerId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public long ExpectedInventoryRevision { get; set; }
        public long ExpectedItemRevision { get; set; }
        public string PreviewHash { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
        public ProgressionIntent Intent { get; set; } = new ProgressionIntent();

        public string CanonicalPayload()
            => string.Join("|", OwnerId, ItemId, ExpectedInventoryRevision, ExpectedItemRevision, PreviewHash, Intent?.CanonicalValue() ?? string.Empty);
    }

    public sealed class GrantCommand
    {
        public OwnedPlayerItem Item { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;

        public string CanonicalPayload()
        {
            OwnedPlayerItem item = Item;
            return item == null
                ? string.Empty
                : string.Join("|", item.OwnerId, item.ItemId, item.ItemDefinitionId, item.CatalogVersion, item.RulesVersion, item.AcquiredAt.UtcDateTime.Ticks);
        }
    }

    public sealed class LedgerEntry
    {
        public string TransactionId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public long Amount { get; set; }
    }

    public sealed class TransactionReceipt
    {
        public string TransactionId { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public TransactionFailureCode FailureCode { get; set; }
        public InventoryDelta InventoryDelta { get; set; }
        public List<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
        public DateTimeOffset CreatedAt { get; set; }
    }

    public interface IInventoryRepository
    {
        bool TryLoad(string ownerId, out InventorySnapshot snapshot);
        bool TryCommit(string ownerId, long expectedRevision, InventorySnapshot next, TransactionReceipt receipt, IReadOnlyList<LedgerEntry> entries);
    }

    public interface ITransactionReceiptRepository
    {
        bool TryGetReceipt(string idempotencyKey, out TransactionReceipt receipt);
    }

    public interface ILedger
    {
        IReadOnlyList<LedgerEntry> EntriesForTransaction(string transactionId);
        bool IsBalanced(string transactionId);
    }

    public interface IProgressionPreviewService
    {
        ProgressionPreviewResult Build(InventorySnapshot snapshot, string itemId, ProgressionIntent intent, DateTimeOffset nowUtc);
    }

    public interface IInventoryTransactionService
    {
        TransactionReceipt Grant(GrantCommand command, DateTimeOffset nowUtc);
        TransactionReceipt Execute(ProgressionCommand command, ProgressionPreview preview, DateTimeOffset nowUtc);
    }
}
