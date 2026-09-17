using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class InventoryPositionCanvas : UIfocusable
{
    private const float Pad = 8f;
    private const float DragSlop = 4f;

    private readonly MoreSlugHUDOptions _owner;
    private readonly FSprite _screenFill;
    private readonly FSprite _safeFill;
    private readonly FSprite _nativeFill;
    private readonly FSprite _originH;
    private readonly FSprite _originV;
    private readonly FSprite[] _slots;
    private Vector2 _screen = new(InventoryLayoutPresets.FallbackScreenX, InventoryLayoutPresets.FallbackScreenY);
    private Vector2 _safe;
    private float _scale = 0.25f;
    private Vector2 _screenOrigin;
    private bool _maybeDrag;
    private bool _drag;
    private Vector2 _pressLocal;
    private Vector2 _pressPercent;

    internal InventoryPositionCanvas(Vector2 pos, Vector2 size, MoreSlugHUDOptions owner) : base(pos, size)
    {
        _owner = owner;
        description = OptionInterface.Translate(LocKeys.OptionsPositionCanvasHint);
        var back = Sprite("pixel", Color.black, 0.9f);
        back.anchorX = 0f;
        back.anchorY = 0f;
        back.scaleX = size.x;
        back.scaleY = size.y;
        _screenFill = Sprite("pixel", new Color(0.18f, 0.18f, 0.2f), 1f);
        _safeFill = Sprite("pixel", new Color(0.28f, 0.32f, 0.28f), 0.55f);
        _nativeFill = Sprite("pixel", new Color(0.45f, 0.18f, 0.18f), 0.35f);
        _originH = Sprite("pixel", Color.white, 0.9f);
        _originV = Sprite("pixel", Color.white, 0.9f);
        _slots = new FSprite[InventorySlots.Count];
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            _slots[i] = Sprite("pixel", InventoryLayoutPresets.SlotColor((SlotId)i, enabled: true, selected: false), 1f);
        }
    }

    public override void Update()
    {
        base.Update();
        RefreshMetrics();
        var local = MousePos;
        var over = Inside(local);
        if (Input.GetMouseButtonDown(0) && over && HitsHud(local))
        {
            _maybeDrag = true;
            _pressLocal = local;
            _pressPercent = _owner.EffectiveOriginPercent();
        }

        if (_maybeDrag && Input.GetMouseButton(0))
        {
            if (!_drag && Vector2.Distance(local, _pressLocal) >= DragSlop)
            {
                _drag = true;
            }

            if (_drag)
            {
                held = true;
                var delta = ToScreen(local) - ToScreen(_pressLocal);
                var min = _safe;
                var max = _screen - _safe;
                var span = max - min;
                var percent = _pressPercent;
                if (span.x > 0.01f)
                {
                    percent.x += delta.x / span.x * 100f;
                }

                if (span.y > 0.01f)
                {
                    percent.y += delta.y / span.y * 100f;
                }

                _owner.WriteOriginPercent(percent);
            }
        }

        if (!Input.GetMouseButton(0))
        {
            _maybeDrag = false;
            _drag = false;
            held = false;
        }
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        RefreshMetrics();
        PlaceRect(_screenFill, _screenOrigin, _screen * _scale);
        var safePos = _screenOrigin + _safe * _scale;
        var safeSize = (_screen - _safe * 2f) * _scale;
        if (safeSize.x < 1f || safeSize.y < 1f)
        {
            safeSize = _screen * _scale;
            safePos = _screenOrigin;
        }

        PlaceRect(_safeFill, safePos, safeSize);
        var nativeH = (InventoryLayoutPresets.NativeBandPx + MoreSlugHUDConfig.BottomPulseExtent) * _scale;
        PlaceRect(_nativeFill, _screenOrigin, new Vector2(_screen.x * _scale, nativeH));
        var originPercent = _owner.EffectiveOriginPercent();
        var origin = InventoryLayoutPresets.Origin(_screen, _safe, originPercent.x, originPercent.y);
        var originCanvas = ToCanvas(origin);
        _originH.x = originCanvas.x;
        _originH.y = originCanvas.y;
        _originH.scaleX = 14f;
        _originH.scaleY = 2f;
        _originV.x = originCanvas.x;
        _originV.y = originCanvas.y;
        _originV.scaleX = 2f;
        _originV.scaleY = 14f;
        var box = MoreSlugHUDConfig.SlotSize * _scale;
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var id = (SlotId)i;
            var enabled = _owner.SlotEnabled(id);
            _slots[i].color = InventoryLayoutPresets.SlotColor(id, enabled, selected: false);
            _slots[i].alpha = enabled ? 1f : 0.35f;
            var center = ToCanvas(origin + _owner.EffectiveSlotLocal(id));
            _slots[i].x = center.x;
            _slots[i].y = center.y;
            _slots[i].scaleX = box;
            _slots[i].scaleY = box;
        }
    }

    private bool HitsHud(Vector2 local)
    {
        var originPercent = _owner.EffectiveOriginPercent();
        var origin = InventoryLayoutPresets.Origin(_screen, _safe, originPercent.x, originPercent.y);
        if (Vector2.Distance(local, ToCanvas(origin)) <= 12f)
        {
            return true;
        }

        var reach = Mathf.Max(10f, MoreSlugHUDConfig.SlotSize * _scale * 0.6f);
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var id = (SlotId)i;
            var center = ToCanvas(origin + _owner.EffectiveSlotLocal(id));
            if (Vector2.Distance(local, center) <= reach)
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshMetrics()
    {
        InventoryLayoutPresets.ResolveScreen(out _screen, out _safe);
        var avail = size - new Vector2(Pad * 2f, Pad * 2f);
        _scale = Mathf.Min(avail.x / Mathf.Max(1f, _screen.x), avail.y / Mathf.Max(1f, _screen.y));
        var drawn = _screen * _scale;
        _screenOrigin = new Vector2(Pad + (avail.x - drawn.x) * 0.5f, Pad + (avail.y - drawn.y) * 0.5f);
    }

    private Vector2 ToCanvas(Vector2 screenPos) => _screenOrigin + screenPos * _scale;

    private Vector2 ToScreen(Vector2 canvasLocal) => (canvasLocal - _screenOrigin) / Mathf.Max(0.0001f, _scale);

    private bool Inside(Vector2 local) =>
        local.x >= 0f && local.y >= 0f && local.x <= size.x && local.y <= size.y;

    private FSprite Sprite(string name, Color color, float alpha)
    {
        var sprite = new FSprite(name)
        {
            color = color,
            alpha = alpha,
            anchorX = 0.5f,
            anchorY = 0.5f,
        };
        myContainer.AddChild(sprite);
        return sprite;
    }

    private static void PlaceRect(FSprite sprite, Vector2 bottomLeft, Vector2 rectSize)
    {
        sprite.anchorX = 0f;
        sprite.anchorY = 0f;
        sprite.x = bottomLeft.x;
        sprite.y = bottomLeft.y;
        sprite.scaleX = Mathf.Max(1f, rectSize.x);
        sprite.scaleY = Mathf.Max(1f, rectSize.y);
    }
}
