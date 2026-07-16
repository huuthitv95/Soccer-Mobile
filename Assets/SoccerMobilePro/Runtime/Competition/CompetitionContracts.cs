using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoccerMobilePro.Competition
{
    public enum TournamentFormat { SingleElimination = 0, RoundRobin = 1 }
    public enum TournamentState { Draft = 0, Registration = 1, Running = 2, Completed = 3, Cancelled = 4 }
    public enum TournamentMatchState { Scheduled = 0, Completed = 1, Corrected = 2 }
    public enum ResultReceiptStatus { Accepted = 0, Replayed = 1, Rejected = 2 }
    public enum CompetitionFailureCode
    {
        None = 0, Disabled = 1, InvalidRequest = 2, NotFound = 3, StaleRevision = 4,
        StaleRules = 5, StaleCatalog = 6, RosterLocked = 7, Ineligible = 8,
        ResultOutOfOrder = 9, IdempotencyConflict = 10, VerificationFailed = 11,
        PersistenceFailed = 12, InvalidTransition = 13, RewardFailed = 14, ReadOnly = 15
    }
    public enum TieBreakRule { Points = 0, GoalDifference = 1, GoalsFor = 2, ParticipantId = 3 }
    public enum ReconnectState { Connected = 0, Interrupted = 1, Reconnecting = 2, Resynced = 3, ForfeitPending = 4, Resolved = 5 }
    public enum DisputeState { Submitted = 0, UnderReview = 1, Accepted = 2, Rejected = 3, Corrected = 4 }

    [Serializable]
    public sealed class CompetitionRules
    {
        public string RulesVersion { get; set; } = string.Empty;
        public TournamentFormat Format { get; set; }
        public int MinimumParticipants { get; set; } = 2;
        public int MaximumParticipants { get; set; } = 32;
        public int RosterSize { get; set; } = 11;
        public bool AllowDraw { get; set; }
        public int WinPoints { get; set; } = 3;
        public int DrawPoints { get; set; } = 1;
        public int LossPoints { get; set; }
        public int ReconnectGraceSeconds { get; set; } = 90;
        public List<TieBreakRule> TieBreakOrder { get; set; } = new List<TieBreakRule>
        {
            TieBreakRule.Points, TieBreakRule.GoalDifference, TieBreakRule.GoalsFor, TieBreakRule.ParticipantId
        };
        public string RewardPolicyId { get; set; } = string.Empty;
    }

    [Serializable]
    public sealed class TournamentDefinition
    {
        public string TournamentId { get; set; } = string.Empty;
        public string CatalogVersion { get; set; } = string.Empty;
        public CompetitionRules Rules { get; set; } = new CompetitionRules();
        public List<string> ParticipantIds { get; set; } = new List<string>();
        public bool OnlineRewardsEnabled { get; set; }
    }

    [Serializable]
    public sealed class RosterSubmission
    {
        public string TournamentId { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public string CatalogVersion { get; set; } = string.Empty;
        public string RulesVersion { get; set; } = string.Empty;
        public long ExpectedSnapshotRevision { get; set; }
        public List<string> PlayerIdentityIds { get; set; } = new List<string>();
        public DateTimeOffset SubmittedAt { get; set; }
    }

    [Serializable]
    public sealed class TournamentMatch
    {
        public string MatchId { get; set; } = string.Empty;
        public int Round { get; set; }
        public int Index { get; set; }
        public string HomeParticipantId { get; set; } = string.Empty;
        public string AwayParticipantId { get; set; } = string.Empty;
        public TournamentMatchState State { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string WinnerParticipantId { get; set; } = string.Empty;
        public long ResultSequence { get; set; }
    }

    [Serializable]
    public sealed class StandingEntry
    {
        public string ParticipantId { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        [JsonIgnore] public int GoalDifference => GoalsFor - GoalsAgainst;
    }

    [Serializable]
    public sealed class MatchResultSubmission
    {
        public string TournamentId { get; set; } = string.Empty;
        public string MatchId { get; set; } = string.Empty;
        public string RulesVersion { get; set; } = string.Empty;
        public string CatalogVersion { get; set; } = string.Empty;
        public long ExpectedSnapshotRevision { get; set; }
        public long Sequence { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public string HomeParticipantId { get; set; } = string.Empty;
        public string AwayParticipantId { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string MatchEventDigest { get; set; } = string.Empty;
        public DateTimeOffset SubmittedAt { get; set; }
    }

    [Serializable]
    public sealed class AuthoritativeResultReceipt
    {
        public string ReceiptId { get; set; } = string.Empty;
        public string TournamentId { get; set; } = string.Empty;
        public string MatchId { get; set; } = string.Empty;
        public ResultReceiptStatus Status { get; set; }
        public CompetitionFailureCode FailureCode { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public long Sequence { get; set; }
        public long SnapshotRevision { get; set; }
        public string RewardReferenceId { get; set; } = string.Empty;
        public string RewardReceiptId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    [Serializable]
    public sealed class ReconnectTransition
    {
        public long Sequence { get; set; }
        public ReconnectState From { get; set; }
        public ReconnectState To { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; set; }
    }

    [Serializable]
    public sealed class ReconnectTimeline
    {
        public string MatchId { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public ReconnectState State { get; set; } = ReconnectState.Connected;
        public List<ReconnectTransition> Transitions { get; set; } = new List<ReconnectTransition>();
    }

    [Serializable]
    public sealed class DisputeRecord
    {
        public string DisputeId { get; set; } = string.Empty;
        public string MatchId { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public DisputeState State { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public int? CorrectedHomeScore { get; set; }
        public int? CorrectedAwayScore { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
    }

    [Serializable]
    public sealed class TournamentSnapshot
    {
        public int SchemaVersion { get; set; } = CompetitionCodec.CurrentSchemaVersion;
        public string TournamentId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public TournamentState State { get; set; } = TournamentState.Registration;
        public long LastResultSequence { get; set; }
        public TournamentDefinition Definition { get; set; } = new TournamentDefinition();
        public List<RosterSubmission> Rosters { get; set; } = new List<RosterSubmission>();
        public List<TournamentMatch> Matches { get; set; } = new List<TournamentMatch>();
        public List<StandingEntry> Standings { get; set; } = new List<StandingEntry>();
        public List<AuthoritativeResultReceipt> Receipts { get; set; } = new List<AuthoritativeResultReceipt>();
        public List<ReconnectTimeline> ReconnectTimelines { get; set; } = new List<ReconnectTimeline>();
        public List<DisputeRecord> Disputes { get; set; } = new List<DisputeRecord>();
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; } = new Dictionary<string, JToken>(StringComparer.Ordinal);
    }

    public sealed class CompetitionOperationResult
    {
        public bool Succeeded { get; set; }
        public CompetitionFailureCode FailureCode { get; set; }
        public TournamentSnapshot Snapshot { get; set; }
    }

    public interface ICompetitionRepository
    {
        bool IsReadOnly { get; }
        bool TryLoad(string tournamentId, out TournamentSnapshot snapshot);
        bool TryCommit(string tournamentId, long expectedRevision, TournamentSnapshot next);
    }

    public interface IResultVerifier
    {
        CompetitionFailureCode Verify(TournamentSnapshot snapshot, TournamentMatch match, MatchResultSubmission submission);
    }

    public interface IRewardGrantGateway
    {
        bool TryGrantOnce(string rewardReferenceId, string participantId, string rewardPolicyId, out string rewardReceiptId);
    }

    public interface ICompetitionService
    {
        CompetitionOperationResult Create(TournamentDefinition definition);
        CompetitionOperationResult SubmitRoster(RosterSubmission submission);
        CompetitionOperationResult Start(string tournamentId, long expectedRevision);
        AuthoritativeResultReceipt SubmitResult(MatchResultSubmission submission, DateTimeOffset now);
        CompetitionOperationResult TransitionReconnect(string tournamentId, long expectedRevision, string matchId, string participantId, ReconnectState next, string reasonCode, DateTimeOffset now);
        CompetitionOperationResult AppendDispute(string tournamentId, long expectedRevision, DisputeRecord record);
        CompetitionOperationResult CorrectResult(string tournamentId, long expectedRevision, string disputeId, int homeScore, int awayScore, DateTimeOffset now);
    }
}
