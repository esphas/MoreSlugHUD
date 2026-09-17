using System.Collections.Generic;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class SettingsPageBuilder
{
    internal const float StartY = 540f;
    internal const float LabelX = 40f;
    internal const float DefaultControlX = 220f;
    internal const float DefaultCol2 = 300f;
    internal const float Row = 36f;
    internal const float CheckRow = 36f;

    internal SettingsPageBuilder(
        float controlX = DefaultControlX,
        float col2 = DefaultCol2,
        float startY = StartY,
        float control2 = 430f)
    {
        ControlX = controlX;
        Col2 = col2;
        Control2 = control2;
        Y = startY;
    }

    internal float ControlX { get; }
    internal float Col2 { get; }
    internal float Control2 { get; }
    internal float Y { get; private set; }
    internal List<UIelement> Items { get; } = new();

    internal float Advance(float dy)
    {
        Y -= dy;
        return Y;
    }

    internal void Add(UIelement element) => Items.Add(element);

    internal void Add(UIelement first, UIelement second)
    {
        Items.Add(first);
        Items.Add(second);
    }

    internal OpLabel Section(string text)
    {
        var label = new OpLabel(LabelX, Y, text)
        {
            color = MenuColorEffect.rgbWhite,
        };
        Items.Add(label);
        return label;
    }

    internal OpLabel Wrapped(float width, string text)
    {
        var label = new OpLabel(new Vector2(LabelX, Y), new Vector2(width, 30f), text, FLabelAlignment.Left)
        {
            autoWrap = true,
        };
        Items.Add(label);
        return label;
    }
}
