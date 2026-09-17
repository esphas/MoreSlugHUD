using UnityEngine;

namespace MoreSlugHUD;

internal static class IntentStyle
{
    internal const string NeutralElement = "SurvivorA";
    internal const string HostileElement = "miscDangerSymbol";
    internal const string FriendlyElement = "FriendA";

    internal static readonly Color NeutralColor = new(1f, 0.78f, 0.16f);
    internal static readonly Color HostileColor = new(1f, 0.12f, 0.08f);
    internal static readonly Color FriendlyColor = new(0.22f, 0.84f, 0.3f);

    internal static string ElementOf(IdIntentKind kind) => kind switch
    {
        IdIntentKind.Hostile => HostileElement,
        IdIntentKind.Friendly => FriendlyElement,
        _ => NeutralElement,
    };

    internal static Color ColorOf(IdIntentKind kind) => kind switch
    {
        IdIntentKind.Hostile => HostileColor,
        IdIntentKind.Friendly => FriendlyColor,
        _ => NeutralColor,
    };

    internal static float UniformScale(float width, float height, float target)
    {
        var longest = Mathf.Max(width, height);
        return longest > 0f ? target / longest : 1f;
    }

    internal static float FitVisible(float left, float width, float height, float target, out float visibleWidth, out float leftInset)
    {
        var scale = UniformScale(width, height, target);
        visibleWidth = width * scale;
        leftInset = left * scale;
        return scale;
    }
}
