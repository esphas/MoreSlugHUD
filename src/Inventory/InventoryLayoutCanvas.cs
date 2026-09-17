using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal sealed class InventoryLayoutCanvas : UIfocusable
{
    private const float DragSlop = 4f;
    private const float Scale = 0.7f;

    private readonly MoreSlugHUDOptions _owner;
    private readonly FSprite _originH;
    private readonly FSprite _originV;
    private readonly FSprite[] _slots;
    private readonly FLabel[] _letters;
    private SlotId? _drag;
    private Vector2 _dragGrab;
    private bool _maybeDrag;
    private Vector2 _pressLocal;

    internal SlotId Selected { get; private set; } = SlotId.Left;

    internal event System.Action<SlotId>? SelectionChanged;

    internal InventoryLayoutCanvas(Vector2 pos, Vector2 size, MoreSlugHUDOptions owner) : base(pos, size)
    {
        _owner = owner;
        description = OptionInterface.Translate(LocKeys.OptionsLayoutCanvasHint);
        var back = Sprite("pixel", new Color(0.08f, 0.08f, 0.1f), 0.95f);
        back.anchorX = 0f;
        back.anchorY = 0f;
        back.scaleX = size.x;
        back.scaleY = size.y;
        _originH = Sprite("pixel", new Color(0.7f, 0.7f, 0.7f), 0.9f);
        _originV = Sprite("pixel", new Color(0.7f, 0.7f, 0.7f), 0.9f);
        _slots = new FSprite[InventorySlots.Count];
        _letters = new FLabel[InventorySlots.Count];
        var letters = new[] { "C", "L", "R", "S", "B", "H" };
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            _slots[i] = Sprite("pixel", InventoryLayoutPresets.SlotColor((SlotId)i, enabled: true, selected: false), 1f);
            _letters[i] = new FLabel(Custom.GetDisplayFont(), letters[i])
            {
                scale = 0.6f,
                alignment = FLabelAlignment.Center,
            };
            myContainer.AddChild(_letters[i]);
        }
    }

    public override void Update()
    {
        base.Update();
        var local = MousePos;
        var over = Inside(local);
        if (Input.GetMouseButtonDown(0) && over)
        {
            if (TryHitSlot(local, out var id))
            {
                Select(id);
                _maybeDrag = true;
                _pressLocal = local;
                _dragGrab = local - SlotCanvasCenter(id);
            }
        }

        if (_maybeDrag && Input.GetMouseButton(0))
        {
            if (!_drag.HasValue && Vector2.Distance(local, _pressLocal) >= DragSlop)
            {
                _drag = Selected;
            }

            if (_drag.HasValue)
            {
                held = true;
                _owner.WriteSlotLocal(_drag.Value, ToLocal(local - _dragGrab));
            }
        }

        if (!Input.GetMouseButton(0))
        {
            _maybeDrag = false;
            _drag = null;
            held = false;
        }
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        var origin = OriginCanvas();
        _originH.x = origin.x;
        _originH.y = origin.y;
        _originH.scaleX = 18f;
        _originH.scaleY = 2f;
        _originV.x = origin.x;
        _originV.y = origin.y;
        _originV.scaleX = 2f;
        _originV.scaleY = 18f;
        var box = MoreSlugHUDConfig.SlotSize * Scale;
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var id = (SlotId)i;
            var enabled = _owner.SlotEnabled(id);
            var selected = id == Selected;
            _slots[i].color = InventoryLayoutPresets.SlotColor(id, enabled, selected);
            _slots[i].alpha = enabled ? 1f : 0.35f;
            var center = SlotCanvasCenter(id);
            _slots[i].x = center.x;
            _slots[i].y = center.y;
            _slots[i].scaleX = box;
            _slots[i].scaleY = box;
            _letters[i].x = center.x;
            _letters[i].y = center.y;
            _letters[i].alpha = enabled ? 1f : 0.4f;
            _letters[i].color = selected ? Color.white : new Color(0.85f, 0.85f, 0.85f);
        }
    }

    internal void Select(SlotId id)
    {
        if (Selected == id)
        {
            return;
        }

        Selected = id;
        SelectionChanged?.Invoke(id);
    }

    private Vector2 OriginCanvas() => size * 0.5f;

    private Vector2 SlotCanvasCenter(SlotId id) => OriginCanvas() + _owner.EffectiveSlotLocal(id) * Scale;

    private Vector2 ToLocal(Vector2 canvas) => (canvas - OriginCanvas()) / Scale;

    private bool TryHitSlot(Vector2 local, out SlotId id)
    {
        id = SlotId.Left;
        var best = float.MaxValue;
        var hit = false;
        var reach = Mathf.Max(10f, MoreSlugHUDConfig.SlotSize * Scale * 0.6f);
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var candidate = (SlotId)i;
            var dist = Vector2.Distance(local, SlotCanvasCenter(candidate));
            if (dist <= reach && dist < best)
            {
                best = dist;
                id = candidate;
                hit = true;
            }
        }

        return hit;
    }

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
}
