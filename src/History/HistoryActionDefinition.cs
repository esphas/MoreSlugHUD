namespace MoreSlugHUD;

internal readonly struct HistoryActionDefinition
{
    internal HistoryActionDefinition(string element, string letter, string locKey)
    {
        Element = element;
        Letter = letter;
        LocKey = locKey;
    }

    internal string Element { get; }
    internal string Letter { get; }
    internal string LocKey { get; }
}

internal static class HistoryActionCatalog
{
    internal static readonly HistoryActionDefinition[] DisplayOrder =
    {
        new("ih_jump", "J", LocKeys.HistoryActions[0]),
        new("ih_throw", "T", LocKeys.HistoryActions[1]),
        new("ih_pickup", "G", LocKeys.HistoryActions[2]),
        new("ih_special", "S", LocKeys.HistoryActions[3]),
    };

    internal static readonly string[] Elements;
    internal static readonly string[] Letters;
    internal static readonly string[] HintKeys;

    internal static int Count => DisplayOrder.Length;

    static HistoryActionCatalog()
    {
        Elements = new string[DisplayOrder.Length];
        Letters = new string[DisplayOrder.Length];
        HintKeys = new string[DisplayOrder.Length];
        for (var i = 0; i < DisplayOrder.Length; i++)
        {
            Elements[i] = DisplayOrder[i].Element;
            Letters[i] = DisplayOrder[i].Letter;
            HintKeys[i] = DisplayOrder[i].LocKey;
        }
    }

    internal static bool IsPressed(in InputState state, int index) => index switch
    {
        0 => state.Jump,
        1 => state.Throw,
        2 => state.Pickup,
        _ => state.Special,
    };
}
