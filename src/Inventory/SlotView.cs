using UnityEngine;

namespace MoreSlugHUD;

internal sealed class SlotView
{
    private readonly FSprite _placeholder;
    private readonly FSprite[] _content;
    private string? _last0;
    private string? _last1;
    private string? _last2;
    private int _lastCount = -1;
    private float _pulse;
    private float _lastPulse;

    internal SlotView(FContainer container)
    {
        _placeholder = CreateSprite(container);
        _content = new FSprite[MoreSlugHUDConfig.MaxStack];
        for (var i = 0; i < _content.Length; i++)
        {
            _content[i] = CreateSprite(container);
        }
    }

    internal void Tick()
    {
        _lastPulse = _pulse;
        _pulse += MoreSlugHUDConfig.CraftPulseSpeed;
    }

    internal void Draw(SlotContent slot, Vector2 position, float timeStacker)
    {
        if (!slot.OccupiesLayout)
        {
            Hide();
            return;
        }

        if (slot.ShowPlaceholder)
        {
            HideContent();
            DrawPlaceholder(slot.Id, position);
            return;
        }

        _placeholder.isVisible = false;
        var count = Mathf.Min(slot.StackCount, _content.Length);
        var changed = count != _lastCount
            || (count > 0 && slot.Icon0.SpriteName != _last0)
            || (count > 1 && slot.Icon1.SpriteName != _last1)
            || (count > 2 && slot.Icon2.SpriteName != _last2);
        _lastCount = count;
        _last0 = count > 0 ? slot.Icon0.SpriteName : null;
        _last1 = count > 1 ? slot.Icon1.SpriteName : null;
        _last2 = count > 2 ? slot.Icon2.SpriteName : null;
        var craftGrey = slot.Id == SlotId.Craft
            ? BreathGrey(timeStacker)
            : (Color?)null;
        for (var i = 0; i < _content.Length; i++)
        {
            if (i >= count)
            {
                _content[i].isVisible = false;
                continue;
            }

            var icon = slot.IconAt(i);
            var sprite = _content[i];
            if (changed)
            {
                sprite.SetElementByName(icon.SpriteName);
            }

            sprite.color = craftGrey.HasValue
                ? Color.Lerp(icon.Color, craftGrey.Value, MoreSlugHUDConfig.CraftGreyMix)
                : icon.Color;
            var offsetIndex = i - (count - 1) * 0.5f;
            var offset = new Vector2(offsetIndex * MoreSlugHUDConfig.StackOffset, offsetIndex * MoreSlugHUDConfig.StackOffset * 0.4f);
            sprite.SetPosition(position + offset);
            sprite.rotation = 0f;
            sprite.scale = FitScale(sprite, MoreSlugHUDConfig.SlotSize * icon.Scale);
            sprite.alpha = slot.ContentAlpha;
            sprite.isVisible = true;
        }
    }

    private Color BreathGrey(float timeStacker)
    {
        var wobble = Mathf.Sin(Mathf.Lerp(_lastPulse, _pulse, timeStacker)) * MoreSlugHUDConfig.CraftPulseAmplitude;
        var grey = MoreSlugHUDConfig.CraftPulseGrey + wobble;
        return new Color(grey, grey, grey);
    }

    internal void Hide()
    {
        _placeholder.isVisible = false;
        HideContent();
        _lastCount = -1;
        _last0 = null;
        _last1 = null;
        _last2 = null;
    }

    internal void Remove()
    {
        _placeholder.RemoveFromContainer();
        for (var i = 0; i < _content.Length; i++)
        {
            _content[i].RemoveFromContainer();
        }
    }

    private void DrawPlaceholder(SlotId id, Vector2 position)
    {
        _placeholder.SetElementByName(IconLookup.PlaceholderElement());
        _placeholder.color = IconLookup.PlaceholderColor();
        _placeholder.SetPosition(position);
        _placeholder.alpha = MoreSlugHUDConfig.EmptyAlpha;
        ApplyPlaceholderTransform(id, _placeholder);
        _placeholder.isVisible = true;
    }

    private void HideContent()
    {
        for (var i = 0; i < _content.Length; i++)
        {
            _content[i].isVisible = false;
        }
    }

    private static void ApplyPlaceholderTransform(SlotId id, FSprite sprite)
    {
        var baseScale = FitScale(sprite, MoreSlugHUDConfig.SlotSize);
        switch (id)
        {
            case SlotId.Left:
                sprite.scaleX = baseScale * 1.15f;
                sprite.scaleY = baseScale * 0.55f;
                sprite.rotation = -25f;
                break;
            case SlotId.Right:
                sprite.scaleX = baseScale * 1.15f;
                sprite.scaleY = baseScale * 0.55f;
                sprite.rotation = 25f;
                break;
            case SlotId.Stomach:
                sprite.scaleX = baseScale * 0.7f;
                sprite.scaleY = baseScale * 0.7f;
                sprite.rotation = 0f;
                break;
            case SlotId.Back:
                sprite.scaleX = baseScale * 1.15f;
                sprite.scaleY = baseScale * 1.15f;
                sprite.rotation = 0f;
                break;
            default:
                sprite.scale = baseScale;
                sprite.rotation = 0f;
                break;
        }
    }

    private static float FitScale(FSprite sprite, float maxSize)
    {
        var width = Mathf.Max(1f, sprite.element?.sourcePixelSize.x ?? sprite.width);
        var height = Mathf.Max(1f, sprite.element?.sourcePixelSize.y ?? sprite.height);
        var longest = Mathf.Max(width, height);
        return longest > maxSize ? maxSize / longest : 1f;
    }

    private static FSprite CreateSprite(FContainer container)
    {
        var sprite = new FSprite("Futile_White")
        {
            anchorX = 0.5f,
            anchorY = 0.5f,
            isVisible = false,
        };
        container.AddChild(sprite);
        return sprite;
    }
}
