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
    internal bool IsPickUpCandidate;
    internal int PyroHeat;
    internal int PyroCapacity;
    internal float PyroCooldown;
    internal float PyroFill;
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
    internal SlotContent Pyro;

    internal SlotContent this[SlotId id] => id switch
    {
        SlotId.Craft => Craft,
        SlotId.Left => Left,
        SlotId.Right => Right,
        SlotId.Stomach => Stomach,
        SlotId.Back => Back,
        SlotId.Pyro => Pyro,
        _ => SlotContent.Hidden(id),
    };

    internal int ContentStamp()
    {
        unchecked
        {
            var hash = SlotStamp(Craft);
            hash = hash * 31 + SlotStamp(Left);
            hash = hash * 31 + SlotStamp(Right);
            hash = hash * 31 + SlotStamp(Stomach);
            hash = hash * 31 + SlotStamp(Back);
            return hash * 31 + SlotStamp(Pyro);
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

        if (content.IsPickUpCandidate)
        {
            hash = hash * 31 + 2;
        }

        hash = hash * 31 + content.PyroHeat;

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
