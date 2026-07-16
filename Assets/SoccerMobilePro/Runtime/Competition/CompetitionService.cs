using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SoccerMobilePro.Competition
{
    public sealed class CompetitionFeatureOptions
    {
        public bool Enabled { get; set; }
    }

    public sealed class DefaultResultVerifier : IResultVerifier
    {
        public CompetitionFailureCode Verify(TournamentSnapshot snapshot, TournamentMatch match, MatchResultSubmission submission)
        {
            if (match == null || match.State != TournamentMatchState.Scheduled) return CompetitionFailureCode.InvalidTransition;
            if (!string.Equals(match.HomeParticipantId, submission.HomeParticipantId, StringComparison.Ordinal) ||
                !string.Equals(match.AwayParticipantId, submission.AwayParticipantId, StringComparison.Ordinal))
                return CompetitionFailureCode.VerificationFailed;
            if (submission.HomeScore < 0 || submission.AwayScore < 0) return CompetitionFailureCode.InvalidRequest;
            if (!snapshot.Definition.Rules.AllowDraw && submission.HomeScore == submission.AwayScore)
                return CompetitionFailureCode.VerificationFailed;
            return CompetitionFailureCode.None;
        }
    }

    public sealed class InMemoryCompetitionRepository : ICompetitionRepository
    {
        private readonly Dictionary<string, TournamentSnapshot> snapshots = new Dictionary<string, TournamentSnapshot>(StringComparer.Ordinal);
        public bool IsReadOnly { get; set; }

        public bool TryLoad(string tournamentId, out TournamentSnapshot snapshot)
        {
            if (!snapshots.TryGetValue(tournamentId ?? string.Empty, out TournamentSnapshot stored))
            {
                snapshot = null;
                return false;
            }
            snapshot = CompetitionClone.Snapshot(stored);
            return true;
        }

        public bool TryCommit(string tournamentId, long expectedRevision, TournamentSnapshot next)
        {
            if (IsReadOnly || next == null) return false;
            bool exists = snapshots.TryGetValue(tournamentId ?? string.Empty, out TournamentSnapshot current);
            if ((exists ? current.Revision : -1L) != expectedRevision) return false;
            snapshots[tournamentId] = CompetitionClone.Snapshot(next);
            return true;
        }
    }

    public sealed class InMemoryRewardGrantGateway : IRewardGrantGateway
    {
        private readonly Dictionary<string, string> receipts = new Dictionary<string, string>(StringComparer.Ordinal);
        public int GrantCount => receipts.Count;

        public bool TryGrantOnce(string rewardReferenceId, string participantId, string rewardPolicyId, out string rewardReceiptId)
        {
            if (string.IsNullOrWhiteSpace(rewardReferenceId) || string.IsNullOrWhiteSpace(participantId) || string.IsNullOrWhiteSpace(rewardPolicyId))
            {
                rewardReceiptId = string.Empty;
                return false;
            }
            if (!receipts.TryGetValue(rewardReferenceId, out rewardReceiptId))
            {
                rewardReceiptId = "reward-" + StableHash.Sha256(rewardReferenceId).Substring(0, 16);
                receipts.Add(rewardReferenceId, rewardReceiptId);
            }
            return true;
        }
    }

    public sealed class CompetitionService : ICompetitionService
    {
        private readonly ICompetitionRepository repository;
        private readonly IResultVerifier verifier;
        private readonly IRewardGrantGateway rewards;
        private readonly CompetitionFeatureOptions options;

        public CompetitionService(ICompetitionRepository repository, IResultVerifier verifier, IRewardGrantGateway rewards, CompetitionFeatureOptions options = null)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
            this.rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            this.options = options ?? new CompetitionFeatureOptions();
        }

        public CompetitionOperationResult Create(TournamentDefinition definition)
        {
            CompetitionFailureCode validation = ValidateDefinition(definition);
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (repository.IsReadOnly) return Failure(CompetitionFailureCode.ReadOnly);
            if (validation != CompetitionFailureCode.None) return Failure(validation);
            var snapshot = new TournamentSnapshot
            {
                TournamentId = definition.TournamentId,
                Revision = 0,
                State = TournamentState.Registration,
                Definition = CompetitionClone.Definition(definition),
                Standings = definition.ParticipantIds.OrderBy(id => id, StringComparer.Ordinal)
                    .Select(id => new StandingEntry { ParticipantId = id }).ToList()
            };
            return repository.TryCommit(definition.TournamentId, -1, snapshot) ? Success(snapshot) : Failure(CompetitionFailureCode.PersistenceFailed);
        }

        public CompetitionOperationResult SubmitRoster(RosterSubmission submission)
        {
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (submission == null || !repository.TryLoad(submission.TournamentId, out TournamentSnapshot snapshot)) return Failure(CompetitionFailureCode.NotFound);
            if (snapshot.State != TournamentState.Registration) return Failure(CompetitionFailureCode.RosterLocked, snapshot);
            if (snapshot.Revision != submission.ExpectedSnapshotRevision) return Failure(CompetitionFailureCode.StaleRevision, snapshot);
            if (!string.Equals(snapshot.Definition.Rules.RulesVersion, submission.RulesVersion, StringComparison.Ordinal)) return Failure(CompetitionFailureCode.StaleRules, snapshot);
            if (!string.Equals(snapshot.Definition.CatalogVersion, submission.CatalogVersion, StringComparison.Ordinal)) return Failure(CompetitionFailureCode.StaleCatalog, snapshot);
            if (!snapshot.Definition.ParticipantIds.Contains(submission.ParticipantId) || submission.PlayerIdentityIds == null ||
                submission.PlayerIdentityIds.Count != snapshot.Definition.Rules.RosterSize ||
                submission.PlayerIdentityIds.Any(string.IsNullOrWhiteSpace) || submission.PlayerIdentityIds.Distinct(StringComparer.Ordinal).Count() != submission.PlayerIdentityIds.Count)
                return Failure(CompetitionFailureCode.Ineligible, snapshot);

            snapshot.Rosters.RemoveAll(item => string.Equals(item.ParticipantId, submission.ParticipantId, StringComparison.Ordinal));
            snapshot.Rosters.Add(CompetitionClone.Roster(submission));
            return Commit(snapshot, submission.ExpectedSnapshotRevision);
        }

        public CompetitionOperationResult Start(string tournamentId, long expectedRevision)
        {
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (!repository.TryLoad(tournamentId, out TournamentSnapshot snapshot)) return Failure(CompetitionFailureCode.NotFound);
            if (snapshot.Revision != expectedRevision) return Failure(CompetitionFailureCode.StaleRevision, snapshot);
            if (snapshot.State != TournamentState.Registration) return Failure(CompetitionFailureCode.InvalidTransition, snapshot);
            if (snapshot.Rosters.Select(item => item.ParticipantId).Distinct(StringComparer.Ordinal).Count() != snapshot.Definition.ParticipantIds.Count)
                return Failure(CompetitionFailureCode.Ineligible, snapshot);
            snapshot.Matches = TournamentProjection.CreateInitialMatches(snapshot.Definition);
            snapshot.State = TournamentState.Running;
            return Commit(snapshot, expectedRevision);
        }

        public AuthoritativeResultReceipt SubmitResult(MatchResultSubmission submission, DateTimeOffset now)
        {
            if (!options.Enabled) return Rejected(submission, CompetitionFailureCode.Disabled, now);
            if (submission == null || !repository.TryLoad(submission.TournamentId, out TournamentSnapshot snapshot)) return Rejected(submission, CompetitionFailureCode.NotFound, now);

            AuthoritativeResultReceipt previous = snapshot.Receipts.FirstOrDefault(item => string.Equals(item.IdempotencyKey, submission.IdempotencyKey, StringComparison.Ordinal));
            if (previous != null)
            {
                if (!string.Equals(previous.PayloadHash, submission.PayloadHash, StringComparison.Ordinal)) return Rejected(submission, CompetitionFailureCode.IdempotencyConflict, now, snapshot.Revision);
                AuthoritativeResultReceipt replay = CompetitionClone.Receipt(previous);
                replay.Status = ResultReceiptStatus.Replayed;
                return replay;
            }

            CompetitionFailureCode precondition = ValidateResultPreconditions(snapshot, submission);
            if (precondition != CompetitionFailureCode.None) return Rejected(submission, precondition, now, snapshot.Revision);
            TournamentMatch match = snapshot.Matches.FirstOrDefault(item => string.Equals(item.MatchId, submission.MatchId, StringComparison.Ordinal));
            CompetitionFailureCode verification = verifier.Verify(snapshot, match, submission);
            if (verification != CompetitionFailureCode.None) return Rejected(submission, verification, now, snapshot.Revision);

            match.HomeScore = submission.HomeScore;
            match.AwayScore = submission.AwayScore;
            match.WinnerParticipantId = submission.HomeScore > submission.AwayScore ? submission.HomeParticipantId :
                submission.AwayScore > submission.HomeScore ? submission.AwayParticipantId : string.Empty;
            match.ResultSequence = submission.Sequence;
            match.State = TournamentMatchState.Completed;
            snapshot.LastResultSequence = submission.Sequence;
            TournamentProjection.Advance(snapshot);

            var receipt = new AuthoritativeResultReceipt
            {
                ReceiptId = "result-" + StableHash.Sha256(submission.TournamentId + "|" + submission.IdempotencyKey).Substring(0, 16),
                TournamentId = submission.TournamentId,
                MatchId = submission.MatchId,
                Status = ResultReceiptStatus.Accepted,
                FailureCode = CompetitionFailureCode.None,
                IdempotencyKey = submission.IdempotencyKey,
                PayloadHash = submission.PayloadHash,
                Sequence = submission.Sequence,
                SnapshotRevision = snapshot.Revision + 1,
                CreatedAt = now
            };

            if (snapshot.State == TournamentState.Completed && snapshot.Definition.OnlineRewardsEnabled)
            {
                string champion = TournamentProjection.Champion(snapshot);
                receipt.RewardReferenceId = snapshot.TournamentId + ":champion";
                if (!rewards.TryGrantOnce(receipt.RewardReferenceId, champion, snapshot.Definition.Rules.RewardPolicyId, out string rewardReceipt))
                    return Rejected(submission, CompetitionFailureCode.RewardFailed, now, snapshot.Revision);
                receipt.RewardReceiptId = rewardReceipt;
            }

            snapshot.Receipts.Add(receipt);
            long expected = snapshot.Revision;
            snapshot.Revision++;
            if (!repository.TryCommit(snapshot.TournamentId, expected, snapshot)) return Rejected(submission, CompetitionFailureCode.PersistenceFailed, now, expected);
            return receipt;
        }

        public CompetitionOperationResult TransitionReconnect(string tournamentId, long expectedRevision, string matchId, string participantId, ReconnectState next, string reasonCode, DateTimeOffset now)
        {
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (!repository.TryLoad(tournamentId, out TournamentSnapshot snapshot)) return Failure(CompetitionFailureCode.NotFound);
            if (snapshot.Revision != expectedRevision) return Failure(CompetitionFailureCode.StaleRevision, snapshot);
            TournamentMatch match = snapshot.Matches.FirstOrDefault(item => item.MatchId == matchId);
            if (match == null || (match.HomeParticipantId != participantId && match.AwayParticipantId != participantId)) return Failure(CompetitionFailureCode.Ineligible, snapshot);
            ReconnectTimeline timeline = snapshot.ReconnectTimelines.FirstOrDefault(item => item.MatchId == matchId && item.ParticipantId == participantId);
            if (timeline == null)
            {
                timeline = new ReconnectTimeline { MatchId = matchId, ParticipantId = participantId };
                snapshot.ReconnectTimelines.Add(timeline);
            }
            if (!ReconnectTransitions.CanMove(timeline.State, next)) return Failure(CompetitionFailureCode.InvalidTransition, snapshot);
            timeline.Transitions.Add(new ReconnectTransition { Sequence = timeline.Transitions.Count + 1, From = timeline.State, To = next, ReasonCode = reasonCode ?? string.Empty, OccurredAt = now });
            timeline.State = next;
            return Commit(snapshot, expectedRevision);
        }

        public CompetitionOperationResult AppendDispute(string tournamentId, long expectedRevision, DisputeRecord record)
        {
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (!repository.TryLoad(tournamentId, out TournamentSnapshot snapshot)) return Failure(CompetitionFailureCode.NotFound);
            if (snapshot.Revision != expectedRevision) return Failure(CompetitionFailureCode.StaleRevision, snapshot);
            if (record == null || string.IsNullOrWhiteSpace(record.DisputeId) || snapshot.Disputes.Any(item => item.DisputeId == record.DisputeId) ||
                !snapshot.Matches.Any(item => item.MatchId == record.MatchId && item.State != TournamentMatchState.Scheduled))
                return Failure(CompetitionFailureCode.InvalidRequest, snapshot);
            snapshot.Disputes.Add(CompetitionClone.Dispute(record));
            return Commit(snapshot, expectedRevision);
        }

        public CompetitionOperationResult CorrectResult(string tournamentId, long expectedRevision, string disputeId, int homeScore, int awayScore, DateTimeOffset now)
        {
            if (!options.Enabled) return Failure(CompetitionFailureCode.Disabled);
            if (!repository.TryLoad(tournamentId, out TournamentSnapshot snapshot)) return Failure(CompetitionFailureCode.NotFound);
            if (snapshot.Revision != expectedRevision) return Failure(CompetitionFailureCode.StaleRevision, snapshot);
            DisputeRecord dispute = snapshot.Disputes.LastOrDefault(item => item.DisputeId == disputeId);
            TournamentMatch match = dispute == null ? null : snapshot.Matches.FirstOrDefault(item => item.MatchId == dispute.MatchId);
            if (match == null || homeScore < 0 || awayScore < 0 || (!snapshot.Definition.Rules.AllowDraw && homeScore == awayScore))
                return Failure(CompetitionFailureCode.InvalidRequest, snapshot);
            snapshot.Disputes.Add(new DisputeRecord
            {
                DisputeId = dispute.DisputeId + ":correction:" + (snapshot.Disputes.Count(item => item.DisputeId.StartsWith(dispute.DisputeId, StringComparison.Ordinal)) + 1),
                MatchId = dispute.MatchId, SubmittedBy = "authority", CorrelationId = dispute.CorrelationId,
                State = DisputeState.Corrected, ReasonCode = "authoritative_correction",
                CorrectedHomeScore = homeScore, CorrectedAwayScore = awayScore, OccurredAt = now
            });
            match.HomeScore = homeScore;
            match.AwayScore = awayScore;
            match.WinnerParticipantId = homeScore > awayScore ? match.HomeParticipantId : awayScore > homeScore ? match.AwayParticipantId : string.Empty;
            match.State = TournamentMatchState.Corrected;
            TournamentProjection.Rebuild(snapshot);
            return Commit(snapshot, expectedRevision);
        }

        private CompetitionOperationResult Commit(TournamentSnapshot snapshot, long expectedRevision)
        {
            snapshot.Revision = expectedRevision + 1;
            return repository.TryCommit(snapshot.TournamentId, expectedRevision, snapshot) ? Success(snapshot) : Failure(CompetitionFailureCode.PersistenceFailed);
        }

        private static CompetitionFailureCode ValidateDefinition(TournamentDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.TournamentId) || string.IsNullOrWhiteSpace(definition.CatalogVersion) ||
                definition.Rules == null || string.IsNullOrWhiteSpace(definition.Rules.RulesVersion) || definition.ParticipantIds == null)
                return CompetitionFailureCode.InvalidRequest;
            int count = definition.ParticipantIds.Count;
            if (count < definition.Rules.MinimumParticipants || count > definition.Rules.MaximumParticipants ||
                definition.ParticipantIds.Any(string.IsNullOrWhiteSpace) || definition.ParticipantIds.Distinct(StringComparer.Ordinal).Count() != count || definition.Rules.RosterSize < 1)
                return CompetitionFailureCode.Ineligible;
            if (definition.Rules.Format == TournamentFormat.SingleElimination && (count & (count - 1)) != 0) return CompetitionFailureCode.Ineligible;
            if (definition.OnlineRewardsEnabled && string.IsNullOrWhiteSpace(definition.Rules.RewardPolicyId)) return CompetitionFailureCode.InvalidRequest;
            return CompetitionFailureCode.None;
        }

        private static CompetitionFailureCode ValidateResultPreconditions(TournamentSnapshot snapshot, MatchResultSubmission submission)
        {
            if (snapshot.State != TournamentState.Running) return CompetitionFailureCode.InvalidTransition;
            if (snapshot.Revision != submission.ExpectedSnapshotRevision) return CompetitionFailureCode.StaleRevision;
            if (!string.Equals(snapshot.Definition.Rules.RulesVersion, submission.RulesVersion, StringComparison.Ordinal)) return CompetitionFailureCode.StaleRules;
            if (!string.Equals(snapshot.Definition.CatalogVersion, submission.CatalogVersion, StringComparison.Ordinal)) return CompetitionFailureCode.StaleCatalog;
            if (submission.Sequence != snapshot.LastResultSequence + 1) return CompetitionFailureCode.ResultOutOfOrder;
            if (string.IsNullOrWhiteSpace(submission.IdempotencyKey) || !string.Equals(submission.PayloadHash, ResultPayloadHasher.Compute(submission), StringComparison.Ordinal))
                return CompetitionFailureCode.VerificationFailed;
            return CompetitionFailureCode.None;
        }

        private static CompetitionOperationResult Success(TournamentSnapshot snapshot) => new CompetitionOperationResult { Succeeded = true, Snapshot = CompetitionClone.Snapshot(snapshot) };
        private static CompetitionOperationResult Failure(CompetitionFailureCode code, TournamentSnapshot snapshot = null) => new CompetitionOperationResult { FailureCode = code, Snapshot = snapshot == null ? null : CompetitionClone.Snapshot(snapshot) };
        private static AuthoritativeResultReceipt Rejected(MatchResultSubmission submission, CompetitionFailureCode code, DateTimeOffset now, long revision = -1) => new AuthoritativeResultReceipt
        {
            TournamentId = submission?.TournamentId ?? string.Empty, MatchId = submission?.MatchId ?? string.Empty,
            Status = ResultReceiptStatus.Rejected, FailureCode = code, IdempotencyKey = submission?.IdempotencyKey ?? string.Empty,
            PayloadHash = submission?.PayloadHash ?? string.Empty, Sequence = submission?.Sequence ?? 0, SnapshotRevision = revision, CreatedAt = now
        };
    }

    public static class ResultPayloadHasher
    {
        public static string Compute(MatchResultSubmission value)
        {
            if (value == null) return string.Empty;
            return StableHash.Sha256(string.Join("|", value.TournamentId, value.MatchId, value.RulesVersion, value.CatalogVersion,
                value.ExpectedSnapshotRevision, value.Sequence, value.HomeParticipantId, value.AwayParticipantId,
                value.HomeScore, value.AwayScore, value.MatchEventDigest));
        }
    }

    internal static class StableHash
    {
        public static string Sha256(string value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte item in bytes) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }
    }

    internal static class CompetitionClone
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.DateTimeOffset,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
        private static T Copy<T>(T value) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value, Settings), Settings);
        public static TournamentSnapshot Snapshot(TournamentSnapshot value) => Copy(value);
        public static TournamentDefinition Definition(TournamentDefinition value) => Copy(value);
        public static RosterSubmission Roster(RosterSubmission value) => Copy(value);
        public static AuthoritativeResultReceipt Receipt(AuthoritativeResultReceipt value) => Copy(value);
        public static DisputeRecord Dispute(DisputeRecord value) => Copy(value);
    }

    internal static class ReconnectTransitions
    {
        public static bool CanMove(ReconnectState from, ReconnectState to)
        {
            switch (from)
            {
                case ReconnectState.Connected: return to == ReconnectState.Interrupted;
                case ReconnectState.Interrupted: return to == ReconnectState.Reconnecting || to == ReconnectState.ForfeitPending;
                case ReconnectState.Reconnecting: return to == ReconnectState.Resynced || to == ReconnectState.ForfeitPending;
                case ReconnectState.Resynced: return to == ReconnectState.Resolved || to == ReconnectState.Interrupted;
                case ReconnectState.ForfeitPending: return to == ReconnectState.Resolved || to == ReconnectState.Reconnecting;
                default: return false;
            }
        }
    }

    internal static class TournamentProjection
    {
        public static List<TournamentMatch> CreateInitialMatches(TournamentDefinition definition)
        {
            var result = new List<TournamentMatch>();
            List<string> participants = definition.ParticipantIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
            if (definition.Rules.Format == TournamentFormat.SingleElimination)
            {
                for (int index = 0; index < participants.Count; index += 2)
                    result.Add(NewMatch(definition.TournamentId, 1, index / 2 + 1, participants[index], participants[index + 1]));
            }
            else
            {
                int index = 1;
                for (int home = 0; home < participants.Count; home++)
                    for (int away = home + 1; away < participants.Count; away++)
                        result.Add(NewMatch(definition.TournamentId, 1, index++, participants[home], participants[away]));
            }
            return result;
        }

        public static void Advance(TournamentSnapshot snapshot)
        {
            RebuildStandings(snapshot);
            if (snapshot.Definition.Rules.Format == TournamentFormat.RoundRobin)
            {
                if (snapshot.Matches.All(item => item.State != TournamentMatchState.Scheduled)) snapshot.State = TournamentState.Completed;
                return;
            }
            int currentRound = snapshot.Matches.Max(item => item.Round);
            List<TournamentMatch> round = snapshot.Matches.Where(item => item.Round == currentRound).OrderBy(item => item.Index).ToList();
            if (round.Any(item => item.State == TournamentMatchState.Scheduled)) return;
            if (round.Count == 1)
            {
                snapshot.State = TournamentState.Completed;
                return;
            }
            for (int index = 0; index < round.Count; index += 2)
                snapshot.Matches.Add(NewMatch(snapshot.TournamentId, currentRound + 1, index / 2 + 1, round[index].WinnerParticipantId, round[index + 1].WinnerParticipantId));
        }

        public static void Rebuild(TournamentSnapshot snapshot)
        {
            RebuildStandings(snapshot);
            if (snapshot.Definition.Rules.Format == TournamentFormat.SingleElimination)
            {
                List<TournamentMatch> completed = snapshot.Matches.Where(item => item.State != TournamentMatchState.Scheduled).OrderBy(item => item.Round).ThenBy(item => item.Index).ToList();
                foreach (TournamentMatch match in snapshot.Matches.Where(item => item.Round > 1))
                {
                    List<TournamentMatch> parents = snapshot.Matches.Where(item => item.Round == match.Round - 1).OrderBy(item => item.Index).Skip((match.Index - 1) * 2).Take(2).ToList();
                    if (parents.Count == 2)
                    {
                        match.HomeParticipantId = parents[0].WinnerParticipantId;
                        match.AwayParticipantId = parents[1].WinnerParticipantId;
                    }
                }
                snapshot.State = snapshot.Matches.Count > 0 && snapshot.Matches.OrderByDescending(item => item.Round).First().State != TournamentMatchState.Scheduled ? TournamentState.Completed : TournamentState.Running;
            }
        }

        public static string Champion(TournamentSnapshot snapshot)
        {
            if (snapshot.Definition.Rules.Format == TournamentFormat.SingleElimination)
                return snapshot.Matches.OrderByDescending(item => item.Round).ThenBy(item => item.Index).First().WinnerParticipantId;
            return snapshot.Standings.OrderBy(item => item.Rank).First().ParticipantId;
        }

        private static void RebuildStandings(TournamentSnapshot snapshot)
        {
            var table = snapshot.Definition.ParticipantIds.ToDictionary(id => id, id => new StandingEntry { ParticipantId = id }, StringComparer.Ordinal);
            foreach (TournamentMatch match in snapshot.Matches.Where(item => item.State != TournamentMatchState.Scheduled))
            {
                StandingEntry home = table[match.HomeParticipantId];
                StandingEntry away = table[match.AwayParticipantId];
                home.Played++; away.Played++;
                home.GoalsFor += match.HomeScore; home.GoalsAgainst += match.AwayScore;
                away.GoalsFor += match.AwayScore; away.GoalsAgainst += match.HomeScore;
                if (match.HomeScore > match.AwayScore) { home.Won++; away.Lost++; home.Points += snapshot.Definition.Rules.WinPoints; away.Points += snapshot.Definition.Rules.LossPoints; }
                else if (match.AwayScore > match.HomeScore) { away.Won++; home.Lost++; away.Points += snapshot.Definition.Rules.WinPoints; home.Points += snapshot.Definition.Rules.LossPoints; }
                else { home.Drawn++; away.Drawn++; home.Points += snapshot.Definition.Rules.DrawPoints; away.Points += snapshot.Definition.Rules.DrawPoints; }
            }
            List<StandingEntry> ordered = table.Values.ToList();
            ordered.Sort((left, right) => CompareStanding(left, right, snapshot.Definition.Rules.TieBreakOrder));
            snapshot.Standings = ordered.Select((item, index) => { item.Rank = index + 1; return item; }).ToList();
        }

        private static int CompareStanding(StandingEntry left, StandingEntry right, IReadOnlyList<TieBreakRule> rules)
        {
            IReadOnlyList<TieBreakRule> effective = rules == null || rules.Count == 0
                ? new[] { TieBreakRule.Points, TieBreakRule.GoalDifference, TieBreakRule.GoalsFor, TieBreakRule.ParticipantId }
                : rules;
            foreach (TieBreakRule rule in effective)
            {
                int comparison;
                switch (rule)
                {
                    case TieBreakRule.Points: comparison = right.Points.CompareTo(left.Points); break;
                    case TieBreakRule.GoalDifference: comparison = right.GoalDifference.CompareTo(left.GoalDifference); break;
                    case TieBreakRule.GoalsFor: comparison = right.GoalsFor.CompareTo(left.GoalsFor); break;
                    default: comparison = string.Compare(left.ParticipantId, right.ParticipantId, StringComparison.Ordinal); break;
                }
                if (comparison != 0) return comparison;
            }
            return string.Compare(left.ParticipantId, right.ParticipantId, StringComparison.Ordinal);
        }

        private static TournamentMatch NewMatch(string tournamentId, int round, int index, string home, string away) => new TournamentMatch
        {
            MatchId = tournamentId + ":r" + round + ":m" + index, Round = round, Index = index,
            HomeParticipantId = home, AwayParticipantId = away, State = TournamentMatchState.Scheduled
        };
    }
}
