namespace MoreSlugHUD;

internal readonly struct IdLabelPresentMode
{
    internal IdLabelPresentMode(bool allowRetry, bool hideForPause)
    {
        AllowRetry = allowRetry;
        HideForPause = hideForPause;
    }

    internal bool AllowRetry { get; }

    internal bool HideForPause { get; }

    internal static IdLabelPresentMode DrawSprites { get; } = new(true, false);

    internal static IdLabelPresentMode PausedDrawSprites(bool gamePaused) => new(false, gamePaused);

    internal void Apply(bool ready, bool canDisplay, ref bool visible)
    {
        if (!ready)
        {
            return;
        }

        if (HideForPause)
        {
            visible = false;
            return;
        }

        if (!AllowRetry)
        {
            return;
        }

        visible = canDisplay;
    }
}
