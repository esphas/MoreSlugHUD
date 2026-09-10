using UnityEngine;

namespace MoreSlugHUD;

internal readonly struct IconDraw
{
    internal IconDraw(string spriteName, Color color, float scale = 1f)
    {
        SpriteName = spriteName;
        Color = color;
        Scale = scale;
    }

    internal string SpriteName { get; }
    internal Color Color { get; }
    internal float Scale { get; }
}

internal struct SlotContent
{
    internal SlotId Id;
    internal bool OccupiesLayout;
    internal bool ShowPlaceholder;
    internal float ContentAlpha;
    internal int StackCount;
    internal IconDraw Icon0;
    internal IconDraw Icon1;
    internal IconDraw Icon2;

    internal static SlotContent Hidden(SlotId id) => new()
    {
        Id = id,
        OccupiesLayout = false,
        ShowPlaceholder = false,
        ContentAlpha = 0f,
    };

    internal bool HasContent => StackCount > 0;

    internal IconDraw IconAt(int index) => index switch
    {
        0 => Icon0,
        1 => Icon1,
        _ => Icon2,
    };
}

internal sealed class InventorySnapshot
{
    internal SlotContent Craft;
    internal SlotContent Left;
    internal SlotContent Right;
    internal SlotContent Stomach;
    internal SlotContent Back;

    internal SlotContent this[SlotId id] => id switch
    {
        SlotId.Craft => Craft,
        SlotId.Left => Left,
        SlotId.Right => Right,
        SlotId.Stomach => Stomach,
        SlotId.Back => Back,
        _ => SlotContent.Hidden(id),
    };

    internal int CopyOccupying(SlotId[] dest)
    {
        var count = 0;
        if (Craft.OccupiesLayout)
        {
            dest[count++] = SlotId.Craft;
        }

        if (Left.OccupiesLayout)
        {
            dest[count++] = SlotId.Left;
        }

        if (Right.OccupiesLayout)
        {
            dest[count++] = SlotId.Right;
        }

        if (Stomach.OccupiesLayout)
        {
            dest[count++] = SlotId.Stomach;
        }

        if (Back.OccupiesLayout)
        {
            dest[count++] = SlotId.Back;
        }

        return count;
    }

    internal int ContentStamp()
    {
        unchecked
        {
            var hash = SlotStamp(Craft);
            hash = hash * 31 + SlotStamp(Left);
            hash = hash * 31 + SlotStamp(Right);
            hash = hash * 31 + SlotStamp(Stomach);
            return hash * 31 + SlotStamp(Back);
        }
    }

    private static int SlotStamp(SlotContent content)
    {
        if (!content.OccupiesLayout)
        {
            return 0;
        }

        var hash = content.StackCount;
        if (content.ShowPlaceholder)
        {
            hash = hash * 31 + 1;
        }

        if (content.StackCount > 0)
        {
            hash = hash * 31 + (content.Icon0.SpriteName?.GetHashCode() ?? 0);
        }

        if (content.StackCount > 1)
        {
            hash = hash * 31 + (content.Icon1.SpriteName?.GetHashCode() ?? 0);
        }

        if (content.StackCount > 2)
        {
            hash = hash * 31 + (content.Icon2.SpriteName?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
