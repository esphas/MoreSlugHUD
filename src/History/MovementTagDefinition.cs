namespace MoreSlugHUD;

internal readonly struct MovementTagDefinition
{
    internal MovementTagDefinition(
        MovementTags tag,
        string element,
        string letter,
        string locKey,
        string debugName)
    {
        Tag = tag;
        Element = element;
        Letter = letter;
        LocKey = locKey;
        DebugName = debugName;
    }

    internal MovementTags Tag { get; }
    internal string Element { get; }
    internal string Letter { get; }
    internal string LocKey { get; }
    internal string DebugName { get; }
}

internal static class MovementTagCatalog
{
    internal static readonly MovementTagDefinition[] DisplayOrder =
    {
        new(MovementTags.Incapacitated, "ih_incap", "XX", LocKeys.HistoryStates[0], "incap"),
        new(MovementTags.Grabbed, "ih_grabbed", "GX", LocKeys.HistoryStates[1], "grab"),
        new(MovementTags.Shortcut, "ih_shortcut", "TR", LocKeys.HistoryStates[2], "pipe"),
        new(MovementTags.Corridor, "ih_corridor", "TN", LocKeys.HistoryStates[3], "corr"),
        new(MovementTags.ZeroG, "ih_zerog", "ZR", LocKeys.HistoryStates[4], "zg"),
        new(MovementTags.ZeroGPole, "ih_zerog_pole", "ZP", LocKeys.HistoryStates[5], "zgpole"),
        new(MovementTags.SurfaceSwim, "ih_swim_surface", "FS", LocKeys.HistoryStates[6], "surf"),
        new(MovementTags.DeepSwim, "ih_swim_deep", "DS", LocKeys.HistoryStates[7], "deep"),
        new(MovementTags.WallClimb, "ih_wall", "WL", LocKeys.HistoryStates[8], "wall"),
        new(MovementTags.Pole, "ih_pole", "PO", LocKeys.HistoryStates[9], "pole"),
        new(MovementTags.Stand, "ih_stand", "ST", LocKeys.HistoryStates[10], "stand"),
        new(MovementTags.Crawl, "ih_crawl", "CR", LocKeys.HistoryStates[11], "crawl"),
        new(MovementTags.Air, "ih_air", "AR", LocKeys.HistoryStates[12], "air"),
    };

    internal static readonly string[] Elements;
    internal static readonly string[] Letters;
    internal static readonly string[] HintKeys;

    static MovementTagCatalog()
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
}
