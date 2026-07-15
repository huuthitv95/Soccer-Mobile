using SoccerMobilePro.MatchCore;
using UnityEngine;

public sealed class LegacyMatchCoreAdapter
{
    public const string FeaturePrefKey = "smp_match_core_v1";

    private readonly DeterministicMatchSimulation simulation;
    private long nextSequenceId = 1;

    private LegacyMatchCoreAdapter(int seed, bool isFirstHalf)
    {
        simulation = new DeterministicMatchSimulation(seed);
        Apply(MatchCommandType.StartMatch, 0);
        Apply(MatchCommandType.ResumePlay, 0);

        if (!isFirstHalf)
        {
            Apply(MatchCommandType.EndHalf, 0);
            Apply(MatchCommandType.StartSecondHalf, 0);
            Apply(MatchCommandType.ResumePlay, 0);
        }
    }

    public static LegacyMatchCoreAdapter Current { get; private set; }

    public MatchSnapshot Snapshot => simulation.Snapshot;

    public static void BeginSession(bool isFirstHalf, int currentMatch)
    {
        Current = PlayerPrefs.GetInt(FeaturePrefKey, 0) == 1
            ? new LegacyMatchCoreAdapter(currentMatch + 1, isFirstHalf)
            : null;
    }

    public static void EndSession()
    {
        Current = null;
    }

    public static void RecordFoul(int gameTick)
    {
        Current?.RecordIncident(MatchCommandType.RecordFoul, gameTick);
    }

    public static void RecordCorner(int gameTick)
    {
        Current?.RecordIncident(MatchCommandType.RecordCorner, gameTick);
    }

    public void AdvanceClock(int gameTick)
    {
        int remaining = gameTick - Snapshot.Tick;
        while (remaining > 0 && Snapshot.Phase == MatchPhase.InPlay)
        {
            int delta = Mathf.Min(remaining, 600);
            Apply(MatchCommandType.AdvanceTick, Snapshot.Tick, -1, delta);
            remaining -= delta;
        }
    }

    public void CompleteCurrentHalf(int gameTick)
    {
        AdvanceClock(gameTick);
        if (Snapshot.Phase != MatchPhase.InPlay)
        {
            return;
        }

        Apply(Snapshot.Half == 1 ? MatchCommandType.EndHalf : MatchCommandType.EndMatch, gameTick);
    }

    private void RecordIncident(MatchCommandType type, int gameTick)
    {
        AdvanceClock(gameTick);
        if (Snapshot.Phase != MatchPhase.InPlay)
        {
            return;
        }

        Apply(type, gameTick);
        Apply(MatchCommandType.ResumePlay, gameTick);
    }

    private MatchTransitionResult Apply(
        MatchCommandType type,
        int clientTick,
        int teamIndex = -1,
        int tickDelta = 0)
    {
        MatchTransitionResult result = simulation.Apply(new MatchCommand(
            nextSequenceId,
            clientTick,
            "legacy-shadow",
            type,
            teamIndex,
            tickDelta));

        if (result.Accepted)
        {
            nextSequenceId++;
        }

        return result;
    }
}
