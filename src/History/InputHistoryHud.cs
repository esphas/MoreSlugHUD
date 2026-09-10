using System;
using HUD;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class InputHistoryHud : HudPart
{
    private HistoryRowView[] _rows = Array.Empty<HistoryRowView>();

    internal InputHistoryHud(HUD.HUD hud) : base(hud)
    {
        HudIcons.EnsureLoaded();
        EnsureRows(InputHistoryConfig.EffectiveMaxRows);
    }

    public override void Update()
    {
        base.Update();
        if (!MoreSlugHUDPlugin.Active)
        {
            return;
        }

        var player = LocalPlayerBinder.Bind(hud);
        Visibility.PollToggle(
            player,
            MoreSlugHUDPlugin.ToggleHistoryKeybind,
            HudLayer.History,
            InputHistoryConfig.Enabled,
            "history-toggle");
    }

    public override void Draw(float timeStacker)
    {
        base.Draw(timeStacker);
        try
        {
            DrawHistory();
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("history-draw", "history HUD draw failed", exception);
            HideAll();
        }
    }

    private void DrawHistory()
    {
        if (!MoreSlugHUDPlugin.Active)
        {
            HideAll();
            return;
        }

        EnsureRows(InputHistoryConfig.EffectiveMaxRows);
        var game = GameContext.FromHud(hud);
        var player = LocalPlayerBinder.Bind(hud);
        InputTimeline? timeline = null;
        if (player != null)
        {
            PlayerTimelines.TryGet(player, out timeline);
        }

        if (timeline == null || Visibility.HideHud(hud, player, game, HudLayer.History) != null)
        {
            HideAll();
            return;
        }

        var screen = hud.rainWorld.options.ScreenSize;
        var safe = hud.rainWorld.options.SafeScreenOffset;
        InputHistoryConfig.ResolveAnchor(screen, safe, out var x, out var y, out var alignLeft);
        var left = alignLeft ? x : x - HudIcons.RowWidth;

        for (var i = 0; i < _rows.Length; i++)
        {
            var entry = i < timeline!.Entries.Count ? timeline.Entries[i] : null;
            var alpha = InputHistoryConfig.Opacity * Mathf.Lerp(1f, 0.45f, i / (float)_rows.Length);
            _rows[i].Draw(entry, left, y - i * InputHistoryConfig.LineHeight, alpha, alignLeft);
        }
    }

    private void EnsureRows(int count)
    {
        if (_rows.Length == count)
        {
            return;
        }

        foreach (var row in _rows)
        {
            row.Remove();
        }

        _rows = new HistoryRowView[count];
        var container = hud.fContainers[1];
        for (var i = 0; i < count; i++)
        {
            _rows[i] = new HistoryRowView(container);
        }
    }

    private void HideAll()
    {
        for (var i = 0; i < _rows.Length; i++)
        {
            _rows[i].Hide();
        }
    }

    public override void ClearSprites()
    {
        base.ClearSprites();
        foreach (var row in _rows)
        {
            row.Remove();
        }

        _rows = Array.Empty<HistoryRowView>();
    }
}
