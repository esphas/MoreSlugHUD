using UnityEngine;

namespace MoreSlugHUD;

internal readonly struct SlotPlacement
{
    internal SlotPlacement(SlotId id, Vector2 local)
    {
        Id = id;
        Local = local;
    }

    internal SlotId Id { get; }
    internal Vector2 Local { get; }
}

internal readonly struct LayoutResult
{
    internal LayoutResult(Vector2 origin, SlotPlacement[] slots, int count, Vector2 size)
    {
        Origin = origin;
        Slots = slots;
        Count = count;
        Size = size;
    }

    internal Vector2 Origin { get; }
    internal SlotPlacement[] Slots { get; }
    internal int Count { get; }
    internal Vector2 Size { get; }
}

internal static class LayoutEngine
{
    private static readonly SlotPlacement[] Buffer = new SlotPlacement[5];
    private static readonly float[] Widths = new float[5];

    internal static LayoutResult Place(HUD.HUD hud, SlotId[] occupying, int count)
    {
        var filled = MoreSlugHUDConfig.Layout == LayoutStyle.Cross
            ? CrossLocals(occupying, count)
            : RowLocals(occupying, count);
        var size = Measure(filled);
        var origin = Clamp(hud, IndependentOrigin(hud, size), size);
        return new LayoutResult(origin, Buffer, filled, size);
    }

    private static int RowLocals(SlotId[] occupying, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        var total = 0f;
        for (var i = 0; i < count; i++)
        {
            Widths[i] = MoreSlugHUDConfig.SlotSize;
            if (i > 0)
            {
                total += MoreSlugHUDConfig.SlotGap;
            }

            total += Widths[i];
        }

        var x = -total * 0.5f;
        for (var i = 0; i < count; i++)
        {
            x += Widths[i] * 0.5f;
            Buffer[i] = new SlotPlacement(occupying[i], new Vector2(x, 0f));
            x += Widths[i] * 0.5f + MoreSlugHUDConfig.SlotGap;
        }

        return count;
    }

    private static int CrossLocals(SlotId[] occupying, int count)
    {
        var mask = 0;
        for (var i = 0; i < count; i++)
        {
            mask |= 1 << (int)occupying[i];
        }

        var filled = 0;
        var gap = MoreSlugHUDConfig.SlotGap;
        var hasLeft = (mask & (1 << (int)SlotId.Left)) != 0;
        var hasRight = (mask & (1 << (int)SlotId.Right)) != 0;
        var hasBack = (mask & (1 << (int)SlotId.Back)) != 0;
        var hasStomach = (mask & (1 << (int)SlotId.Stomach)) != 0;
        var hasCraft = (mask & (1 << (int)SlotId.Craft)) != 0;
        var leftX = 0f;
        var rightX = 0f;
        if (hasLeft && hasRight)
        {
            leftX = -gap * 0.5f;
            rightX = gap * 0.5f;
        }

        if (hasLeft)
        {
            Buffer[filled++] = new SlotPlacement(SlotId.Left, new Vector2(leftX, 0f));
        }

        if (hasRight)
        {
            Buffer[filled++] = new SlotPlacement(SlotId.Right, new Vector2(rightX, 0f));
        }

        var coreLeft = hasLeft ? leftX : hasRight ? rightX : 0f;
        if (hasBack)
        {
            var y = hasLeft || hasRight || hasStomach ? gap : 0f;
            Buffer[filled++] = new SlotPlacement(SlotId.Back, new Vector2(0f, y));
        }

        if (hasStomach)
        {
            var y = hasLeft || hasRight || hasBack ? -gap : 0f;
            Buffer[filled++] = new SlotPlacement(SlotId.Stomach, new Vector2(0f, y));
        }

        if (hasCraft)
        {
            var craftX = coreLeft - gap;
            if (!hasLeft && !hasRight)
            {
                craftX = -gap;
            }

            Buffer[filled++] = new SlotPlacement(SlotId.Craft, new Vector2(craftX, 0f));
        }

        return filled;
    }

    private static Vector2 Measure(int count)
    {
        if (count == 0)
        {
            return Vector2.zero;
        }

        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        for (var i = 0; i < count; i++)
        {
            var half = MoreSlugHUDConfig.SlotSize * 0.5f;
            var p = Buffer[i].Local;
            min = Vector2.Min(min, p - new Vector2(half, half));
            max = Vector2.Max(max, p + new Vector2(half, half));
        }

        return max - min;
    }

    private static Vector2 IndependentOrigin(HUD.HUD hud, Vector2 size)
    {
        var screen = hud.rainWorld.options.ScreenSize;
        var safe = hud.rainWorld.options.SafeScreenOffset;
        var min = safe;
        var max = screen - safe;
        var pad = MoreSlugHUDConfig.ScreenPadding;
        var midX = (min.x + max.x) * 0.5f;
        return MoreSlugHUDConfig.Position switch
        {
            PositionPreset.TopCenter => new Vector2(midX, max.y - pad - size.y * 0.5f),
            PositionPreset.TopLeft => new Vector2(min.x + pad + size.x * 0.5f, max.y - pad - size.y * 0.5f),
            PositionPreset.TopRight => new Vector2(max.x - pad - size.x * 0.5f, max.y - pad - size.y * 0.5f),
            PositionPreset.BottomRight => new Vector2(max.x - pad - size.x * 0.5f, min.y + pad + size.y * 0.5f),
            PositionPreset.Custom => new Vector2(
                Mathf.Lerp(min.x, max.x, MoreSlugHUDConfig.CustomX / 100f),
                Mathf.Lerp(min.y, max.y, MoreSlugHUDConfig.CustomY / 100f)),
            _ => BottomCenterOrigin(hud, size),
        };
    }

    private static Vector2 BottomCenterOrigin(HUD.HUD hud, Vector2 size)
    {
        var screen = hud.rainWorld.options.ScreenSize;
        var safe = hud.rainWorld.options.SafeScreenOffset;
        var pad = MoreSlugHUDConfig.ScreenPadding;
        var midX = (safe.x + screen.x - safe.x) * 0.5f;
        return new Vector2(midX, BottomCenterFloor(hud, safe.y + pad) + size.y * 0.5f);
    }

    private static float BottomCenterFloor(HUD.HUD hud, float safeFloor)
    {
        var bandTop = DownpourCompat.PulseBandTop(hud);
        if (bandTop <= 0f)
        {
            return safeFloor;
        }

        return Mathf.Max(safeFloor, bandTop + MoreSlugHUDConfig.ScreenPadding);
    }

    private static Vector2 Clamp(HUD.HUD hud, Vector2 origin, Vector2 size)
    {
        var screen = hud.rainWorld.options.ScreenSize;
        var safe = hud.rainWorld.options.SafeScreenOffset;
        var half = size * 0.5f;
        var min = safe + half;
        var max = screen - safe - half;
        return new Vector2(
            Mathf.Clamp(origin.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x)),
            Mathf.Clamp(origin.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y)));
    }
}
