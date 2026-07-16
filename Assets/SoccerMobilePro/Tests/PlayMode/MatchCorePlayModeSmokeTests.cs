using System.Collections;
using NUnit.Framework;
using SoccerMobilePro.MatchCore;
using SoccerMobilePro.Input;
using SoccerMobilePro.Platform;
using SoccerMobilePro.Competition;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace SoccerMobilePro.MatchCore.PlayModeTests
{
    public sealed class MatchCorePlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator QuickMatchSelectionScene_LoadsFromBuildSettings()
        {
            yield return SceneManager.LoadSceneAsync(SceneIds.GameModeSelection, LoadSceneMode.Single);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneIds.GameModeSelection));
        }

        [UnityTest]
        public IEnumerator CupGroupScene_LoadsFromBuildSettings()
        {
            yield return SceneManager.LoadSceneAsync(SceneIds.TournamentGroups, LoadSceneMode.Single);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneIds.TournamentGroups));
        }

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

        [UnityTest]
        public IEnumerator CompetitionFeatureGate_DefaultsOffWithoutChangingSceneFlow()
        {
            yield return SceneManager.LoadSceneAsync(SceneIds.TournamentGroups, LoadSceneMode.Single);
            var service = new CompetitionService(
                new InMemoryCompetitionRepository(),
                new DefaultResultVerifier(),
                new InMemoryRewardGrantGateway());
            CompetitionOperationResult result = service.Create(new TournamentDefinition());

            Assert.That(result.FailureCode, Is.EqualTo(CompetitionFailureCode.Disabled));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneIds.TournamentGroups));
        }
    }
}
