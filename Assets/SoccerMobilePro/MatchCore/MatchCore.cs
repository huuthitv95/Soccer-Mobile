using System;
using System.Collections.Generic;

namespace SoccerMobilePro.MatchCore
{
    public enum MatchPhase
    {
        PreMatch = 0,
        Kickoff = 1,
        InPlay = 2,
        Foul = 3,
        Corner = 4,
        HalfTime = 5,
        FullTime = 6
    }

    public enum MatchCommandType
    {
        StartMatch = 0,
        ResumePlay = 1,
        AdvanceTick = 2,
        RecordFoul = 3,
        RecordCorner = 4,
        AwardGoal = 5,
        EndHalf = 6,
        StartSecondHalf = 7,
        EndMatch = 8,
        PlayerMove = 20,
        Sprint = 21,
        Pass = 22,
        ThroughPass = 23,
        Shoot = 24,
        Skill = 25,
        SwitchPlayer = 26,
        Press = 27,
        Tackle = 28,
        SlideTackle = 29,
        MatchUp = 30,
        GoalkeeperRush = 31,
        GoalkeeperDive = 32,
        GoalkeeperCatch = 33,
        Distribute = 34,
        SetPieceAim = 35,
        SetPiecePower = 36,
        SetPieceCurl = 37,
        TriggerRunner = 38,
        Confirm = 39,
        Cancel = 40,
        Pause = 41
    }

    public enum MatchEventType
    {
        PhaseChanged = 0,
        TickAdvanced = 1,
        GoalAwarded = 2,
        CommandRejected = 3
    }

    public readonly struct MatchCommand
    {
        public MatchCommand(
            long sequenceId,
            int clientTick,
            string actorId,
            MatchCommandType type,
            int teamIndex = -1,
            int tickDelta = 0,
            float directionX = 0f,
            float directionY = 0f,
            float magnitude = 0f,
            string modifiers = "")
        {
            SequenceId = sequenceId;
            ClientTick = clientTick;
            ActorId = actorId ?? string.Empty;
            Type = type;
            TeamIndex = teamIndex;
            TickDelta = tickDelta;
            DirectionX = directionX;
            DirectionY = directionY;
            Magnitude = magnitude;
            Modifiers = modifiers ?? string.Empty;
        }

        public long SequenceId { get; }
        public int ClientTick { get; }
        public string ActorId { get; }
        public MatchCommandType Type { get; }
        public int TeamIndex { get; }
        public int TickDelta { get; }
        public float DirectionX { get; }
        public float DirectionY { get; }
        public float Magnitude { get; }
        public string Modifiers { get; }
    }

    public readonly struct MatchEvent
    {
        public MatchEvent(
            long sequenceId,
            MatchEventType type,
            MatchPhase fromPhase,
            MatchPhase toPhase,
            int tick,
            string reasonCode)
        {
            SequenceId = sequenceId;
            Type = type;
            FromPhase = fromPhase;
            ToPhase = toPhase;
            Tick = tick;
            ReasonCode = reasonCode ?? string.Empty;
        }

        public long SequenceId { get; }
        public MatchEventType Type { get; }
        public MatchPhase FromPhase { get; }
        public MatchPhase ToPhase { get; }
        public int Tick { get; }
        public string ReasonCode { get; }
    }

    public sealed class MatchSnapshot
    {
        internal MatchSnapshot(
            int seed,
            MatchPhase phase,
            int half,
            int tick,
            int homeScore,
            int awayScore,
            long lastSequenceId)
        {
            Seed = seed;
            Phase = phase;
            Half = half;
            Tick = tick;
            HomeScore = homeScore;
            AwayScore = awayScore;
            LastSequenceId = lastSequenceId;
            StateHash = DeterministicStateHash.Compute(this);
        }

        public int Seed { get; }
        public MatchPhase Phase { get; }
        public int Half { get; }
        public int Tick { get; }
        public int HomeScore { get; }
        public int AwayScore { get; }
        public long LastSequenceId { get; }
        public ulong StateHash { get; }
    }

    public readonly struct RuleDecision
    {
        public RuleDecision(bool accepted, MatchPhase nextPhase, string reasonCode)
        {
            Accepted = accepted;
            NextPhase = nextPhase;
            ReasonCode = reasonCode ?? string.Empty;
        }

        public bool Accepted { get; }
        public MatchPhase NextPhase { get; }
        public string ReasonCode { get; }
    }

