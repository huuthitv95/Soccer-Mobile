using System;
using System.Collections.Generic;
using System.Linq;
using SoccerMobilePro.Catalog;

namespace SoccerMobilePro.PlayerItems
{
    public static class PlayerItemsFixtureFactory
    {
        public static InventorySnapshot CreateInventory(CatalogSnapshot catalog, string ownerId, string rulesVersion, DateTimeOffset acquiredAt)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Fixture owner is required.", nameof(ownerId));
            if (!CatalogVersion.IsCanonical(catalog.CatalogVersion)) throw new ArgumentException("Fixture catalog version must be canonical.", nameof(catalog));

            List<PlayerItemDefinition> definitions = (catalog.Items ?? new List<PlayerItemDefinition>()).Take(3).ToList();
            if (definitions.Count < 2) throw new ArgumentException("Fixture catalog needs at least two player item definitions.", nameof(catalog));

            return new InventorySnapshot
            {
                OwnerId = ownerId,
                Revision = 0,
                CatalogVersion = catalog.CatalogVersion,
                RulesVersion = rulesVersion,
                Items = definitions.Select((definition, index) => new OwnedPlayerItem
                {
                    ItemId = $"fixture-owned-{index + 1:00}",
                    OwnerId = ownerId,
                    ItemDefinitionId = definition.ItemDefinitionId,
                    CatalogVersion = catalog.CatalogVersion,
                    RulesVersion = rulesVersion,
                    AcquiredAt = acquiredAt,
                    State = index == 1 ? PlayerItemState.Available : PlayerItemState.InSquad,
                    LockState = PlayerItemLockState.Unlocked,
                    Revision = 0
                }).ToList()
            };
        }

        public static FixedProgressionRuleSet CreateRules(CatalogSnapshot catalog, string rulesVersion)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            string[] definitionIds = (catalog.Items ?? new List<PlayerItemDefinition>()).Select(item => item.ItemDefinitionId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();
            if (definitionIds.Length == 0) throw new ArgumentException("Fixture catalog needs player item definitions.", nameof(catalog));
            var positions = definitionIds.ToDictionary(id => id, id => (IEnumerable<string>)new[] { "CM", "DM" }, StringComparer.Ordinal);
            var skills = new[]
            {
                new SkillDefinition { SkillId = "fixture-pass-lane", ModifierBasisPoints = 250, EligibleItemDefinitionIds = definitionIds.ToList() },
                new SkillDefinition { SkillId = "fixture-pressure", ModifierBasisPoints = 300, EligibleItemDefinitionIds = definitionIds.ToList() }
            };
            return new FixedProgressionRuleSet(rulesVersion, 1000, 2, 100, skills, positions);
        }
    }
}
