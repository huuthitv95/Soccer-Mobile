using System.Linq;
using NUnit.Framework;
using SoccerMobilePro.Input;
using SoccerMobilePro.MatchCore;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class ContextualMatchInputTests
    {
        private InputActionAsset sourceAsset;

        [SetUp]
        public void SetUp()
        {
            sourceAsset = Resources.Load<InputActionAsset>(ContextualMatchInputRuntime.ResourcePath);
            Assert.That(sourceAsset, Is.Not.Null, "Input Action asset must be available through Resources.");
        }

        [Test]
        public void Asset_DefinesRequiredMapsAndSchemes()
        {
            string[] expectedMaps =
            {
                ContextualMatchInputAdapter.OnBallMap,
                ContextualMatchInputAdapter.OffBallMap,
                ContextualMatchInputAdapter.SetPieceMap,
                ContextualMatchInputAdapter.GoalkeeperMap,
                ContextualMatchInputAdapter.UiMap
            };

            CollectionAssert.AreEquivalent(expectedMaps, sourceAsset.actionMaps.Select(map => map.name));
            CollectionAssert.AreEquivalent(
                new[] { "Touch", "Gamepad", "Keyboard" },
                sourceAsset.controlSchemes.Select(scheme => scheme.name));
        }

        [Test]
        public void Asset_HasNoConflictingSimpleBindingsWithinAContext()
        {
            Assert.That(InputBindingConflictValidator.FindConflicts(sourceAsset), Is.Empty);
        }

        [Test]
        public void SetContext_EnablesExactlyOneMap()
        {
            using (var adapter = new ContextualMatchInputAdapter(sourceAsset, "test-player"))
            {
                adapter.SetContext(MatchInputContext.OnBall);
                Assert.That(adapter.Asset.actionMaps.Count(map => map.enabled), Is.EqualTo(1));
                Assert.That(adapter.ActiveMap.name, Is.EqualTo(ContextualMatchInputAdapter.OnBallMap));

                adapter.SetContext(MatchInputContext.OffBall);
                Assert.That(adapter.Asset.actionMaps.Count(map => map.enabled), Is.EqualTo(1));
                Assert.That(adapter.ActiveMap.name, Is.EqualTo(ContextualMatchInputAdapter.OffBallMap));
            }
        }

        [Test]
        public void Commands_AreContextualAndSequenceIsMonotonic()
        {
            using (var adapter = new ContextualMatchInputAdapter(sourceAsset, "test-player"))
            {
                adapter.SetContext(MatchInputContext.OnBall);
                Assert.That(adapter.TryCreateCommand("Pass", 10, Vector2.zero, 1f, out MatchCommand pass), Is.True);
                Assert.That(pass.Type, Is.EqualTo(MatchCommandType.Pass));

                adapter.SetContext(MatchInputContext.OffBall);
                Assert.That(adapter.TryCreateCommand("Pass", 11, Vector2.zero, 1f, out _), Is.False);
                Assert.That(adapter.TryCreateCommand("Tackle", 11, Vector2.zero, 1f, out MatchCommand tackle), Is.True);
                Assert.That(tackle.Type, Is.EqualTo(MatchCommandType.Tackle));
                Assert.That(tackle.SequenceId, Is.EqualTo(pass.SequenceId + 1));
            }
        }

        [Test]
        public void HudProfiles_MirrorAnchorsAndStayWithinBounds()
        {
            HudLayoutProfile standard = HudLayoutProfile.Create(HudControlPreset.Standard);
            HudLayoutProfile leftHanded = HudLayoutProfile.Create(HudControlPreset.LeftHanded);

            Assert.That(leftHanded.MovementAnchor, Is.EqualTo(standard.ActionAnchor));
            Assert.That(leftHanded.ActionAnchor, Is.EqualTo(standard.MovementAnchor));
            Assert.That(standard.Scale, Is.InRange(0.75f, 1.5f));
            Assert.That(standard.Opacity, Is.InRange(0f, 1f));
            Assert.That(standard.DeadZone, Is.InRange(0.05f, 0.5f));
        }

        [Test]
        public void BindingOverride_RoundTripsWithoutChangingAssetDefaults()
        {
            using (var first = new ContextualMatchInputAdapter(sourceAsset, "first"))
            using (var second = new ContextualMatchInputAdapter(sourceAsset, "second"))
            {
                InputAction firstPass = first.Asset.FindActionMap(ContextualMatchInputAdapter.OnBallMap).FindAction("Pass");
                firstPass.ApplyBindingOverride(1, "<Keyboard>/f");
                string overrides = first.Asset.SaveBindingOverridesAsJson();
                second.Asset.LoadBindingOverridesFromJson(overrides);

                InputAction secondPass = second.Asset.FindActionMap(ContextualMatchInputAdapter.OnBallMap).FindAction("Pass");
                Assert.That(secondPass.bindings[1].effectivePath, Is.EqualTo("<Keyboard>/f"));
                Assert.That(sourceAsset.FindActionMap(ContextualMatchInputAdapter.OnBallMap).FindAction("Pass").bindings[1].effectivePath, Is.EqualTo("<Keyboard>/e"));
            }
        }
    }
}
