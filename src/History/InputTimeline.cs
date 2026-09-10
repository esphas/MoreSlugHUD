using System.Collections.Generic;

namespace MoreSlugHUD;

internal sealed class InputTimeline
{
    private readonly List<HistoryEntry> _entries = new();

    internal IReadOnlyList<HistoryEntry> Entries => _entries;

    internal bool Observe(InputState state, int capacity)
    {
        capacity = capacity < 1 ? 1 : capacity;
        if (_entries.Count > 0 && _entries[0].State.Equals(state))
        {
            _entries[0].Tick();
            Trim(capacity);
            return false;
        }

        _entries.Insert(0, new HistoryEntry(state));
        Trim(capacity);
        return true;
    }

    internal void Clear() => _entries.Clear();

    private void Trim(int capacity)
    {
        if (_entries.Count > capacity)
        {
            _entries.RemoveRange(capacity, _entries.Count - capacity);
        }
    }
}
