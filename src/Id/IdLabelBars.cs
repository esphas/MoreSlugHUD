using UnityEngine;

namespace MoreSlugHUD;

internal readonly struct IdLabelBar
{
    internal IdLabelBar(bool visible, float width, float height, float x, float y)
    {
        Visible = visible;
        Width = width;
        Height = height;
        X = x;
        Y = y;
    }

    internal bool Visible { get; }
    internal float Width { get; }
    internal float Height { get; }
    internal float X { get; }
    internal float Y { get; }

    internal static IdLabelBar Hidden => new(false, 0f, 0f, 0f, 0f);
}

internal static class IdLabelBars
{
    internal static void Layout(
        bool enabled,
        in CreatureLabelPlacement placement,
        in CreatureLabelMetrics metrics,
        out IdLabelBar number,
        out IdLabelBar name)
    {
        number = IdLabelBar.Hidden;
        name = IdLabelBar.Hidden;
        if (!enabled || (!placement.ShowNumber && !placement.ShowName))
        {
            return;
        }

        if (placement.BothLines)
        {
            if (placement.ShowNumber)
            {
                number = Bar(metrics.NumberWidth, MoreSlugHUDConfig.IdBarHeight, 0f, placement.NumberY);
            }

            if (placement.ShowName)
            {
                name = Bar(metrics.NameWidth, metrics.NameHeight + 6f, 0f, placement.NameY);
            }

            return;
        }

        var height = 0f;
        if (placement.ShowNumber)
        {
            height = MoreSlugHUDConfig.IdBarHeight;
        }

        if (placement.ShowName)
        {
            height = Mathf.Max(height, metrics.NameHeight + 6f);
        }

        var width = placement.Row ? placement.TextWidth : placement.ShowNumber ? metrics.NumberWidth : metrics.NameWidth;
        var x = placement.Row ? -placement.RowWidth * 0.5f + placement.TextWidth * 0.5f : 0f;
        number = Bar(width, height, x, 0f);
    }

    private static IdLabelBar Bar(float contentWidth, float height, float x, float y)
    {
        if (contentWidth <= 0f || height <= 0f)
        {
            return IdLabelBar.Hidden;
        }

        return new IdLabelBar(true, contentWidth + MoreSlugHUDConfig.IdPadX, height, x, y);
    }
}
