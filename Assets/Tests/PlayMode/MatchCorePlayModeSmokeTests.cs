using System.Collections;
using NUnit.Framework;
using SoccerMobilePro.MatchCore;
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
    }
}