    public sealed class MatchTransitionResult
    {
        public MatchTransitionResult(
            bool accepted,
            string reasonCode,
            MatchSnapshot snapshot,
            IReadOnlyList<MatchEvent> newEvents)
        {
            Accepted = accepted;
            ReasonCode = reasonCode ?? string.Empty;
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            NewEvents = newEvents ?? throw new ArgumentNullException(nameof(newEvents));
        }

        public bool Accepted { get; }
        public string ReasonCode { get; }
        public MatchSnapshot Snapshot { get; }
        public IReadOnlyList<MatchEvent> NewEvents { get; }
    }

    public interface IRuleEngine
    {
        RuleDecision Evaluate(MatchSnapshot snapshot, MatchCommand command);
    }

    public interface IMatchSimulation
    {
        MatchSnapshot Snapshot { get; }
        IReadOnlyList<MatchEvent> Events { get; }
        MatchTransitionResult Apply(MatchCommand command);
    }

    public sealed class DefaultRuleEngine : IRuleEngine
    {
        public const string Accepted = "accepted";
        public const string InvalidSequence = "invalid_sequence";
        public const string InvalidTick = "invalid_tick";
        public const string InvalidTickDelta = "invalid_tick_delta";
        public const string InvalidTeam = "invalid_team";
        public const string InvalidTransition = "invalid_transition";

        public RuleDecision Evaluate(MatchSnapshot snapshot, MatchCommand command)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (command.SequenceId != snapshot.LastSequenceId + 1)
            {
                return Reject(snapshot, InvalidSequence);
            }

            if (command.ClientTick < snapshot.Tick)
            {
                return Reject(snapshot, InvalidTick);
            }

            if (command.Type == MatchCommandType.AdvanceTick &&
                (command.TickDelta < 1 || command.TickDelta > 600))
            {
                return Reject(snapshot, InvalidTickDelta);
            }

            if (command.Type == MatchCommandType.AwardGoal &&
                command.TeamIndex != 0 && command.TeamIndex != 1)
            {
                return Reject(snapshot, InvalidTeam);
            }

            switch (snapshot.Phase)
            {
                case MatchPhase.PreMatch:
                    return command.Type == MatchCommandType.StartMatch
                        ? Accept(MatchPhase.Kickoff)
                        : Reject(snapshot, InvalidTransition);
                case MatchPhase.Kickoff:
                    return command.Type == MatchCommandType.ResumePlay
                        ? Accept(MatchPhase.InPlay)
                        : Reject(snapshot, InvalidTransition);
                case MatchPhase.InPlay:
                    return EvaluateInPlay(snapshot, command);
                case MatchPhase.Foul:
                case MatchPhase.Corner:
                    return command.Type == MatchCommandType.ResumePlay
                        ? Accept(MatchPhase.InPlay)
                        : Reject(snapshot, InvalidTransition);
                case MatchPhase.HalfTime:
                    return command.Type == MatchCommandType.StartSecondHalf && snapshot.Half == 1
                        ? Accept(MatchPhase.Kickoff)
                        : Reject(snapshot, InvalidTransition);
                case MatchPhase.FullTime:
                default:
                    return Reject(snapshot, InvalidTransition);
            }
        }

        private static RuleDecision EvaluateInPlay(MatchSnapshot snapshot, MatchCommand command)
        {
            switch (command.Type)
            {
                case MatchCommandType.AdvanceTick:
                case MatchCommandType.AwardGoal:
                    return Accept(MatchPhase.InPlay);
                case MatchCommandType.RecordFoul:
                    return Accept(MatchPhase.Foul);
                case MatchCommandType.RecordCorner:
                    return Accept(MatchPhase.Corner);
                case MatchCommandType.EndHalf when snapshot.Half == 1:
                    return Accept(MatchPhase.HalfTime);
                case MatchCommandType.EndMatch when snapshot.Half == 2:
                    return Accept(MatchPhase.FullTime);
                default:
                    return Reject(snapshot, InvalidTransition);
            }
        }

        private static RuleDecision Accept(MatchPhase phase)
        {
            return new RuleDecision(true, phase, Accepted);
        }

