using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SoccerMobilePro.PlayerItems
{
    public sealed class SkillDefinition
    {
        public string SkillId { get; set; } = string.Empty;
        public int ModifierBasisPoints { get; set; }
        public List<string> EligibleItemDefinitionIds { get; set; } = new List<string>();
    }

    public interface IProgressionRuleSet
    {
        string Version { get; }
        int MaxLevelXp { get; }
        int MaxAdditionalSkills { get; }
        int FusionBaseXp { get; }
        bool TryGetSkill(string skillId, out SkillDefinition definition);
        bool IsPositionEligible(string itemDefinitionId, string positionId);
    }

    public sealed class FixedProgressionRuleSet : IProgressionRuleSet
    {
        private readonly Dictionary<string, SkillDefinition> skills;
        private readonly Dictionary<string, HashSet<string>> positions;

        public FixedProgressionRuleSet(
            string version,
            int maxLevelXp,
            int maxAdditionalSkills,
            int fusionBaseXp,
            IEnumerable<SkillDefinition> skills,
            IReadOnlyDictionary<string, IEnumerable<string>> positions)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            MaxLevelXp = maxLevelXp;
            MaxAdditionalSkills = maxAdditionalSkills;
            FusionBaseXp = fusionBaseXp;
            this.skills = (skills ?? Array.Empty<SkillDefinition>()).ToDictionary(skill => skill.SkillId, StringComparer.Ordinal);
            this.positions = (positions ?? new Dictionary<string, IEnumerable<string>>())
                .ToDictionary(pair => pair.Key, pair => new HashSet<string>(pair.Value ?? Array.Empty<string>(), StringComparer.Ordinal), StringComparer.Ordinal);
        }

        public string Version { get; }
        public int MaxLevelXp { get; }
        public int MaxAdditionalSkills { get; }
        public int FusionBaseXp { get; }
        public bool TryGetSkill(string skillId, out SkillDefinition definition) => skills.TryGetValue(skillId ?? string.Empty, out definition);
        public bool IsPositionEligible(string itemDefinitionId, string positionId)
            => positions.TryGetValue(itemDefinitionId ?? string.Empty, out HashSet<string> allowed) && allowed.Contains(positionId ?? string.Empty);
    }

    public sealed class DeterministicProgressionPreviewService : IProgressionPreviewService
    {
        private static readonly TimeSpan PreviewLifetime = TimeSpan.FromMinutes(5);
        private readonly IProgressionRuleSet rules;

        public DeterministicProgressionPreviewService(IProgressionRuleSet rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public ProgressionPreviewResult Build(InventorySnapshot snapshot, string itemId, ProgressionIntent intent, DateTimeOffset nowUtc)
        {
            if (snapshot == null || intent == null || string.IsNullOrWhiteSpace(itemId)) return Fail(TransactionFailureCode.InvalidRequest);
            if (!string.Equals(snapshot.RulesVersion, rules.Version, StringComparison.Ordinal)) return Fail(TransactionFailureCode.StaleRules);

            OwnedPlayerItem item = snapshot.Find(itemId);
            if (item == null) return Fail(TransactionFailureCode.ItemNotFound);
            if (!string.Equals(item.OwnerId, snapshot.OwnerId, StringComparison.Ordinal)) return Fail(TransactionFailureCode.OwnerMismatch);
            if (!string.Equals(item.CatalogVersion, snapshot.CatalogVersion, StringComparison.Ordinal)) return Fail(TransactionFailureCode.StaleCatalog);
            if (!string.Equals(item.RulesVersion, rules.Version, StringComparison.Ordinal)) return Fail(TransactionFailureCode.StaleRules);
            if (item.LockState == PlayerItemLockState.Locked) return Fail(TransactionFailureCode.Locked);
            if (item.State == PlayerItemState.Consumed || item.State == PlayerItemState.Reserved) return Fail(TransactionFailureCode.InvalidState);

            OwnedPlayerItem after = item.Clone();
            string sourceItemId = string.Empty;
            TransactionFailureCode failure = Apply(snapshot, after, intent, out sourceItemId);
            if (failure != TransactionFailureCode.None) return Fail(failure);

            after.Revision = item.Revision + 1;
            after.RulesVersion = rules.Version;
            var preview = new ProgressionPreview
            {
                OwnerId = snapshot.OwnerId,
                ItemId = item.ItemId,
                InventoryRevision = snapshot.Revision,
                ItemRevision = item.Revision,
                CatalogVersion = snapshot.CatalogVersion,
                RulesVersion = rules.Version,
                Intent = new ProgressionIntent
                {
                    Operation = intent.Operation,
                    Amount = intent.Amount,
                    SelectionId = intent.SelectionId ?? string.Empty,
                    SourceItemId = intent.SourceItemId ?? string.Empty
                },
                BeforeItem = item.Clone(),
                AfterItem = after,
                SourceItemId = sourceItemId,
                ExpiresAt = nowUtc.Add(PreviewLifetime)
            };
            preview.PreviewHash = ProgressionHash.ComputePreview(preview);
            return new ProgressionPreviewResult(preview, TransactionFailureCode.None);
        }

        private TransactionFailureCode Apply(InventorySnapshot snapshot, OwnedPlayerItem after, ProgressionIntent intent, out string sourceItemId)
        {
            sourceItemId = string.Empty;
            switch (intent.Operation)
            {
                case ProgressionOperation.Allocate:
                    if (intent.Amount <= 0) return TransactionFailureCode.InvalidRequest;
                    if (after.LevelXp > rules.MaxLevelXp - intent.Amount) return TransactionFailureCode.CapExceeded;
                    after.LevelXp += intent.Amount;
                    after.ProgressionAllocation["level-xp"] = after.LevelXp;
                    return TransactionFailureCode.None;
                case ProgressionOperation.Reset:
                    after.ProgressionAllocation.Clear();
                    return TransactionFailureCode.None;
                case ProgressionOperation.AssignSkill:
                    if (!rules.TryGetSkill(intent.SelectionId, out SkillDefinition skill)) return TransactionFailureCode.Ineligible;
                    if (skill.ModifierBasisPoints < -1000 || skill.ModifierBasisPoints > 1000) return TransactionFailureCode.Ineligible;
                    if (skill.EligibleItemDefinitionIds.Count > 0 && !skill.EligibleItemDefinitionIds.Contains(after.ItemDefinitionId, StringComparer.Ordinal)) return TransactionFailureCode.Ineligible;
                    if (after.AdditionalSkills.Contains(skill.SkillId, StringComparer.Ordinal)) return TransactionFailureCode.InvalidRequest;
                    if (after.AdditionalSkills.Count >= rules.MaxAdditionalSkills) return TransactionFailureCode.CapExceeded;
                    after.AdditionalSkills.Add(skill.SkillId);
                    after.AdditionalSkills.Sort(StringComparer.Ordinal);
                    return TransactionFailureCode.None;
                case ProgressionOperation.ChoosePosition:
                    if (!rules.IsPositionEligible(after.ItemDefinitionId, intent.SelectionId)) return TransactionFailureCode.Ineligible;
                    if (!after.PositionProficiencies.Contains(intent.SelectionId, StringComparer.Ordinal))
                    {
                        after.PositionProficiencies.Add(intent.SelectionId);
                        after.PositionProficiencies.Sort(StringComparer.Ordinal);
                    }
                    return TransactionFailureCode.None;
                case ProgressionOperation.Fuse:
                    if (string.IsNullOrWhiteSpace(intent.SourceItemId) || string.Equals(after.ItemId, intent.SourceItemId, StringComparison.Ordinal)) return TransactionFailureCode.InvalidRequest;
                    OwnedPlayerItem source = snapshot.Find(intent.SourceItemId);
                    if (source == null) return TransactionFailureCode.ItemNotFound;
                    if (!string.Equals(source.OwnerId, snapshot.OwnerId, StringComparison.Ordinal)) return TransactionFailureCode.OwnerMismatch;
                    if (source.LockState == PlayerItemLockState.Locked) return TransactionFailureCode.Locked;
                    if (source.State != PlayerItemState.Available) return TransactionFailureCode.InvalidState;
                    int gained = Math.Max(0, source.LevelXp) + rules.FusionBaseXp;
                    if (after.LevelXp > rules.MaxLevelXp - gained) return TransactionFailureCode.CapExceeded;
                    after.LevelXp += gained;
                    after.ProgressionAllocation["level-xp"] = after.LevelXp;
                    sourceItemId = source.ItemId;
                    return TransactionFailureCode.None;
                default:
                    return TransactionFailureCode.InvalidRequest;
            }
        }

        private static ProgressionPreviewResult Fail(TransactionFailureCode code) => new ProgressionPreviewResult(null, code);
    }

    public static class ProgressionHash
    {
        public static string ComputePreview(ProgressionPreview preview)
        {
            if (preview == null) return string.Empty;
            string canonical = string.Join("|",
                preview.OwnerId,
                preview.ItemId,
                preview.InventoryRevision.ToString(CultureInfo.InvariantCulture),
                preview.ItemRevision.ToString(CultureInfo.InvariantCulture),
                preview.CatalogVersion,
                preview.RulesVersion,
                preview.Intent?.CanonicalValue() ?? string.Empty,
                CanonicalItem(preview.BeforeItem),
                CanonicalItem(preview.AfterItem),
                preview.SourceItemId,
                preview.ExpiresAt.UtcTicks.ToString(CultureInfo.InvariantCulture));
            return Sha256(canonical);
        }

        public static string ComputePayload(string payload) => Sha256(payload ?? string.Empty);

        private static string CanonicalItem(OwnedPlayerItem item)
        {
            if (item == null) return string.Empty;
            string allocations = string.Join(",", (item.ProgressionAllocation ?? new Dictionary<string, int>()).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => $"{pair.Key}:{pair.Value}"));
            string skills = string.Join(",", (item.AdditionalSkills ?? new List<string>()).OrderBy(value => value, StringComparer.Ordinal));
            string positions = string.Join(",", (item.PositionProficiencies ?? new List<string>()).OrderBy(value => value, StringComparer.Ordinal));
            return string.Join("~", item.ItemId, item.OwnerId, item.ItemDefinitionId, item.CatalogVersion, item.LevelXp, allocations, skills, positions, (int)item.LockState, (int)item.State, item.Revision, item.RulesVersion);
        }

        private static string Sha256(string value)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte item in hash) builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }
    }
}
