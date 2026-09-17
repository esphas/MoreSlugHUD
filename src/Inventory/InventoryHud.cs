using System;
using HUD;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class InventoryHud : HudPart
{
    private const float UpdateRetryMin = 0.25f;
    private const float UpdateRetryMax = 5f;
    private const float DrawRetrySeconds = 2f;

    private static readonly SlotId[] SlotOrder =
    {
        SlotId.Craft,
        SlotId.Left,
        SlotId.Right,
        SlotId.Stomach,
        SlotId.Back,
        SlotId.Pyro,
    };

    private readonly SlotView[] _views;
    private readonly InventorySnapshot _snapshot = new();
    private readonly InventoryFailState _slotFails = new();
    private readonly PyroRecovery _pyro = new();
    private Player? _pyroPlayer;
    private bool _hasSnapshot;
    private bool _updateBroken;
    private InputScope _broken;
    private int _updateFails;
    private float _updateRetryAt;
    private bool _drawBroken;
    private int _drawStamp;
    private float _drawRetryAt;
    private InputScope _scope;

    internal InventoryHud(HUD.HUD hud) : base(hud)
    {
        var container = hud.fContainers[1];
        _views = new SlotView[SlotOrder.Length];
        for (var i = 0; i < _views.Length; i++)
        {
            _views[i] = new SlotView(container);
        }
    }

    public override void Update()
    {
        base.Update();
        for (var i = 0; i < _views.Length; i++)
        {
            _views[i].Tick();
        }

        if (!MoreSlugHUDPlugin.Active)
        {
            _hasSnapshot = false;
            _updateBroken = false;
            _drawBroken = false;
            _updateFails = 0;
            _broken = default;
            _scope = default;
            _slotFails.Reset();
            _pyro.Reset();
            _pyroPlayer = null;
            return;
        }

        if (_updateBroken && _broken.Player == null && Time.realtimeSinceStartup < _updateRetryAt)
        {
            _hasSnapshot = false;
            return;
        }

        Player? player = null;
        try
        {
            player = LocalPlayerBinder.Bind(hud);
            if (player != null)
            {
                ObservePyro(player);
            }

            var game = GameContext.FromHud(hud);
            if (Visibility.HideHud(hud, player, game, HudFeatureId.Inventory) != null || player == null)
            {
                _hasSnapshot = false;
                return;
            }

            RefreshFailScope(player);
            if (ShouldDeferUpdate(player))
            {
                _hasSnapshot = false;
                return;
            }

            InventoryReader.Read(player, _snapshot, _slotFails);
            _hasSnapshot = true;
            _updateBroken = false;
            _broken = default;
            _updateFails = 0;
        }
        catch (Exception exception)
        {
            if (!_updateBroken)
            {
                MoreSlugHUDLog.Error("HUD update failed", exception);
            }

            _hasSnapshot = false;
            _updateBroken = true;
            _broken = player == null ? default : InputScope.Capture(player);
            _updateFails++;
            _updateRetryAt = Time.realtimeSinceStartup + UpdateDelay(_updateFails);
        }
    }

    public override void Draw(float timeStacker)
    {
        base.Draw(timeStacker);
        try
        {
            if (!MoreSlugHUDPlugin.Active || !_hasSnapshot)
            {
                HideAll();
                return;
            }

            var stamp = _snapshot.ContentStamp();
            if (_drawBroken)
            {
                if (stamp == _drawStamp && Time.realtimeSinceStartup < _drawRetryAt)
                {
                    HideAll();
                    return;
                }

                _drawBroken = false;
            }

            var origin = InventoryLayout.OriginOf(hud);
            for (var i = 0; i < SlotOrder.Length; i++)
            {
                var id = SlotOrder[i];
                var content = _snapshot[id];
                if (!content.OccupiesLayout)
                {
                    _views[i].Hide();
                    continue;
                }

                if (id == SlotId.Pyro)
                {
                    content.PyroFill = _pyro.Fill;
                }

                _views[i].Draw(content, origin + MoreSlugHUDConfig.SlotLocal(id), timeStacker);
            }
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("inventory-draw", "HUD draw failed", exception);
            _drawBroken = true;
            _drawStamp = _snapshot.ContentStamp();
            _drawRetryAt = Time.realtimeSinceStartup + DrawRetrySeconds;
            HideAll();
        }
    }

    public override void ClearSprites()
    {
        base.ClearSprites();
        for (var i = 0; i < _views.Length; i++)
        {
            _views[i].Remove();
        }
    }

    private void ObservePyro(Player player)
    {
        if (!ReferenceEquals(_pyroPlayer, player))
        {
            _pyro.Reset();
            _pyroPlayer = player;
        }

        if (!DownpourCompat.HasPyroMechanics(player))
        {
            _pyro.Observe(0, 0f);
            return;
        }

        _pyro.Observe(player.pyroJumpCounter, player.pyroJumpCooldown);
    }

    private void RefreshFailScope(Player player)
    {
        var next = InputScope.Capture(player);
        if (!_scope.SamePlayerRoom(next))
        {
            _slotFails.Reset();
        }

        if (!_scope.SameInput(next))
        {
            _updateFails = 0;
            _updateRetryAt = 0f;
            _updateBroken = false;
            _broken = default;
        }

        _scope = next;
    }

    private bool ShouldDeferUpdate(Player player)
    {
        if (!_updateBroken)
        {
            return false;
        }

        if (!_broken.SameInput(InputScope.Capture(player)))
        {
            return false;
        }

        return Time.realtimeSinceStartup < _updateRetryAt;
    }

    private static float UpdateDelay(int fails)
    {
        var shift = Math.Min(Math.Max(fails, 1), 5) - 1;
        return Math.Min(UpdateRetryMax, UpdateRetryMin * (1 << shift));
    }

    private readonly struct InputScope
    {
        internal Player? Player { get; }
        internal Room? Room { get; }
        internal PhysicalObject? Left { get; }
        internal PhysicalObject? Right { get; }
        internal AbstractPhysicalObject? Stomach { get; }
        internal int Food { get; }

        internal static InputScope Capture(Player player) => new(
            player,
            player.room,
            Grasp(player, 0),
            Grasp(player, 1),
            player.objectInStomach,
            player.FoodInStomach);

        private InputScope(
            Player? player,
            Room? room,
            PhysicalObject? left,
            PhysicalObject? right,
            AbstractPhysicalObject? stomach,
            int food)
        {
            Player = player;
            Room = room;
            Left = left;
            Right = right;
            Stomach = stomach;
            Food = food;
        }

        internal bool SamePlayerRoom(InputScope other) =>
            ReferenceEquals(Player, other.Player) && ReferenceEquals(Room, other.Room);

        internal bool SameInput(InputScope other) =>
            SamePlayerRoom(other)
            && ReferenceEquals(Left, other.Left)
            && ReferenceEquals(Right, other.Right)
            && ReferenceEquals(Stomach, other.Stomach)
            && Food == other.Food;

        private static PhysicalObject? Grasp(Player player, int index)
        {
            if (player.grasps == null || index >= player.grasps.Length)
            {
                return null;
            }

            return player.grasps[index]?.grabbed;
        }
    }

    private void HideAll()
    {
        for (var i = 0; i < _views.Length; i++)
        {
            _views[i].Hide();
        }
    }
}
