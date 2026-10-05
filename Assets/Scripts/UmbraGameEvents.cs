using System;
using UnityEngine;

public struct UmbraProgressSnapshot
{
    public int LevelNumber;
    public int TotalLevels;
    public bool HasKey;
    public bool LevelCompleted;

    public UmbraProgressSnapshot(int levelNumber, int totalLevels, bool hasKey, bool levelCompleted)
    {
        LevelNumber = levelNumber;
        TotalLevels = totalLevels;
        HasKey = hasKey;
        LevelCompleted = levelCompleted;
    }
}

public struct UmbraPlayerStateSnapshot
{
    public string State;
    public int AttemptNumber;
    public int DeathCount;

    public UmbraPlayerStateSnapshot(string state, int attemptNumber, int deathCount)
    {
        State = state;
        AttemptNumber = attemptNumber;
        DeathCount = deathCount;
    }
}

/// <summary>
/// Observer/Event Aggregator used by gameplay publishers and independent listeners.
/// Publishers do not need references to the HUD, diagnostics or future analytics.
/// </summary>
public static class UmbraGameEvents
{
    public static event Action<string> InteractionPerformed;
    public static event Action<UmbraProgressSnapshot> ProgressChanged;
    public static event Action<UmbraPlayerStateSnapshot> PlayerStateChanged;

    public static int PublishedEventCount { get; private set; }

    public static int ObserverCount =>
        ListenerCount(InteractionPerformed) +
        ListenerCount(ProgressChanged) +
        ListenerCount(PlayerStateChanged);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetChannels()
    {
        InteractionPerformed = null;
        ProgressChanged = null;
        PlayerStateChanged = null;
        PublishedEventCount = 0;
    }

    public static void PublishInteraction(string description)
    {
        PublishedEventCount++;
        InteractionPerformed?.Invoke(description);
    }

    public static void PublishProgress(UmbraProgressSnapshot snapshot)
    {
        PublishedEventCount++;
        ProgressChanged?.Invoke(snapshot);
    }

    public static void PublishPlayerState(UmbraPlayerStateSnapshot snapshot)
    {
        PublishedEventCount++;
        PlayerStateChanged?.Invoke(snapshot);
    }

    private static int ListenerCount(Delegate channel)
    {
        return channel == null ? 0 : channel.GetInvocationList().Length;
    }
}
