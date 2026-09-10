using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal static class DownpourCompat
{
    private static bool _probed;
    private static MethodInfo? _creatureData;
    private static PropertyInfo? _expeditionUnlocks;
    private static FieldInfo? _npcStomach;
    private static readonly Dictionary<Type, FieldInfo?> PulsePos = new();
    private static readonly object[] ComboArgs = new object[2];
    private static int _comboBusy;
    private static bool _unlockCached;
    private static RainWorldGame? _unlockGame;
    private static int _unlockClock = int.MinValue;
    private static bool _unlockHas;
    private static float _unlockRetryAt;

    internal static void Reset()
    {
        _probed = false;
        _creatureData = null;
        _expeditionUnlocks = null;
        _npcStomach = null;
        ComboArgs[0] = null!;
        ComboArgs[1] = null!;
        _comboBusy = 0;
        _unlockCached = false;
        _unlockGame = null;
        _unlockClock = int.MinValue;
        _unlockHas = false;
        _unlockRetryAt = 0f;
        PulsePos.Clear();
    }

    internal static void Probe()
    {
        if (_probed)
        {
            return;
        }

        _probed = true;
        try
        {
            var game = typeof(RainWorld).Assembly;
            _creatureData = game.GetType("MoreSlugcats.GourmandCombos")?.GetMethod(
                "CraftingResults_CreatureData",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(Creature.Grasp), typeof(Creature.Grasp) },
                null);
            _expeditionUnlocks = game.GetType("Expedition.ExpeditionGame")?.GetProperty(
                "activeUnlocks",
                BindingFlags.Public | BindingFlags.Static);
            _npcStomach = game.GetType("MoreSlugcats.PlayerNPCState")?.GetField(
                "StomachObject",
                BindingFlags.Public | BindingFlags.Instance);
        }
        catch (Exception exception)
        {
            _creatureData = null;
            _expeditionUnlocks = null;
            _npcStomach = null;
            MoreSlugHUDLog.Error("Downpour probe failed; DLC features disabled", exception);
        }
    }

    internal static bool IsArtificer(Player player) =>
        ModManager.MSC && player.SlugCatClass?.value == "Artificer";

    internal static bool IsGourmand(Player player) =>
        ModManager.MSC && player.SlugCatClass?.value == "Gourmand";

    internal static bool IsSpearmaster(Player player) =>
        ModManager.MSC && player.SlugCatClass?.value == "Spear";

    internal static bool HasExpeditionCrafting()
    {
        if (!ModManager.Expedition || Custom.rainWorld is not { ExpeditionMode: true })
        {
            _unlockCached = false;
            return false;
        }

        if (_unlockRetryAt > 0f && Time.realtimeSinceStartup < _unlockRetryAt)
        {
            return false;
        }

        if (CurrentGame() is not { } game)
        {
            _unlockCached = false;
            return false;
        }

        if (_unlockCached
            && ReferenceEquals(_unlockGame, game)
            && game.clock == _unlockClock)
        {
            return _unlockHas;
        }

        Probe();
        if (!TryReadCraftingUnlock(out var has))
        {
            return false;
        }

        _unlockCached = true;
        _unlockGame = game;
        _unlockClock = game.clock;
        _unlockHas = has;
        return has;
    }

    private static bool TryReadCraftingUnlock(out bool has)
    {
        has = false;
        try
        {
            if (_expeditionUnlocks?.GetValue(null) is not IEnumerable unlocks)
            {
                return true;
            }

            foreach (var unlock in unlocks)
            {
                if (unlock is string name && name == "unl-crafting")
                {
                    has = true;
                    return true;
                }
            }

            return true;
        }
        catch (Exception exception)
        {
            _unlockRetryAt = Time.realtimeSinceStartup + 5f;
            _unlockCached = false;
            MoreSlugHUDLog.ErrorOnce("expedition-unlocks", "Expedition unlock read failed; will retry", exception);
            return false;
        }
    }

    private static RainWorldGame? CurrentGame() =>
        Custom.rainWorld?.processManager?.currentMainLoop as RainWorldGame;

    internal static CreatureTemplate.Type? ComboCreature(Creature.Grasp? left, Creature.Grasp? right)
    {
        if (!ModManager.MSC || left == null || right == null)
        {
            return null;
        }

        Probe();
        if (_creatureData == null)
        {
            return null;
        }

        var nested = _comboBusy > 0;
        var args = nested ? new object[2] : ComboArgs;
        _comboBusy++;
        args[0] = left;
        args[1] = right;
        try
        {
            return _creatureData.Invoke(null, args) as CreatureTemplate.Type;
        }
        catch (Exception exception)
        {
            _creatureData = null;
            MoreSlugHUDLog.Error("Gourmand creature craft query failed", exception);
            return null;
        }
        finally
        {
            args[0] = null!;
            args[1] = null!;
            _comboBusy--;
        }
    }

    internal static AbstractPhysicalObject? TryNpcStomach(Player player)
    {
        if (!ModManager.MSC)
        {
            return null;
        }

        Probe();
        var state = player.playerState;
        if (_npcStomach == null || state == null || state.GetType() != _npcStomach.DeclaringType)
        {
            return null;
        }

        try
        {
            return _npcStomach.GetValue(state) as AbstractPhysicalObject;
        }
        catch (Exception exception)
        {
            _npcStomach = null;
            MoreSlugHUDLog.Error("PlayerNPCState stomach read failed", exception);
            return null;
        }
    }

    internal static float PulseBandTop(HUD.HUD hud)
    {
        var parts = hud.parts;
        if (parts == null)
        {
            return 0f;
        }

        var top = 0f;
        var extent = MoreSlugHUDConfig.BottomPulseExtent;
        for (var i = 0; i < parts.Count; i++)
        {
            if (TryPulsePos(parts[i], out var pos))
            {
                top = Mathf.Max(top, pos.y + extent);
            }
        }

        return top;
    }

    private static bool TryPulsePos(HudPart part, out Vector2 pos)
    {
        pos = default;
        var type = part.GetType();
        var name = type.Name;
        if (name != "BreathMeter" && name != "ThreatPulser")
        {
            return false;
        }

        if (!PulsePos.TryGetValue(type, out var field))
        {
            field = type.GetField("pos", BindingFlags.Public | BindingFlags.Instance);
            PulsePos[type] = field;
        }

        if (field?.GetValue(part) is Vector2 value)
        {
            pos = value;
            return true;
        }

        return false;
    }
}
