using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class IdLabelView
{
    internal const string GraphicsStage = "GraphicsInit";
    private const int NumberBarChild = 0;
    private const int NameBarChild = 1;
    private const int TextChild = 2;
    private const int FirstGlyphChild = 3;
    private const int IntentChild = FirstGlyphChild + IdName.MaxLength;

    private static float _glyphWidth = 15f;
    private static bool _glyphWidthCached;

    private readonly IdAttachBackoff _fail = new();

    internal bool Ready { get; private set; }

    internal object? Surface { get; private set; }

    internal static void ResetGlyphCache()
    {
        _glyphWidth = 15f;
        _glyphWidthCached = false;
    }

    internal void Initiate(IdLabel label, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float now)
    {
        TryBuild(label, sLeaser, rCam, now, applyPalette: false);
    }

    internal void Present(
        IdLabel label,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        float timeStacker,
        Vector2 camPos,
        IdLabelPresentMode mode)
    {
        if (sLeaser.deleteMeNextFrame || label.Abandoned)
        {
            return;
        }

        if (!Ready && mode.AllowRetry)
        {
            TryBuild(label, sLeaser, rCam, Time.realtimeSinceStartup, applyPalette: true);
        }

        if (!Ready || sLeaser.containers is not { Length: > 0 })
        {
            return;
        }

        var top = sLeaser.containers[0];
        var visible = top.isVisible;
        var canDisplay = false;
        Player? viewer = null;
        var showNumber = false;
        var showName = false;
        var showIntent = false;
        if (mode.AllowRetry)
        {
            canDisplay = label.CanDisplay(rCam, out viewer, out showNumber, out showName, out showIntent);
        }

        mode.Apply(Ready, canDisplay, ref visible);
        top.isVisible = visible;
        if (!visible || !mode.AllowRetry)
        {
            return;
        }

        Draw(label, top, timeStacker, camPos, viewer!, showNumber, showName, showIntent);
    }

    internal void ApplyPalette(RoomCamera.SpriteLeaser sLeaser)
    {
        if (sLeaser.sprites == null)
        {
            return;
        }

        for (var i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (sLeaser.sprites[i] != null)
            {
                sLeaser.sprites[i].color = Color.black;
            }
        }
    }

    internal void AddToHud(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
    {
        var hud = newContatiner ?? rCam.ReturnFContainer("HUD");
        if (sLeaser.containers is not { Length: > 0 })
        {
            return;
        }

        var top = sLeaser.containers[0];
        top.RemoveFromContainer();
        hud.AddChild(top);
    }

    private void TryBuild(IdLabel label, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float now, bool applyPalette)
    {
        if (Ready || _fail.IsWaiting(now) || label.Abandoned)
        {
            return;
        }

        try
        {
            BuildSprites(sLeaser, rCam);
            if (applyPalette)
            {
                ApplyPalette(sLeaser);
            }

#if DEBUG
            if (_fail.Attempts >= 2)
            {
                MoreSlugHUDLog.Info(
                    $"ID label graphics recovered after {_fail.Attempts} {_fail.Stage} ({_fail.LastExceptionType}) failures in {now - _fail.FirstFailedAt:0.0}s");
            }
#endif
            NoteGraphicsReady(sLeaser);
        }
        catch (Exception exception)
        {
            FailLocal(sLeaser, exception, now);
        }
    }

    private void BuildSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Detach(sLeaser);
        var numberBar = MakeBar();
        var nameBar = MakeBar();
        var text = new FLabel(Custom.GetDisplayFont(), string.Empty)
        {
            anchorX = 0.5f,
            anchorY = 0.5f,
            scale = MoreSlugHUDConfig.IdLabelScale,
        };
        var top = new FContainer();
        top.AddChild(numberBar);
        top.AddChild(nameBar);
        top.AddChild(text);
        FShader? shader = null;
        rCam.game.rainWorld.Shaders?.TryGetValue("SingleGlyph", out shader);
        for (var i = 0; i < IdName.MaxLength; i++)
        {
            var glyph = new FSprite("glyphs")
            {
                anchorX = 0f,
                anchorY = 0.5f,
                isVisible = false,
                color = Color.white,
            };
            if (shader != null)
            {
                glyph.shader = shader;
            }

            top.AddChild(glyph);
        }

        top.AddChild(new FSprite("pixel")
        {
            anchorX = 0f,
            anchorY = 0.5f,
            isVisible = false,
        });

        sLeaser.sprites = [numberBar, nameBar];
        sLeaser.containers = [top];
        AddToHud(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
    }

    private static FSprite MakeBar() => new("pixel")
    {
        anchorX = 0.5f,
        anchorY = 0.5f,
        scaleY = MoreSlugHUDConfig.IdBarHeight,
        alpha = MoreSlugHUDConfig.IdBarAlpha,
        color = Color.black,
        isVisible = false,
    };

    internal int GraphicsAttempts => _fail.Attempts;

    internal void NoteGraphicsReady(object? surface = null)
    {
        Ready = true;
        if (surface != null)
        {
            Surface = surface;
        }

        _fail.Clear();
    }

    internal void NoteGraphicsFailure(Exception exception, float now)
    {
        Ready = false;
        Surface = null;
        _fail.Record(GraphicsStage, exception, now);
    }

    private void FailLocal(RoomCamera.SpriteLeaser sLeaser, Exception exception, float now)
    {
        Detach(sLeaser);
        NoteGraphicsFailure(exception, now);
        MoreSlugHUDLog.ErrorOnce(
            $"id-attach-{GraphicsStage}-{exception.GetType().FullName}",
            "ID label graphics failed",
            exception);
    }

    private static void Detach(RoomCamera.SpriteLeaser sLeaser)
    {
        try
        {
            if (sLeaser.containers != null)
            {
                for (var i = 0; i < sLeaser.containers.Length; i++)
                {
                    sLeaser.containers[i]?.RemoveFromContainer();
                }
            }

            if (sLeaser.sprites != null)
            {
                for (var i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i]?.RemoveFromContainer();
                }
            }
        }
        catch (Exception cleanup)
        {
            MoreSlugHUDLog.ErrorOnce(
                $"id-attach-DestroyCleanup-{cleanup.GetType().FullName}",
                "ID label sprite detach failed",
                cleanup);
        }

        sLeaser.sprites = [];
        sLeaser.containers = [];
    }

    private static void Draw(
        IdLabel label,
        FContainer top,
        float timeStacker,
        Vector2 camPos,
        Player viewer,
        bool showNumber,
        bool showName,
        bool showIntent)
    {
        var creature = label.Creature;
        var model = label.Model;
        var chunk = creature.mainBodyChunk ?? creature.firstChunk;
        var draw = Vector2.Lerp(chunk.lastPos, chunk.pos, timeStacker) - camPos;
        top.isVisible = true;
        top.SetPosition(draw.x, draw.y + MoreSlugHUDConfig.IdLift);

        var glyphScale = MoreSlugHUDConfig.IdGlyphScale;
        var advance = 15f * glyphScale;
        var glyphs = model.Glyphs;
        var nameWidth = glyphs.Length * advance;
        var nameHeight = 15f * glyphScale;

        var numberWidth = 0f;
        FLabel? text = top.GetChildAt(TextChild) as FLabel;
        if (text != null)
        {
            if (showNumber)
            {
                text.isVisible = true;
                var numberText = model.NumberText(creature);
                if (text.text != numberText)
                {
                    text.text = numberText;
                }

                numberWidth = text.textRect.width;
            }
            else
            {
                text.isVisible = false;
                text.text = string.Empty;
            }
        }

        var glyphWidth = GlyphWidth();
        var intentState = showIntent ? model.PeekIntent(viewer) : IdIntentDrawState.Hidden;
        var intentWidth = PrepareIntent(top, showIntent, intentState.Shown, out var intent, out var intentLeftInset);
        var row = MoreSlugHUDConfig.IdArrange == IdArrange.Row;
        var bothLines = !row && showNumber && showName;
        var line = nameHeight * 0.5f + 4f;
        var numberY = bothLines ? line : 0f;
        var nameY = bothLines ? -line : 0f;
        var numberX = 0f;
        var nameStartX = -nameWidth * 0.5f;
        var intentX = 0f;
        var intentY = 0f;
        var textWidth = 0f;
        if (showNumber)
        {
            textWidth += numberWidth;
        }

        if (showNumber && showName)
        {
            textWidth += MoreSlugHUDConfig.IdItemGap;
        }

        if (showName)
        {
            textWidth += nameWidth;
        }

        var rowWidth = textWidth;
        if (row)
        {
            if (intent != null && (showNumber || showName))
            {
                rowWidth += MoreSlugHUDConfig.IdIntentGap + intentWidth;
            }
            else if (intent != null)
            {
                rowWidth += intentWidth;
            }

            var cursor = -rowWidth * 0.5f;
            numberX = cursor + numberWidth * 0.5f;
            if (showNumber)
            {
                cursor += numberWidth;
            }

            if (showName)
            {
                if (showNumber)
                {
                    cursor += MoreSlugHUDConfig.IdItemGap;
                }

                nameStartX = cursor;
                cursor += nameWidth;
            }

            if (intent != null && (showNumber || showName))
            {
                cursor += MoreSlugHUDConfig.IdIntentGap;
            }

            intentX = cursor;
            intentY = 0f;
            numberY = 0f;
            nameY = 0f;
        }
        else
        {
            numberX = 0f;
            nameStartX = -nameWidth * 0.5f;
            if (showNumber)
            {
                intentX = numberWidth * 0.5f + MoreSlugHUDConfig.IdIntentGap;
                intentY = numberY;
            }
            else if (showName)
            {
                intentX = nameWidth * 0.5f + MoreSlugHUDConfig.IdIntentGap;
                intentY = nameY;
            }
            else
            {
                intentX = -intentWidth * 0.5f;
                intentY = 0f;
            }
        }

        if (text != null && showNumber)
        {
            text.SetPosition(numberX, numberY);
        }

        for (var i = 0; i < IdName.MaxLength; i++)
        {
            if (top.GetChildAt(FirstGlyphChild + i) is not FSprite glyph)
            {
                continue;
            }

            if (!showName || i >= glyphs.Length)
            {
                glyph.isVisible = false;
                continue;
            }

            var index = glyphs[i];
            glyph.isVisible = index >= 0;
            glyph.alpha = index < 0 ? 0f : index / 50f;
            glyph.scaleX = 15f / glyphWidth * glyphScale;
            glyph.scaleY = glyphScale;
            glyph.SetPosition(nameStartX + i * advance, nameY);
        }

        if (intent != null)
        {
            intent.SetPosition(intentX - intentLeftInset + intentState.ShakeOffset.x, intentY + intentState.ShakeOffset.y);
        }

        if (top.GetChildAt(NumberBarChild) is FSprite numberBar && top.GetChildAt(NameBarChild) is FSprite nameBar)
        {
            IdLabelBars.Layout(
                MoreSlugHUDConfig.ShowLabelBackground,
                row,
                bothLines,
                showNumber,
                showName,
                numberWidth,
                nameWidth,
                nameHeight,
                textWidth,
                rowWidth,
                numberY,
                nameY,
                out var numberLayout,
                out var nameLayout);
            PlaceBar(numberBar, numberLayout);
            PlaceBar(nameBar, nameLayout);
        }
    }

    private static void PlaceBar(FSprite sprite, in IdLabelBar bar)
    {
        sprite.isVisible = bar.Visible;
        if (!bar.Visible)
        {
            return;
        }

        sprite.scaleX = bar.Width;
        sprite.scaleY = bar.Height;
        sprite.SetPosition(bar.X, bar.Y);
    }

    private static float PrepareIntent(FContainer top, bool showIntent, IdIntentKind shown, out FSprite? sprite, out float leftInset)
    {
        sprite = null;
        leftInset = 0f;
        if (top.GetChildAt(IntentChild) is not FSprite intent)
        {
            return 0f;
        }

        var element = IdIntent.ElementOf(shown);
        if (!showIntent || !IconLookup.HasElement(element))
        {
            intent.isVisible = false;
            return 0f;
        }

        intent.SetElementByName(element);
        var drawn = intent.element.sourceRect;
        intent.scale = IdIntent.FitVisible(drawn.x, drawn.width, drawn.height, MoreSlugHUDConfig.IdIntentHeight, out var visibleWidth, out leftInset);
        intent.color = IdIntent.ColorOf(shown);
        intent.isVisible = true;
        intent.alpha = 1f;
        sprite = intent;
        return visibleWidth;
    }

    private static float GlyphWidth()
    {
        if (_glyphWidthCached)
        {
            return _glyphWidth;
        }

        if (Futile.atlasManager != null && Futile.atlasManager.DoesContainElementWithName("glyphs"))
        {
            _glyphWidth = Futile.atlasManager.GetElementWithName("glyphs").sourcePixelSize.x;
            _glyphWidthCached = true;
        }

        return _glyphWidth;
    }
}
