using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class HistoryRowView
{
    private static readonly string[] DurationText = CreateDurationText();

    private readonly FLabel _duration;
    private readonly FSprite[] _stateIcons;
    private readonly FLabel[] _stateLetters;
    private readonly FSprite _direction;
    private readonly FLabel _neutral;
    private readonly FSprite[] _actionIcons;
    private readonly FLabel[] _actionLetters;
    private byte _lastDuration = 255;

    internal HistoryRowView(FContainer container)
    {
        _duration = CreateLabel(container, FLabelAlignment.Right);
        _stateIcons = new FSprite[HudIcons.MaxSimultaneousStates];
        _stateLetters = new FLabel[HudIcons.MaxSimultaneousStates];
        for (var i = 0; i < HudIcons.MaxSimultaneousStates; i++)
        {
            _stateIcons[i] = CreateIcon(container);
            _stateLetters[i] = CreateLabel(container, FLabelAlignment.Center);
        }

        _direction = CreateIcon(container);
        _neutral = CreateLabel(container, FLabelAlignment.Center);

        _actionIcons = new FSprite[HudIcons.ActionCount];
        _actionLetters = new FLabel[HudIcons.ActionCount];
        for (var i = 0; i < HudIcons.ActionCount; i++)
        {
            _actionIcons[i] = CreateIcon(container);
            _actionLetters[i] = CreateLabel(container, FLabelAlignment.Center);
        }
    }

    internal void Draw(HistoryEntry? entry, float left, float y, float alpha, bool alignLeft)
    {
        if (entry == null)
        {
            Hide();
            return;
        }

        var letters = InputHistoryConfig.DisplayMode == DisplayMode.Letters;
        var showStates = InputHistoryConfig.ShowStates;
        var showDir = InputHistoryConfig.TrackDirection;
        var state = entry.State;
        var stateSlot = HudIcons.StateSlot;
        var actionSlot = HudIcons.ActionSlot;
        var dirSlot = HudIcons.Slot;
        var gap = HudIcons.DirectionGap;
        var stateWidth = showStates ? HudIcons.MaxSimultaneousStates * stateSlot : 0f;
        var dirBlock = showDir ? gap + dirSlot : InputHistoryConfig.TrackedActionCount > 0 ? gap : 0f;
        float stateBandLeft;
        float durationX;
        float directionX;
        float actionOrigin;
        if (alignLeft)
        {
            stateBandLeft = left;
            durationX = left + stateWidth + HudIcons.DurationColumn;
            directionX = durationX + gap + dirSlot * 0.5f;
            actionOrigin = durationX + dirBlock;
        }
        else
        {
            var right = left + HudIcons.RowWidth;
            stateBandLeft = right - stateWidth;
            durationX = stateBandLeft - HudIcons.DurationColumn;
            directionX = durationX - gap - dirSlot * 0.5f;
            actionOrigin = durationX - dirBlock;
        }

        _duration.isVisible = true;
        if (_lastDuration != entry.Duration)
        {
            _lastDuration = entry.Duration;
            _duration.text = DurationText[entry.Duration];
        }
        _duration.alignment = alignLeft ? FLabelAlignment.Right : FLabelAlignment.Left;
        _duration.x = durationX;
        _duration.y = y;
        _duration.scale = HudIcons.DurationScale;
        _duration.alpha = alpha;

        var packed = 0;
        if (showStates)
        {
            for (var i = 0; i < HudIcons.StateOrder.Length; i++)
            {
                if ((state.Movement & HudIcons.StateOrder[i]) == 0)
                {
                    continue;
                }

                if (packed >= HudIcons.MaxSimultaneousStates)
                {
                    break;
                }

                var slotIndex = alignLeft
                    ? HudIcons.MaxSimultaneousStates - 1 - packed
                    : packed;
                var x = stateBandLeft + (slotIndex + 0.5f) * stateSlot;
                if (letters)
                {
                    PlaceLetter(_stateLetters[packed], HudIcons.StateLetters[i], x, y, alpha);
                }
                else
                {
                    PlaceIcon(_stateIcons[packed], HudIcons.StateElements[i], on: true, x, y, alpha);
                }

                packed++;
            }
        }

        for (var i = packed; i < HudIcons.MaxSimultaneousStates; i++)
        {
            _stateIcons[i].isVisible = false;
            _stateLetters[i].isVisible = false;
        }

        if (letters)
        {
            for (var i = 0; i < packed; i++)
            {
                _stateIcons[i].isVisible = false;
            }
        }
        else
        {
            for (var i = 0; i < packed; i++)
            {
                _stateLetters[i].isVisible = false;
            }
        }

        if (showDir)
        {
            var neutral = state.X == 0 && state.Y == 0;
            if (letters && neutral)
            {
                _direction.isVisible = false;
                PlaceLetter(_neutral, HudIcons.NeutralLetter, directionX, y, alpha);
            }
            else
            {
                _neutral.isVisible = false;
                PlaceIcon(
                    _direction,
                    HudIcons.DirectionElement(state.X, state.Y),
                    on: true,
                    directionX,
                    y,
                    alpha);
            }
        }
        else
        {
            _direction.isVisible = false;
            _neutral.isVisible = false;
        }

        var showActions = InputHistoryConfig.TrackActions;
        var actionPacked = 0;
        if (showActions)
        {
            for (var i = 0; i < HudIcons.ActionCount; i++)
            {
                var pressed = i switch
                {
                    0 => state.Jump,
                    1 => state.Throw,
                    2 => state.Pickup,
                    _ => state.Special,
                };
                if (!pressed)
                {
                    continue;
                }

                var actionX = alignLeft
                    ? actionOrigin + (actionPacked + 0.5f) * actionSlot
                    : actionOrigin - (actionPacked + 0.5f) * actionSlot;
                if (letters)
                {
                    PlaceLetter(_actionLetters[actionPacked], HudIcons.ActionLetters[i], actionX, y, alpha);
                    _actionIcons[actionPacked].isVisible = false;
                }
                else
                {
                    PlaceIcon(_actionIcons[actionPacked], HudIcons.ActionElements[i], on: true, actionX, y, alpha);
                    _actionLetters[actionPacked].isVisible = false;
                }

                actionPacked++;
            }
        }

        for (var i = actionPacked; i < HudIcons.ActionCount; i++)
        {
            _actionIcons[i].isVisible = false;
            _actionLetters[i].isVisible = false;
        }
    }

    internal void Hide()
    {
        _duration.isVisible = false;
        _lastDuration = 255;
        _direction.isVisible = false;
        _neutral.isVisible = false;
        for (var i = 0; i < HudIcons.MaxSimultaneousStates; i++)
        {
            _stateIcons[i].isVisible = false;
            _stateLetters[i].isVisible = false;
        }

        for (var i = 0; i < HudIcons.ActionCount; i++)
        {
            _actionIcons[i].isVisible = false;
            _actionLetters[i].isVisible = false;
        }
    }

    internal void Remove()
    {
        _duration.RemoveFromContainer();
        _direction.RemoveFromContainer();
        _neutral.RemoveFromContainer();
        for (var i = 0; i < HudIcons.MaxSimultaneousStates; i++)
        {
            _stateIcons[i].RemoveFromContainer();
            _stateLetters[i].RemoveFromContainer();
        }

        for (var i = 0; i < HudIcons.ActionCount; i++)
        {
            _actionIcons[i].RemoveFromContainer();
            _actionLetters[i].RemoveFromContainer();
        }
    }

    private static FSprite CreateIcon(FContainer container)
    {
        var sprite = new FSprite("pixel")
        {
            isVisible = false,
            anchorX = 0.5f,
            anchorY = 0.5f,
        };
        container.AddChild(sprite);
        return sprite;
    }

    private static FLabel CreateLabel(FContainer container, FLabelAlignment alignment)
    {
        var label = new FLabel(Custom.GetDisplayFont(), string.Empty)
        {
            alignment = alignment,
            isVisible = false,
        };
        container.AddChild(label);
        return label;
    }

    private static void PlaceIcon(
        FSprite sprite,
        string element,
        bool on,
        float x,
        float y,
        float alpha)
    {
        if (!on || !HudIcons.HasElement(element))
        {
            sprite.isVisible = false;
            return;
        }

        sprite.element = Futile.atlasManager.GetElementWithName(element);
        sprite.isVisible = true;
        sprite.x = x;
        sprite.y = y;
        sprite.scale = 1f;
        sprite.alpha = alpha;
        sprite.color = Color.white;
    }

    private static void PlaceLetter(FLabel label, string text, float x, float y, float alpha)
    {
        label.isVisible = true;
        label.text = text;
        label.x = x;
        label.y = y;
        label.scale = HudIcons.DurationScale;
        label.alpha = alpha;
        label.color = Color.white;
    }

    private static string[] CreateDurationText()
    {
        var values = new string[HistoryEntry.MaxDuration + 1];
        for (var i = 0; i <= HistoryEntry.MaxDuration; i++)
        {
            values[i] = i.ToString();
        }

        return values;
    }
}
