using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class IdLabel : CosmeticSprite
{
    private readonly ConditionalWeakTable<RoomCamera.SpriteLeaser, IdLabelView> _views = new();
    private bool _destroyed;

    internal IdLabel(Creature creature)
    {
        Creature = creature;
        Model = new IdLabelModel(creature);
    }

    internal Creature Creature { get; }

    internal IdLabelModel Model { get; }

    internal bool Abandoned => _destroyed || slatedForDeletetion;

    internal void Abandon()
    {
        _destroyed = true;
        Destroy();
    }

    public override void Update(bool eu)
    {
        if (!MoreSlugHUDConfig.IdAttachable || Creature.slatedForDeletetion || Creature.room == null)
        {
            Destroy();
            return;
        }

        if (room != Creature.room)
        {
            room?.RemoveObject(this);
            try
            {
                Creature.room.AddObject(this);
                IdLabelRegistry.ConfirmJoined(Creature, this);
            }
            catch (Exception exception)
            {
                IdLabelRegistry.Rollback(Creature, this, exception, Time.realtimeSinceStartup);
                return;
            }
        }

        Model.Tick(Creature);
        base.Update(eu);
    }

    public override void Destroy()
    {
        IdLabelRegistry.Unindex(Creature, this);
        _destroyed = true;
        RemoveFromRoom();
        base.Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (Abandoned)
        {
            sLeaser.sprites = [];
            sLeaser.containers = [];
            return;
        }

        View(sLeaser).Initiate(this, sLeaser, rCam, Time.realtimeSinceStartup);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        View(sLeaser).AddToHud(sLeaser, rCam, newContatiner);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        View(sLeaser).ApplyPalette(sLeaser);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sLeaser.deleteMeNextFrame)
        {
            return;
        }

        View(sLeaser).Present(this, sLeaser, rCam, timeStacker, camPos, IdLabelPresentMode.DrawSprites);
    }

    public override void PausedDrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.PausedDrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sLeaser.deleteMeNextFrame || !_views.TryGetValue(sLeaser, out var view))
        {
            return;
        }

        var gamePaused = rCam.game != null && rCam.game.GamePaused;
        view.Present(this, sLeaser, rCam, timeStacker, camPos, IdLabelPresentMode.PausedDrawSprites(gamePaused));
    }

    internal bool CanDisplay(
        RoomCamera rCam,
        out Player? viewer,
        out bool showNumber,
        out bool showName,
        out bool showIntent)
    {
        var wantNumber = MoreSlugHUDConfig.ShowId;
        var wantName = MoreSlugHUDConfig.ShowName && Model.Glyphs.Length > 0;
        showIntent = MoreSlugHUDConfig.ShowIntent && !Creature.dead;
        showNumber = wantNumber;
        showName = wantName;
        if (MoreSlugHUDConfig.IdArrange == IdArrange.Cycle && wantNumber && wantName)
        {
            showNumber = Model.CycleShowsNumber(Creature);
            showName = !showNumber;
        }

        viewer = rCam.hud != null ? LocalPlayerBinder.Bind(rCam.hud) : null;
        return MoreSlugHUDConfig.IdAttachable
            && viewer != null
            && SessionVisibility.IsVisible(viewer, HudLayer.Id)
            && Visibility.HideWorld(rCam.hud, rCam, viewer, rCam.game) == null
            && room != null
            && room.BeingViewed
            && Creature.room != null
            && Creature.room.BeingViewed
            && rCam.room == room
            && IdFamiliarity.ShouldShow(Creature, viewer)
            && (showNumber || showName || showIntent);
    }

    private IdLabelView View(RoomCamera.SpriteLeaser sLeaser)
    {
        if (!_views.TryGetValue(sLeaser, out var view))
        {
            view = new IdLabelView();
            _views.Add(sLeaser, view);
        }

        return view;
    }
}
