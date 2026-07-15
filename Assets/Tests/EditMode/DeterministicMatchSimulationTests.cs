using NUnit.Framework;
using SoccerMobilePro.MatchCore;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class DeterministicMatchSimulationTests
    {
        [Test]
        public void ValidFlow_ReachesFullTime()
        {
            DeterministicMatchSimulation simulation = CreateInPlay(42);

            AssertAccepted(simulation, 3, MatchCommandType.RecordFoul, 10);
            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.Foul));
            AssertAccepted(simulation, 4, MatchCommandType.ResumePlay, 10);
            AssertAccepted(simulation, 5, MatchCommandType.RecordCorner, 20);
            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.Corner));
            AssertAccepted(simulation, 6, MatchCommandType.ResumePlay, 20);
            AssertAccepted(simulation, 7, MatchCommandType.EndHalf, 2700);
            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.HalfTime));
            AssertAccepted(simulation, 8, MatchCommandType.StartSecondHalf, 2700);
            Assert.That(simulation.Snapshot.Half, Is.EqualTo(2));
            AssertAccepted(simulation, 9, MatchCommandType.ResumePlay, 2700);
            AssertAccepted(simulation, 10, MatchCommandType.EndMatch, 5400);

            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.FullTime));
        }

        [Test]
        public void SameSeedAndCommands_ProduceSameSnapshotAndEvents()
        {
            DeterministicMatchSimulation first = CreateInPlay(99);
            DeterministicMatchSimulation second = CreateInPlay(99);

            MatchCommand[] commands =
            {
                new MatchCommand(3, 1, "p1", MatchCommandType.AdvanceTick, tickDelta: 1),
                new MatchCommand(4, 1, "p1", MatchCommandType.AwardGoal, teamIndex: 0),
                new MatchCommand(5, 2, "rules", MatchCommandType.RecordCorner),
                new MatchCommand(6, 2, "rules", MatchCommandType.ResumePlay)
            };

            foreach (MatchCommand command in commands)
            {
                Assert.That(first.Apply(command).Accepted, Is.True);
                Assert.That(second.Apply(command).Accepted, Is.True);
            }

            Assert.That(first.Snapshot.StateHash, Is.EqualTo(second.Snapshot.StateHash));
            Assert.That(first.Events.Count, Is.EqualTo(second.Events.Count));
            for (int index = 0; index < first.Events.Count; index++)
            {
                Assert.That(first.Events[index].Type, Is.EqualTo(second.Events[index].Type));
                Assert.That(first.Events[index].Tick, Is.EqualTo(second.Events[index].Tick));
                Assert.That(first.Events[index].ReasonCode, Is.EqualTo(second.Events[index].ReasonCode));
            }
        }

        [Test]
        public void DuplicateSequence_IsRejectedWithoutChangingSnapshot()
        {
            DeterministicMatchSimulation simulation = CreateInPlay(7);
            ulong stateHash = simulation.Snapshot.StateHash;

            MatchTransitionResult result = simulation.Apply(new MatchCommand(
                2,
                0,
                "duplicate",
                MatchCommandType.AdvanceTick,
                tickDelta: 1));

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.ReasonCode, Is.EqualTo(DefaultRuleEngine.InvalidSequence));
            Assert.That(result.Snapshot.StateHash, Is.EqualTo(stateHash));
        }

        [Test]
        public void InvalidTransition_IsRejected()
        {
            var simulation = new DeterministicMatchSimulation(1);

            MatchTransitionResult result = simulation.Apply(new MatchCommand(
                1,
                0,
                "rules",
                MatchCommandType.EndMatch));

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.ReasonCode, Is.EqualTo(DefaultRuleEngine.InvalidTransition));
            Assert.That(simulation.Snapshot.Phase, Is.EqualTo(MatchPhase.PreMatch));
        }

        [Test]
        public void InvalidGoalTeam_IsRejected()
        {
            DeterministicMatchSimulation simulation = CreateInPlay(3);

            MatchTransitionResult result = simulation.Apply(new MatchCommand(
                3,
                0,
                "rules",
                MatchCommandType.AwardGoal,
                teamIndex: 2));

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.ReasonCode, Is.EqualTo(DefaultRuleEngine.InvalidTeam));
            Assert.That(simulation.Snapshot.HomeScore, Is.Zero);
            Assert.That(simulation.Snapshot.AwayScore, Is.Zero);
        }

        [Test]
        public void EventBuffer_StaysBounded()
        {
            const int maxEvents = 8;
            var simulation = new DeterministicMatchSimulation(5, maxEvents);
            AssertAccepted(simulation, 1, MatchCommandType.StartMatch, 0);
            AssertAccepted(simulation, 2, MatchCommandType.ResumePlay, 0);

            for (long sequence = 3; sequence < 30; sequence++)
            {
                MatchTransitionResult result = simulation.Apply(new MatchCommand(
                    sequence,
                    (int)sequence - 2,
                    "clock",
                    MatchCommandType.AdvanceTick,
                    tickDelta: 1));
                Assert.That(result.Accepted, Is.True);
            }

            Assert.That(simulation.Events.Count, Is.EqualTo(maxEvents));
        }

        private static DeterministicMatchSimulation CreateInPlay(int seed)
        {
            var simulation = new DeterministicMatchSimulation(seed);
            AssertAccepted(simulation, 1, MatchCommandType.StartMatch, 0);
            AssertAccepted(simulation, 2, MatchCommandType.ResumePlay, 0);
            return simulation;
        }

        private static void AssertAccepted(
            DeterministicMatchSimulation simulation,
            long sequence,
            MatchCommandType type,
            int tick)
        {
            MatchTransitionResult result = simulation.Apply(new MatchCommand(
                sequence,
                tick,
                "test",
                type));
            Assert.That(result.Accepted, Is.True, result.ReasonCode);
        }
    }
}
