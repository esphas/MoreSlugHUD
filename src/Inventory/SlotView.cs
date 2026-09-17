using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class SlotView
{
    private const int RingSegments = 24;
    private const float RingInner = 13f;
    private const float RingOuter = 16f;

    private readonly FContainer _container;
    private readonly FSprite _placeholder;
    private readonly FSprite[] _content;
    private TriangleMesh? _pyroTrack;
    private TriangleMesh? _pyroFill;
    private FSprite? _pyroIcon;
    private FLabel? _pyroLabel;
    private string? _last0;
    private string? _last1;
    private string? _last2;
    private int _lastCount = -1;
    private float _pulse;
    private float _lastPulse;

    internal SlotView(FContainer container)
    {
        _container = container;
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

        if (slot.Id == SlotId.Pyro)
        {
            HideContent();
            _placeholder.isVisible = false;
            DrawPyro(slot, position);
            return;
        }

        HidePyro();

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
            ApplyIconShader(sprite, slot.IsPickUpCandidate);
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
        HidePyro();
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

        _pyroTrack?.RemoveFromContainer();
        _pyroFill?.RemoveFromContainer();
        _pyroIcon?.RemoveFromContainer();
        _pyroLabel?.RemoveFromContainer();
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
            ApplyIconShader(_content[i], candidate: false);
            _content[i].isVisible = false;
        }
    }

    private static void ApplyIconShader(FSprite sprite, bool candidate)
    {
        var shaders = RWCustom.Custom.rainWorld?.Shaders;
        if (shaders == null)
        {
            return;
        }

        if (candidate && shaders.TryGetValue("GateHologram", out var hologram))
        {
            sprite.shader = hologram;
            return;
        }

        if (shaders.TryGetValue("Basic", out var basic))
        {
            sprite.shader = basic;
        }
    }

    private void DrawPyro(SlotContent slot, Vector2 position)
    {
        EnsurePyro();
        var remaining = Mathf.Max(0, slot.PyroCapacity - slot.PyroHeat);
        var color = MoreSlugHUDConfig.PyroWarningColor(slot.PyroHeat, slot.PyroCapacity);
        PlaceRing(_pyroTrack!, position, 1f);
        _pyroTrack!.color = color;
        _pyroTrack.alpha = 0.28f;
        _pyroTrack.isVisible = true;
        PlaceRing(_pyroFill!, position, slot.PyroFill);
        _pyroFill!.color = color;
        _pyroFill.alpha = 0.95f;
        _pyroFill.isVisible = true;

        var bomb = IconLookup.Bomb();
        if (_pyroIcon!.element?.name != bomb.SpriteName)
        {
            _pyroIcon.SetElementByName(bomb.SpriteName);
        }

        _pyroIcon.color = color;
        _pyroIcon.SetPosition(position);
        _pyroIcon.rotation = 0f;
        _pyroIcon.scale = FitScale(_pyroIcon, MoreSlugHUDConfig.SlotSize * 0.55f);
        _pyroIcon.alpha = 1f;
        _pyroIcon.isVisible = true;
        _pyroLabel!.text = remaining.ToString();
        _pyroLabel.color = color;
        _pyroLabel.SetPosition(position + new Vector2(0f, -MoreSlugHUDConfig.SlotSize * 0.5f - 6f));
        _pyroLabel.isVisible = true;
    }

    private void EnsurePyro()
    {
        if (_pyroTrack != null)
        {
            return;
        }

        _pyroTrack = MakeRing();
        _pyroFill = MakeRing();
        _container.AddChild(_pyroTrack);
        _container.AddChild(_pyroFill);
        _pyroIcon = CreateSprite(_container);
        _pyroLabel = new FLabel(Custom.GetDisplayFont(), "0")
        {
            alignment = FLabelAlignment.Center,
            scale = 0.7f,
            isVisible = false,
        };
        _container.AddChild(_pyroLabel);
    }

    private void HidePyro()
    {
        if (_pyroTrack == null)
        {
            return;
        }

        _pyroTrack.isVisible = false;
        _pyroFill!.isVisible = false;
        _pyroIcon!.isVisible = false;
        _pyroLabel!.isVisible = false;
    }

    private static TriangleMesh MakeRing()
    {
        var tris = new TriangleMesh.Triangle[RingSegments * 2];
        for (var i = 0; i < RingSegments; i++)
        {
            var outer0 = i;
            var outer1 = i + 1;
            var inner0 = RingSegments + 1 + i;
            var inner1 = RingSegments + 1 + i + 1;
            tris[i * 2] = new TriangleMesh.Triangle(outer0, outer1, inner0);
            tris[i * 2 + 1] = new TriangleMesh.Triangle(inner0, outer1, inner1);
        }

        var mesh = new TriangleMesh("Futile_White", tris, customColor: false)
        {
            isVisible = false,
        };
        mesh.SetPosition(0f, 0f);
        return mesh;
    }

    private static void PlaceRing(TriangleMesh mesh, Vector2 center, float fill)
    {
        var shown = fill >= 0.999f ? RingSegments : Mathf.Max(0, Mathf.CeilToInt(fill * RingSegments));
        for (var i = 0; i <= RingSegments; i++)
        {
            var t = i / (float)RingSegments;
            var ang = t * Mathf.PI * 2f;
            var dir = new Vector2(Mathf.Sin(ang), Mathf.Cos(ang));
            var outer = i <= shown ? RingOuter : RingInner;
            mesh.MoveVertice(i, center + dir * outer);
            mesh.MoveVertice(RingSegments + 1 + i, center + dir * RingInner);
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
