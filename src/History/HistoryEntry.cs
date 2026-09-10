namespace MoreSlugHUD;

internal sealed class HistoryEntry
{
    internal const byte MaxDuration = 99;

    internal InputState State { get; }
    internal byte Duration { get; private set; } = 1;

    internal HistoryEntry(InputState state)
    {
        State = state;
    }

    internal void Tick()
    {
        if (Duration < MaxDuration)
        {
            Duration++;
        }
    }
}

