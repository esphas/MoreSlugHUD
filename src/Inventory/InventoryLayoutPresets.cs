using UnityEngine;

namespace MoreSlugHUD;

internal static class InventoryLayoutPresets
{
    internal const float LocalMin = -800f;
    internal const float LocalMax = 800f;
    internal const float NativeBandPx = 80f;
    internal const float FallbackScreenX = 1366f;
    internal const float FallbackScreenY = 768f;

    internal static Vector2 Row(SlotId id)
    {
        var pitch = MoreSlugHUDConfig.SlotSize + MoreSlugHUDConfig.SlotGap;
        return id switch
        {
            SlotId.Craft => new Vector2(-2f * pitch, 0f),
            SlotId.Left => new Vector2(-pitch, 0f),
            SlotId.Right => Vector2.zero,
            SlotId.Stomach => new Vector2(pitch, 0f),
            SlotId.Back => new Vector2(2f * pitch, 0f),
            SlotId.Pyro => new Vector2(0f, -pitch),
            _ => Vector2.zero,
        };
    }

    internal static Vector2 Cross(SlotId id)
    {
        var gap = MoreSlugHUDConfig.SlotGap;
        return id switch
        {
            SlotId.Craft => new Vector2(-1.5f * gap, 0f),
            SlotId.Left => new Vector2(-0.5f * gap, 0f),
            SlotId.Right => new Vector2(0.5f * gap, 0f),
            SlotId.Stomach => new Vector2(0f, -gap),
            SlotId.Back => new Vector2(0f, gap),
            SlotId.Pyro => new Vector2(gap, -gap),
            _ => Vector2.zero,
        };
    }

    internal static Vector2 Local(SlotId id, LayoutStyle style) => style switch
    {
        LayoutStyle.Cross => Cross(id),
        _ => Row(id),
    };

    internal static Color SlotColor(SlotId id, bool enabled, bool selected)
    {
        Color baseColor = id switch
        {
            SlotId.Craft => new Color(0.75f, 0.62f, 0.2f),
            SlotId.Left => new Color(0.35f, 0.62f, 0.85f),
            SlotId.Right => new Color(0.35f, 0.55f, 0.9f),
            SlotId.Stomach => new Color(0.55f, 0.75f, 0.4f),
            SlotId.Pyro => MoreSlugHUDConfig.PyroNormal,
            _ => new Color(0.7f, 0.45f, 0.75f),
        };
        if (!enabled)
        {
            baseColor *= 0.5f;
        }

        return selected ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor;
    }

    internal static Vector2 OriginPercent(PositionPreset position, float customX, float customY) => position switch
    {
        PositionPreset.TopCenter => new Vector2(50f, 90f),
        PositionPreset.TopLeft => new Vector2(14f, 90f),
        PositionPreset.TopRight => new Vector2(86f, 90f),
        PositionPreset.BottomRight => new Vector2(86f, 30f),
        PositionPreset.Custom => new Vector2(customX, customY),
        _ => new Vector2(50f, 30f),
    };

    internal static Vector2 Origin(Vector2 screen, Vector2 safe, float originXPercent, float originYPercent)
    {
        var min = safe;
        var max = screen - safe;
        return new Vector2(
            Mathf.Lerp(min.x, max.x, originXPercent / 100f),
            Mathf.Lerp(min.y, max.y, originYPercent / 100f));
    }

    internal static void ResolveScreen(out Vector2 screen, out Vector2 safe)
    {
        screen = new Vector2(FallbackScreenX, FallbackScreenY);
        safe = Vector2.zero;
        var rainWorld = CustomRainWorld();
        if (rainWorld?.options == null)
        {
            return;
        }

        screen = rainWorld.options.ScreenSize;
        safe = rainWorld.options.SafeScreenOffset;
    }

    internal static string Warning(
        float originXPercent,
        float originYPercent,
        System.Func<SlotId, Vector2> localOf,
        System.Func<SlotId, bool> enabled,
        System.Func<string, string> translate)
    {
        ResolveScreen(out var screen, out var safe);
        var origin = Origin(screen, safe, originXPercent, originYPercent);
        var offScreen = false;
        var overlap = false;
        var nativeTop = NativeBandPx + MoreSlugHUDConfig.BottomPulseExtent;
        var safeMax = screen - safe;
        for (var i = 0; i < InventorySlots.All.Length; i++)
        {
            var id = InventorySlots.All[i];
            if (!enabled(id))
            {
                continue;
            }

            var center = origin + localOf(id);
            var half = SlotHalf(id);
            var min = center - half;
            var max = center + half;
            if (min.x < 0f || min.y < 0f || max.x > screen.x || max.y > screen.y
                || min.x < safe.x || min.y < safe.y || max.x > safeMax.x || max.y > safeMax.y)
            {
                offScreen = true;
            }

            if (min.y < nativeTop)
            {
                overlap = true;
            }
        }

        if (!offScreen && !overlap)
        {
            return string.Empty;
        }

        if (offScreen && overlap)
        {
            return translate(LocKeys.OptionsLayoutOffScreen) + " " + translate(LocKeys.OptionsLayoutOverlapHud);
        }

        return offScreen
            ? translate(LocKeys.OptionsLayoutOffScreen)
            : translate(LocKeys.OptionsLayoutOverlapHud);
    }

    internal static Vector2 SlotHalf(SlotId id)
    {
        var half = MoreSlugHUDConfig.SlotSize * 0.5f;
        return id == SlotId.Pyro
            ? new Vector2(half, half + 14f)
            : new Vector2(half, half);
    }

    private static RainWorld? CustomRainWorld()
    {
        try
        {
            return RWCustom.Custom.rainWorld;
        }
        catch
        {
            return null;
        }
    }
}
