using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SoccerMobilePro.PlayerItems;
using SoccerMobilePro.PlayerItems.Unity;
using UnityEngine;
using UnityEngine.TestTools;

namespace SoccerMobilePro.MatchCore.PlayModeTests
{
    public sealed class PlayerItemsPlayModeTests
    {
        [UnityTest]
        public IEnumerator FeatureGate_DefaultsToReadOnlyAndProjectsFixture()
        {
            var root = new GameObject("Player Items Diagnostic Gate");
            PlayerItemsFeatureGate gate = root.AddComponent<PlayerItemsFeatureGate>();
            var snapshot = new InventorySnapshot
            {
                OwnerId = "owner-play",
                Revision = 4,
                Items = new List<OwnedPlayerItem>
                {
                    new OwnedPlayerItem { ItemId = "item-b" },
                    new OwnedPlayerItem { ItemId = "item-a" }
                }
            };

            yield return null;
            InventoryProjection projection = InventoryProjectionFactory.Create(snapshot, gate);

            Assert.That(gate.FeatureEnabled, Is.False);
            Assert.That(gate.CanMutate, Is.False);
            Assert.That(projection.IsReadOnly, Is.True);
            Assert.That(projection.ItemIds, Is.EqualTo(new[] { "item-a", "item-b" }));
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator DiagnosticOverride_RequiresEnabledAndWritable()
        {
            var root = new GameObject("Player Items Diagnostic Gate");
            PlayerItemsFeatureGate gate = root.AddComponent<PlayerItemsFeatureGate>();
            gate.ConfigureForDiagnostics(true, false);
            yield return null;
            Assert.That(gate.CanMutate, Is.True);
            Object.Destroy(root);
        }
    }
}
