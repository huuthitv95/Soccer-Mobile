using System.Collections;
using NUnit.Framework;
using SoccerMobilePro.MatchCore;
using SoccerMobilePro.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace SoccerMobilePro.MatchCore.PlayModeTests
{
    public sealed class MatchCorePlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator MatchLifecycle_AdvancesAcrossFrames()
        {
            var simulation = new DeterministicMatchSimulation(17);
            Assert.That(simulation.Apply(new MatchCommand(1, 0, "smoke", MatchCommandType.StartMatch)).Accepted, Is.True);
            yield return null;
            Assert.That(simulation.Apply(new MatchCommand(2, 0, "smoke", MatchCommandType.ResumePlay)).Accepted, Is.True);
            yield return null;
            Assert.That(simulation.Apply(new MatchCommand(3, 0, "smoke", MatchCommandType.AdvanceTick, tickDelta: 1)).Accepted, Is.True);

            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.InPlay));
            Assert.That(simulation.Snapshot.Tick, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ContextualInput_CreatesTypedCommandAcrossFrames()
        {
            InputActionAsset source = Resources.Load<InputActionAsset>(ContextualMatchInputRuntime.ResourcePath);
            Assert.That(source, Is.Not.Null);

            using (var adapter = new ContextualMatchInputAdapter(source, "playmode"))
            {
                adapter.SetContext(MatchInputContext.OnBall);
                yield return null;
                Assert.That(adapter.TryCreateCommand("Shoot", 1, Vector2.zero, 1f, out MatchCommand command), Is.True);
                Assert.That(command.Type, Is.EqualTo(MatchCommandType.Shoot));
                Assert.That(command.Modifiers, Is.EqualTo(MatchInputContext.OnBall.ToString()));
            }
        }
    }
}
