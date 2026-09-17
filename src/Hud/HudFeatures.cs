using System;
using HUD;

namespace MoreSlugHUD;

internal readonly struct ScreenHudAttachment
{
    internal ScreenHudAttachment(
        HudFeatureId id,
        string name,
        Func<HudPart, bool> alreadyAttached,
        Func<HUD.HUD, HudPart> create)
    {
        Id = id;
        Name = name;
        AlreadyAttached = alreadyAttached;
        Create = create;
    }

    internal HudFeatureId Id { get; }
    internal string Name { get; }
    internal Func<HudPart, bool> AlreadyAttached { get; }
    internal Func<HUD.HUD, HudPart> Create { get; }
}

internal static class HudFeatures
{
    internal static readonly ScreenHudAttachment[] ScreenHuds =
    {
        new(HudFeatureId.Inventory, "inventory", part => part is InventoryHud, hud => new InventoryHud(hud)),
        new(HudFeatureId.InputHistory, "history", part => part is InputHistoryHud, hud => new InputHistoryHud(hud)),
    };

    internal static void Apply()
    {
        Try("hud-hooks", HudHooks.Apply);
        Try("id-hooks", IdHooks.Apply);
        Try("player-input-hooks", PlayerInputHooks.Apply);
    }

    internal static void Remove()
    {
        Try("player-input-hooks-remove", PlayerInputHooks.Remove);
        Try("id-hooks-remove", IdHooks.Remove);
        Try("hud-hooks-remove", HudHooks.Remove);
    }

    private static void Try(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.Error($"{name} failed", exception);
        }
    }
}
