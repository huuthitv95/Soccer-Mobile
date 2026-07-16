using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoccerMobilePro.Competition;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class CompetitionServiceTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
        private InMemoryCompetitionRepository repository;
        private InMemoryRewardGrantGateway rewards;
        private CompetitionService service;

        [SetUp]
        public void SetUp()
        {
            repository = new InMemoryCompetitionRepository();
            rewards = new InMemoryRewardGrantGateway();
            service = new CompetitionService(repository, new DefaultResultVerifier(), rewards, new CompetitionFeatureOptions { Enabled = true });
        }

        [Test]
        public void FeatureGate_IsDisabledByDefault()
        {
            var disabled = new CompetitionService(repository, new DefaultResultVerifier(), rewards);
            Assert.That(disabled.Create(Definition(TournamentFormat.SingleElimination, 2)).FailureCode, Is.EqualTo(CompetitionFailureCode.Disabled));
        }

        [Test]
        public void SingleElimination_BracketAndChampionAreDeterministic()
        {
            TournamentSnapshot first = Start(Definition(TournamentFormat.SingleElimination, 4));
            var secondRepository = new InMemoryCompetitionRepository();
            var secondService = new CompetitionService(secondRepository, new DefaultResultVerifier(), new InMemoryRewardGrantGateway(), new CompetitionFeatureOptions { Enabled = true });
            TournamentDefinition copy = Definition(TournamentFormat.SingleElimination, 4);
            CreateAndRoster(secondService, secondRepository, copy);
            secondRepository.TryLoad(copy.TournamentId, out TournamentSnapshot beforeStart);
            TournamentSnapshot second = secondService.Start(copy.TournamentId, beforeStart.Revision).Snapshot;

            CollectionAssert.AreEqual(first.Matches.Select(item => item.MatchId), second.Matches.Select(item => item.MatchId));
            CollectionAssert.AreEqual(first.Matches.Select(item => item.HomeParticipantId + ":" + item.AwayParticipantId), second.Matches.Select(item => item.HomeParticipantId + ":" + item.AwayParticipantId));

            Submit(first.Matches[0].MatchId, "a", "b", 2, 0, 1);
            repository.TryLoad(first.TournamentId, out TournamentSnapshot current);
            Submit(current.Matches.Single(item => item.Round == 1 && item.Index == 2).MatchId, "c", "d", 1, 0, 2);
            repository.TryLoad(first.TournamentId, out current);
            TournamentMatch final = current.Matches.Single(item => item.Round == 2);
            AuthoritativeResultReceipt receipt = Submit(final.MatchId, "a", "c", 3, 1, 3);

            Assert.That(receipt.Status, Is.EqualTo(ResultReceiptStatus.Accepted));
            repository.TryLoad(first.TournamentId, out current);
            Assert.That(current.State, Is.EqualTo(TournamentState.Completed));
            Assert.That(receipt.RewardReferenceId, Is.EqualTo("cup:champion"));
            Assert.That(rewards.GrantCount, Is.EqualTo(1));
        }

        [Test]
        public void RoundRobin_StandingsUseDeterministicTieBreak()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.RoundRobin, 3));
            Submit(snapshot.Matches[0].MatchId, "a", "b", 1, 0, 1);
            repository.TryLoad("cup", out snapshot);
            Submit(snapshot.Matches[1].MatchId, "a", "c", 0, 1, 2);
            repository.TryLoad("cup", out snapshot);
            Submit(snapshot.Matches[2].MatchId, "b", "c", 2, 0, 3);
            repository.TryLoad("cup", out snapshot);

            Assert.That(snapshot.State, Is.EqualTo(TournamentState.Completed));
            CollectionAssert.AreEqual(new[] { "b", "a", "c" }, snapshot.Standings.Select(item => item.ParticipantId));
            CollectionAssert.AreEqual(new[] { 3, 3, 3 }, snapshot.Standings.Select(item => item.Points));
        }

        [Test]
        public void RoundRobin_UsesVersionedTieBreakOrder()
        {
            TournamentDefinition definition = Definition(TournamentFormat.RoundRobin, 3);
            definition.Rules.TieBreakOrder = new List<TieBreakRule> { TieBreakRule.Points, TieBreakRule.ParticipantId };
            TournamentSnapshot snapshot = Start(definition);
            Submit(snapshot.Matches[0].MatchId, "a", "b", 1, 0, 1);
            repository.TryLoad("cup", out snapshot);
            Submit(snapshot.Matches[1].MatchId, "a", "c", 0, 1, 2);
            repository.TryLoad("cup", out snapshot);
            Submit(snapshot.Matches[2].MatchId, "b", "c", 2, 0, 3);
            repository.TryLoad("cup", out snapshot);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, snapshot.Standings.Select(item => item.ParticipantId));
        }

        [TestCase("rules", CompetitionFailureCode.StaleRules)]
        [TestCase("catalog", CompetitionFailureCode.StaleCatalog)]
        [TestCase("revision", CompetitionFailureCode.StaleRevision)]
        public void StaleResult_IsRejectedWithoutMutation(string kind, CompetitionFailureCode expected)
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.SingleElimination, 2));
            MatchResultSubmission command = Result(snapshot, snapshot.Matches[0], 1, 0, 1, "result-stale");
            if (kind == "rules") command.RulesVersion = "000000000099";
            if (kind == "catalog") command.CatalogVersion = "000000000199";
            if (kind == "revision") command.ExpectedSnapshotRevision--;
            command.PayloadHash = ResultPayloadHasher.Compute(command);

            AuthoritativeResultReceipt receipt = service.SubmitResult(command, Now);

            Assert.That(receipt.FailureCode, Is.EqualTo(expected));
            repository.TryLoad("cup", out TournamentSnapshot current);
            Assert.That(current.Revision, Is.EqualTo(snapshot.Revision));
        }

        [Test]
        public void DuplicateResult_ReplaysReceiptWithoutDuplicateReward()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.SingleElimination, 2));
            MatchResultSubmission command = Result(snapshot, snapshot.Matches[0], 2, 1, 1, "result-once");

            AuthoritativeResultReceipt first = service.SubmitResult(command, Now);
            AuthoritativeResultReceipt retry = service.SubmitResult(command, Now.AddSeconds(1));

            Assert.That(first.Status, Is.EqualTo(ResultReceiptStatus.Accepted));
            Assert.That(retry.Status, Is.EqualTo(ResultReceiptStatus.Replayed));
            Assert.That(retry.ReceiptId, Is.EqualTo(first.ReceiptId));
            Assert.That(rewards.GrantCount, Is.EqualTo(1));
        }

        [Test]
        public void SameIdempotencyKeyDifferentPayload_IsConflict()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.SingleElimination, 2));
            MatchResultSubmission command = Result(snapshot, snapshot.Matches[0], 2, 1, 1, "result-conflict");
            Assert.That(service.SubmitResult(command, Now).Status, Is.EqualTo(ResultReceiptStatus.Accepted));
            command.HomeScore = 4;
            command.PayloadHash = ResultPayloadHasher.Compute(command);

            Assert.That(service.SubmitResult(command, Now.AddSeconds(1)).FailureCode, Is.EqualTo(CompetitionFailureCode.IdempotencyConflict));
        }

        [Test]
        public void OutOfOrderResult_IsRejected()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.SingleElimination, 2));
            MatchResultSubmission command = Result(snapshot, snapshot.Matches[0], 1, 0, 2, "result-order");
            Assert.That(service.SubmitResult(command, Now).FailureCode, Is.EqualTo(CompetitionFailureCode.ResultOutOfOrder));
        }

        [Test]
        public void OfflineCup_DoesNotGrantOnlineReward()
        {
            TournamentDefinition definition = Definition(TournamentFormat.SingleElimination, 2);
            definition.OnlineRewardsEnabled = false;
            TournamentSnapshot snapshot = Start(definition);
            AuthoritativeResultReceipt receipt = Submit(snapshot.Matches[0].MatchId, "a", "b", 1, 0, 1);
            Assert.That(receipt.Status, Is.EqualTo(ResultReceiptStatus.Accepted));
            Assert.That(receipt.RewardReceiptId, Is.Empty);
            Assert.That(rewards.GrantCount, Is.Zero);
        }

        [Test]
        public void ReconnectTimeline_RejectsInvalidTransitionAndKeepsAppendOnlyOrder()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.SingleElimination, 2));
            string matchId = snapshot.Matches[0].MatchId;
            CompetitionOperationResult interrupted = service.TransitionReconnect("cup", snapshot.Revision, matchId, "a", ReconnectState.Interrupted, "network_lost", Now);
            Assert.That(interrupted.Succeeded, Is.True);
            CompetitionOperationResult invalid = service.TransitionReconnect("cup", interrupted.Snapshot.Revision, matchId, "a", ReconnectState.Resolved, "skip", Now.AddSeconds(1));
            Assert.That(invalid.FailureCode, Is.EqualTo(CompetitionFailureCode.InvalidTransition));
            CompetitionOperationResult reconnecting = service.TransitionReconnect("cup", interrupted.Snapshot.Revision, matchId, "a", ReconnectState.Reconnecting, "retry", Now.AddSeconds(2));
            CompetitionOperationResult resynced = service.TransitionReconnect("cup", reconnecting.Snapshot.Revision, matchId, "a", ReconnectState.Resynced, "snapshot_ok", Now.AddSeconds(3));
            Assert.That(resynced.Snapshot.ReconnectTimelines[0].Transitions.Select(item => item.Sequence), Is.EqualTo(new long[] { 1, 2, 3 }));
        }

        [Test]
        public void DisputeCorrection_IsAppendOnlyAndRebuildsStanding()
        {
            TournamentSnapshot snapshot = Start(Definition(TournamentFormat.RoundRobin, 2));
            Submit(snapshot.Matches[0].MatchId, "a", "b", 1, 0, 1);
            repository.TryLoad("cup", out snapshot);
            var dispute = new DisputeRecord { DisputeId = "dispute-1", MatchId = snapshot.Matches[0].MatchId, SubmittedBy = "b", CorrelationId = "support-1", State = DisputeState.Submitted, ReasonCode = "score", OccurredAt = Now };
            CompetitionOperationResult appended = service.AppendDispute("cup", snapshot.Revision, dispute);
            CompetitionOperationResult corrected = service.CorrectResult("cup", appended.Snapshot.Revision, dispute.DisputeId, 0, 2, Now.AddMinutes(1));

            Assert.That(corrected.Succeeded, Is.True);
            Assert.That(corrected.Snapshot.Disputes, Has.Count.EqualTo(2));
            Assert.That(corrected.Snapshot.Disputes[0].State, Is.EqualTo(DisputeState.Submitted));
            Assert.That(corrected.Snapshot.Disputes[1].State, Is.EqualTo(DisputeState.Corrected));
            Assert.That(corrected.Snapshot.Standings[0].ParticipantId, Is.EqualTo("b"));
        }

        [Test]
        public void RosterLockAndEligibility_AreEnforced()
        {
            TournamentDefinition definition = Definition(TournamentFormat.SingleElimination, 2);
            Assert.That(service.Create(definition).Succeeded, Is.True);
            repository.TryLoad("cup", out TournamentSnapshot snapshot);
            RosterSubmission invalid = Roster(definition, "a", snapshot.Revision);
            invalid.PlayerIdentityIds.RemoveAt(0);
            Assert.That(service.SubmitRoster(invalid).FailureCode, Is.EqualTo(CompetitionFailureCode.Ineligible));
            CreateRemainingRosters(definition);
            repository.TryLoad("cup", out snapshot);
            TournamentSnapshot running = service.Start("cup", snapshot.Revision).Snapshot;
            Assert.That(service.SubmitRoster(Roster(definition, "a", running.Revision)).FailureCode, Is.EqualTo(CompetitionFailureCode.RosterLocked));
        }

        private TournamentSnapshot Start(TournamentDefinition definition)
        {
            CreateAndRoster(service, repository, definition);
            repository.TryLoad(definition.TournamentId, out TournamentSnapshot snapshot);
            CompetitionOperationResult started = service.Start(definition.TournamentId, snapshot.Revision);
            Assert.That(started.Succeeded, Is.True, started.FailureCode.ToString());
            return started.Snapshot;
        }

        private static void CreateAndRoster(CompetitionService target, InMemoryCompetitionRepository targetRepository, TournamentDefinition definition)
        {
            Assert.That(target.Create(definition).Succeeded, Is.True);
            foreach (string participant in definition.ParticipantIds)
            {
                targetRepository.TryLoad(definition.TournamentId, out TournamentSnapshot snapshot);
                Assert.That(target.SubmitRoster(Roster(definition, participant, snapshot.Revision)).Succeeded, Is.True);
            }
        }

        private void CreateRemainingRosters(TournamentDefinition definition)
        {
            foreach (string participant in definition.ParticipantIds)
            {
                repository.TryLoad(definition.TournamentId, out TournamentSnapshot snapshot);
                if (snapshot.Rosters.Any(item => item.ParticipantId == participant)) continue;
                Assert.That(service.SubmitRoster(Roster(definition, participant, snapshot.Revision)).Succeeded, Is.True);
            }
        }

        private AuthoritativeResultReceipt Submit(string matchId, string home, string away, int homeScore, int awayScore, long sequence)
        {
            repository.TryLoad("cup", out TournamentSnapshot snapshot);
            MatchResultSubmission command = Result(snapshot, snapshot.Matches.Single(item => item.MatchId == matchId), homeScore, awayScore, sequence, "result-" + sequence);
            command.HomeParticipantId = home;
            command.AwayParticipantId = away;
            command.PayloadHash = ResultPayloadHasher.Compute(command);
            return service.SubmitResult(command, Now.AddSeconds(sequence));
        }

        private static MatchResultSubmission Result(TournamentSnapshot snapshot, TournamentMatch match, int homeScore, int awayScore, long sequence, string key)
        {
            var result = new MatchResultSubmission
            {
                TournamentId = snapshot.TournamentId, MatchId = match.MatchId,
                RulesVersion = snapshot.Definition.Rules.RulesVersion, CatalogVersion = snapshot.Definition.CatalogVersion,
                ExpectedSnapshotRevision = snapshot.Revision, Sequence = sequence, IdempotencyKey = key,
                HomeParticipantId = match.HomeParticipantId, AwayParticipantId = match.AwayParticipantId,
                HomeScore = homeScore, AwayScore = awayScore, MatchEventDigest = "events-" + sequence, SubmittedAt = Now
            };
            result.PayloadHash = ResultPayloadHasher.Compute(result);
            return result;
        }

        private static TournamentDefinition Definition(TournamentFormat format, int count)
        {
            var participants = new List<string>();
            for (int index = 0; index < count; index++) participants.Add(((char)('a' + index)).ToString());
            return new TournamentDefinition
            {
                TournamentId = "cup", CatalogVersion = "000000000201", ParticipantIds = participants, OnlineRewardsEnabled = true,
                Rules = new CompetitionRules
                {
                    RulesVersion = "000000000101", Format = format, MinimumParticipants = 2, MaximumParticipants = 8,
                    RosterSize = 2, AllowDraw = format == TournamentFormat.RoundRobin, RewardPolicyId = "reward-cup"
                }
            };
        }

        private static RosterSubmission Roster(TournamentDefinition definition, string participant, long revision) => new RosterSubmission
        {
            TournamentId = definition.TournamentId, ParticipantId = participant, CatalogVersion = definition.CatalogVersion,
            RulesVersion = definition.Rules.RulesVersion, ExpectedSnapshotRevision = revision,
            PlayerIdentityIds = new List<string> { participant + "-p1", participant + "-p2" }, SubmittedAt = Now
        };
    }
}