        private static RuleDecision Reject(MatchSnapshot snapshot, string reason)
        {
            return new RuleDecision(false, snapshot.Phase, reason);
        }
    }

    public sealed class DeterministicMatchSimulation : IMatchSimulation
    {
        private readonly IRuleEngine ruleEngine;
        private readonly List<MatchEvent> events;
        private readonly int maxEvents;

        public DeterministicMatchSimulation(int seed, int maxEvents = 256, IRuleEngine ruleEngine = null)
        {
            if (maxEvents < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxEvents));
            }

            this.maxEvents = maxEvents;
            this.ruleEngine = ruleEngine ?? new DefaultRuleEngine();
            events = new List<MatchEvent>(Math.Min(maxEvents, 256));
            Snapshot = new MatchSnapshot(seed, MatchPhase.PreMatch, 1, 0, 0, 0, 0);
        }

        public MatchSnapshot Snapshot { get; private set; }
        public IReadOnlyList<MatchEvent> Events => events;

        public MatchTransitionResult Apply(MatchCommand command)
        {
            RuleDecision decision = ruleEngine.Evaluate(Snapshot, command);
            if (!decision.Accepted)
            {
                MatchEvent rejected = new MatchEvent(
                    command.SequenceId,
                    MatchEventType.CommandRejected,
                    Snapshot.Phase,
                    Snapshot.Phase,
                    Snapshot.Tick,
                    decision.ReasonCode);
                AppendEvent(rejected);
                return new MatchTransitionResult(
                    false,
                    decision.ReasonCode,
                    Snapshot,
                    new[] { rejected });
            }

            MatchSnapshot previous = Snapshot;
            int tick = command.Type == MatchCommandType.AdvanceTick
                ? previous.Tick + command.TickDelta
                : Math.Max(previous.Tick, command.ClientTick);
            int half = command.Type == MatchCommandType.StartSecondHalf ? 2 : previous.Half;
            int homeScore = previous.HomeScore;
            int awayScore = previous.AwayScore;

            if (command.Type == MatchCommandType.AwardGoal)
            {
                if (command.TeamIndex == 0)
                {
                    homeScore++;
                }
                else
                {
                    awayScore++;
                }
            }

            Snapshot = new MatchSnapshot(
                previous.Seed,
                decision.NextPhase,
                half,
                tick,
                homeScore,
                awayScore,
                command.SequenceId);

            var newEvents = new List<MatchEvent>(2);
            if (previous.Phase != Snapshot.Phase)
            {
                newEvents.Add(new MatchEvent(
                    command.SequenceId,
                    MatchEventType.PhaseChanged,
                    previous.Phase,
                    Snapshot.Phase,
                    Snapshot.Tick,
                    DefaultRuleEngine.Accepted));
            }

            if (command.Type == MatchCommandType.AdvanceTick)
            {
                newEvents.Add(new MatchEvent(
                    command.SequenceId,
                    MatchEventType.TickAdvanced,
                    previous.Phase,
                    Snapshot.Phase,
                    Snapshot.Tick,
                    DefaultRuleEngine.Accepted));
            }
            else if (command.Type == MatchCommandType.AwardGoal)
            {
                newEvents.Add(new MatchEvent(
                    command.SequenceId,
                    MatchEventType.GoalAwarded,
                    previous.Phase,
                    Snapshot.Phase,
                    Snapshot.Tick,
                    command.TeamIndex == 0 ? "home" : "away"));
            }

            foreach (MatchEvent matchEvent in newEvents)
            {
                AppendEvent(matchEvent);
            }

            return new MatchTransitionResult(true, decision.ReasonCode, Snapshot, newEvents);
        }

        private void AppendEvent(MatchEvent matchEvent)
        {
            if (events.Count == maxEvents)
            {
                events.RemoveAt(0);
            }

            events.Add(matchEvent);
        }
    }

    internal static class DeterministicStateHash
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Compute(MatchSnapshot snapshot)
        {
            ulong hash = Offset;
            hash = Append(hash, snapshot.Seed);
            hash = Append(hash, (int)snapshot.Phase);
            hash = Append(hash, snapshot.Half);
            hash = Append(hash, snapshot.Tick);
            hash = Append(hash, snapshot.HomeScore);
            hash = Append(hash, snapshot.AwayScore);
            hash = Append(hash, snapshot.LastSequenceId);
            return hash;
        }

        private static ulong Append(ulong hash, int value)
        {
            return Append(hash, (long)value);
        }

        private static ulong Append(ulong hash, long value)
        {
            unchecked
            {
                ulong data = (ulong)value;
                for (int index = 0; index < 8; index++)
                {
                    hash ^= (byte)(data & 0xff);
                    hash *= Prime;
                    data >>= 8;
                }

                return hash;
            }
        }
    }
}
