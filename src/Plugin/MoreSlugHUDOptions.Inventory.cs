using System;
using System.Collections.Generic;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

public sealed partial class MoreSlugHUDOptions
{
    public Configurable<bool> Enabled = null!;
    public Configurable<bool> ShowLeft = null!;
    public Configurable<bool> ShowRight = null!;
    public Configurable<bool> ShowStomach = null!;
    public Configurable<bool> ShowBack = null!;
    public Configurable<bool> ShowCraft = null!;
    public Configurable<bool> ShowPyro = null!;
    public Configurable<bool> ShowPickUpCandidate = null!;
    public Configurable<string> Layout = null!;
    public Configurable<string> Position = null!;
    public Configurable<float> CustomX = null!;
    public Configurable<float> CustomY = null!;
    public Configurable<float> SlotCraftX = null!;
    public Configurable<float> SlotCraftY = null!;
    public Configurable<float> SlotLeftX = null!;
    public Configurable<float> SlotLeftY = null!;
    public Configurable<float> SlotRightX = null!;
    public Configurable<float> SlotRightY = null!;
    public Configurable<float> SlotStomachX = null!;
    public Configurable<float> SlotStomachY = null!;
    public Configurable<float> SlotBackX = null!;
    public Configurable<float> SlotBackY = null!;
    public Configurable<float> SlotPyroX = null!;
    public Configurable<float> SlotPyroY = null!;
    public Configurable<string> EditorSlot = null!;

    private OpComboBox? _positionBox;
    private OpComboBox? _layoutBox;
    private OpComboBox? _slotBox;
    private OpLabel? _warningLabel;
    private OpLabel? _slotShowLabel;
    private OpUpdown? _originX;
    private OpUpdown? _originY;
    private InventoryLayoutCanvas? _layoutCanvas;
    private readonly OpUpdown[] _slotXs = new OpUpdown[InventorySlots.Count];
    private readonly OpUpdown[] _slotYs = new OpUpdown[InventorySlots.Count];
    private readonly OpCheckBox?[] _slotChecks = new OpCheckBox?[InventorySlots.Count];
    private SlotId _selectedSlot = SlotId.Left;
    private bool _syncingPreset;
    private bool _syncingSlotUi;

    private void BindInventory()
    {
        var localRange = new ConfigAcceptableRange<float>(
            InventoryLayoutPresets.LocalMin,
            InventoryLayoutPresets.LocalMax);
        Enabled = config.Bind("Enabled", true);
        ShowLeft = config.Bind("ShowLeft", true);
        ShowRight = config.Bind("ShowRight", true);
        ShowStomach = config.Bind("ShowStomach", true);
        ShowBack = config.Bind("ShowBack", true);
        ShowCraft = config.Bind("ShowCraft", true);
        ShowPyro = config.Bind("ShowPyro", true);
        ShowPickUpCandidate = config.Bind("ShowPickUpCandidate", true);
        Layout = config.Bind("Layout", "row", new ConfigAcceptableList<string>("row", "cross", "custom"));
        Position = config.Bind("Position", "bottomCenter", new ConfigAcceptableList<string>(
            "bottomCenter", "topCenter", "topLeft", "topRight", "bottomRight", "custom"));
        CustomX = config.Bind("CustomX", 50f, new ConfigAcceptableRange<float>(0f, 100f));
        CustomY = config.Bind("CustomY", 30f, new ConfigAcceptableRange<float>(0f, 100f));
        BindSlotLocal(SlotId.Left, localRange, out SlotLeftX, out SlotLeftY);
        BindSlotLocal(SlotId.Right, localRange, out SlotRightX, out SlotRightY);
        BindSlotLocal(SlotId.Stomach, localRange, out SlotStomachX, out SlotStomachY);
        BindSlotLocal(SlotId.Back, localRange, out SlotBackX, out SlotBackY);
        BindSlotLocal(SlotId.Craft, localRange, out SlotCraftX, out SlotCraftY);
        BindSlotLocal(SlotId.Pyro, localRange, out SlotPyroX, out SlotPyroY);
        EditorSlot = config.Bind(
            "EditorSlot",
            nameof(SlotId.Left),
            new ConfigAcceptableList<string>(
                nameof(SlotId.Left),
                nameof(SlotId.Right),
                nameof(SlotId.Stomach),
                nameof(SlotId.Back),
                nameof(SlotId.Craft),
                nameof(SlotId.Pyro)));
    }

    private void BindSlotLocal(
        SlotId id,
        ConfigAcceptableRange<float> localRange,
        out Configurable<float> x,
        out Configurable<float> y)
    {
        var local = InventoryLayoutPresets.Row(id);
        x = config.Bind($"Slot{id}X", local.x, localRange);
        y = config.Bind($"Slot{id}Y", local.y, localRange);
    }

    private void ApplyInventory()
    {
        MoreSlugHUDConfig.Enabled = Enabled.Value;
        MoreSlugHUDConfig.ShowLeft = ShowLeft.Value;
        MoreSlugHUDConfig.ShowRight = ShowRight.Value;
        MoreSlugHUDConfig.ShowStomach = ShowStomach.Value;
        MoreSlugHUDConfig.ShowBack = ShowBack.Value;
        MoreSlugHUDConfig.ShowCraft = ShowCraft.Value;
        MoreSlugHUDConfig.ShowPyro = ShowPyro.Value;
        MoreSlugHUDConfig.ShowPickUpCandidate = ShowPickUpCandidate.Value;
        var origin = AppliedOriginPercent();
        MoreSlugHUDConfig.CustomX = origin.x;
        MoreSlugHUDConfig.CustomY = origin.y;
        foreach (var id in SlotIds)
        {
            MoreSlugHUDConfig.SetSlotLocal(id, AppliedSlotLocal(id));
        }
    }

    private void MigrateSaveTo1()
    {
        var origin = InventoryLayoutPresets.OriginPercent(
            MoreSlugHUDConfig.ParsePosition(Position.Value),
            CustomX.Value,
            CustomY.Value);
        CustomX.Value = origin.x;
        CustomY.Value = origin.y;
        var layout = MoreSlugHUDConfig.ParseLayout(Layout.Value);
        if (layout != LayoutStyle.Custom)
        {
            foreach (var id in SlotIds)
            {
                var local = InventoryLayoutPresets.Local(id, layout);
                SlotX(id).Value = local.x;
                SlotY(id).Value = local.y;
            }
        }
    }

    private OpTab BuildInventoryTab()
    {
        var tab = new OpTab(this, L(LocKeys.OptionsTabInventory));
        var page = new SettingsPageBuilder();
        var labelX = SettingsPageBuilder.LabelX;
        const float rightX = 310f;
        const float canvasW = 250f;
        const float canvasH = 160f;
        const float leftW = 250f;
        const float titleW = 90f;
        const float presetW = leftW - titleW;
        const float indent = 16f;
        const float comboH = 24f;
        const float row = 36f;

        var enabled = new OpCheckBox(Enabled, new Vector2(labelX, page.Y));
        var enabledLabel = new OpLabel(75f, page.Y + 3f, L(LocKeys.OptionsEnabled));
        Hint(enabled, enabledLabel, LocKeys.OptionsEnabledHint);
        page.Add(enabled, enabledLabel);
        page.Advance(38f);
        _inventoryToggleHint = page.Wrapped(520f, ToggleHintText(MoreSlugHUDPlugin.ToggleKeybind));
        page.Advance(36f);

        var positionTop = page.Y;
        var positionHeader = new OpLabel(labelX, positionTop + 5f, L(LocKeys.OptionsPosition))
        {
            color = MenuColorEffect.rgbWhite,
        };
        _positionBox = RaiseCombo(new OpComboBox(
            Position,
            new Vector2(labelX + titleW, positionTop),
            presetW,
            new List<ListItem>
            {
                Item("bottomCenter", LocKeys.OptionsPositionBottomCenter, 0),
                Item("topCenter", LocKeys.OptionsPositionTopCenter, 1),
                Item("topLeft", LocKeys.OptionsPositionTopLeft, 2),
                Item("topRight", LocKeys.OptionsPositionTopRight, 3),
                Item("bottomRight", LocKeys.OptionsPositionBottomRight, 4),
                Item("custom", LocKeys.OptionsPositionCustom, 5),
            })
        {
            listHeight = 6,
        });
        Hint(_positionBox, positionHeader, LocKeys.OptionsPositionHint);
        _positionBox.OnValueChanged += (_, _, _) => OnPositionChanged();
        page.Add(positionHeader);
        page.Add(_positionBox);
        page.Advance(row);

        _originX = new OpUpdown(CustomX, new Vector2(labelX, page.Y), 80f, 1);
        var originXLabel = new OpLabel(labelX + 84f, page.Y + 5f, "X %");
        Hint(_originX, originXLabel, LocKeys.OptionsCustomXHint);
        _originX.OnValueUpdate += (_, _, _) => OnOriginEdited();
        _originX.OnValueChanged += (_, _, _) => OnOriginEdited();
        page.Add(_originX);
        page.Add(originXLabel);
        page.Advance(row);

        _originY = new OpUpdown(CustomY, new Vector2(labelX, page.Y), 80f, 1);
        var originYLabel = new OpLabel(labelX + 84f, page.Y + 5f, "Y %");
        Hint(_originY, originYLabel, LocKeys.OptionsCustomYHint);
        _originY.OnValueUpdate += (_, _, _) => OnOriginEdited();
        _originY.OnValueChanged += (_, _, _) => OnOriginEdited();
        page.Add(_originY);
        page.Add(originYLabel);
        page.Advance(row);

        _warningLabel = new OpLabel(
            new Vector2(labelX, page.Y),
            new Vector2(250f, 36f),
            string.Empty,
            FLabelAlignment.Left)
        {
            autoWrap = true,
            color = new Color(0.95f, 0.75f, 0.35f),
        };
        page.Add(_warningLabel);
        page.Advance(36f);

        page.Add(new InventoryPositionCanvas(
            new Vector2(rightX, positionTop + comboH - canvasH),
            new Vector2(canvasW, canvasH),
            this));
        FinishSection(page, positionTop + comboH - canvasH);

        var styleTop = page.Y;
        var styleHeader = new OpLabel(labelX, styleTop + 5f, L(LocKeys.OptionsStyle))
        {
            color = MenuColorEffect.rgbWhite,
        };
        _layoutBox = RaiseCombo(new OpComboBox(
            Layout,
            new Vector2(labelX + titleW, styleTop),
            presetW,
            new List<ListItem>
            {
                Item("row", LocKeys.OptionsStyleRow, 0),
                Item("cross", LocKeys.OptionsStyleCross, 1),
                Item("custom", LocKeys.OptionsStyleCustom, 2),
            })
        {
            listHeight = 3,
        });
        Hint(_layoutBox, styleHeader, LocKeys.OptionsStyleHint);
        _layoutBox.OnValueChanged += (_, _, _) => OnLayoutChanged();
        page.Add(styleHeader);
        page.Add(_layoutBox);
        page.Advance(row);

        _slotBox = RaiseCombo(new OpComboBox(
            EditorSlot,
            new Vector2(labelX, page.Y),
            leftW,
            new List<ListItem>
            {
                Item(nameof(SlotId.Left), LocKeys.OptionsShowLeft, 1),
                Item(nameof(SlotId.Right), LocKeys.OptionsShowRight, 2),
                Item(nameof(SlotId.Stomach), LocKeys.OptionsShowStomach, 3),
                Item(nameof(SlotId.Back), LocKeys.OptionsShowBack, 4),
                Item(nameof(SlotId.Craft), LocKeys.OptionsShowCraft, 0),
                Item(nameof(SlotId.Pyro), LocKeys.OptionsShowPyro, 5, LocKeys.OptionsShowPyroHint),
            })
        {
            listHeight = 6,
        });
        _slotBox.OnValueChanged += (_, _, _) => OnEditorSlotChanged();
        page.Add(_slotBox);
        page.Advance(row);

        var slotX = labelX + indent;
        var slotXLabel = new OpLabel(slotX + 84f, page.Y + 5f, "X");
        page.Add(slotXLabel);
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            _slotXs[i] = new OpUpdown(SlotX((SlotId)i), new Vector2(slotX, page.Y), 80f, 0);
            _slotXs[i].OnValueUpdate += (_, _, _) => OnSlotEdited();
            _slotXs[i].OnValueChanged += (_, _, _) => OnSlotEdited();
            page.Add(_slotXs[i]);
        }

        page.Advance(row);
        var slotYLabel = new OpLabel(slotX + 84f, page.Y + 5f, "Y");
        page.Add(slotYLabel);
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            _slotYs[i] = new OpUpdown(SlotY((SlotId)i), new Vector2(slotX, page.Y), 80f, 0);
            _slotYs[i].OnValueUpdate += (_, _, _) => OnSlotEdited();
            _slotYs[i].OnValueChanged += (_, _, _) => OnSlotEdited();
            page.Add(_slotYs[i]);
        }

        page.Advance(row);
        _slotShowLabel = new OpLabel(slotX + 35f, page.Y + 3f, L(LocKeys.OptionsSlotShow));
        page.Add(_slotShowLabel);
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var id = (SlotId)i;
            var box = new OpCheckBox(ShowOf(id), new Vector2(slotX, page.Y));
            BindSlotCheck(id, box);
            page.Add(box);
        }

        page.Advance(row);
        _layoutCanvas = new InventoryLayoutCanvas(
            new Vector2(rightX, styleTop + comboH - canvasH),
            new Vector2(canvasW, canvasH),
            this);
        _layoutCanvas.SelectionChanged += ShowSelectedSlotFields;
        page.Add(_layoutCanvas);
        FinishSection(page, styleTop + comboH - canvasH);

        page.Section(L(LocKeys.OptionsSectionMisc));
        page.Advance(22f);
        var pickup = Check(
            ShowPickUpCandidate,
            labelX,
            page.Y,
            LocKeys.OptionsShowPickUpCandidate,
            out var pickupLabel);
        Hint(pickup, pickupLabel, LocKeys.OptionsShowPickUpCandidateHint);
        page.Add(pickup);
        page.Add(pickupLabel);

        tab.AddItems(page.Items.ToArray());
        ShowSelectedSlotFields(ParseSlot(EditorSlot.Value));
        RefreshInventoryLocks();
        return tab;
    }

    private void OnPositionChanged() => RefreshInventoryLocks();

    private void OnLayoutChanged() => RefreshInventoryLocks();

    private void OnEditorSlotChanged()
    {
        if (_syncingSlotUi)
        {
            return;
        }

        ShowSelectedSlotFields(ParseSlot(_slotBox?.value));
    }

    private static void FinishSection(SettingsPageBuilder page, float canvasY, float pad = 10f)
    {
        if (page.Y > canvasY)
        {
            page.Advance(page.Y - canvasY);
        }

        page.Advance(pad);
    }

    private void OnOriginEdited()
    {
        if (!_syncingPreset)
        {
            BeginCustomPosition(seedFromPreset: false);
        }

        RefreshInventoryLocks();
    }

    private void OnSlotEdited()
    {
        if (!_syncingPreset)
        {
            BeginCustomLayout(seedFromPreset: false);
        }

        RefreshInventoryLocks();
    }

    private void RefreshInventoryLocks()
    {
        if (_warningLabel != null)
        {
            var origin = EffectiveOriginPercent();
            _warningLabel.text = InventoryLayoutPresets.Warning(
                origin.x,
                origin.y,
                EffectiveSlotLocal,
                SlotEnabled,
                L);
        }

        var customPosition = IsCustomPosition;
        if (_originX != null)
        {
            _originX.greyedOut = !customPosition;
        }

        if (_originY != null)
        {
            _originY.greyedOut = !customPosition;
        }

        var customLayout = IsCustomLayout;
        for (var i = 0; i < InventorySlots.Count; i++)
        {
            if (_slotXs[i] != null)
            {
                _slotXs[i].greyedOut = !customLayout;
            }

            if (_slotYs[i] != null)
            {
                _slotYs[i].greyedOut = !customLayout;
            }
        }
    }

    private bool IsCustomPosition => (_positionBox?.value ?? Position.Value) == "custom";

    private bool IsCustomLayout => (_layoutBox?.value ?? Layout.Value) == "custom";

    internal Vector2 EffectiveOriginPercent() =>
        InventoryLayoutPresets.OriginPercent(PendingPosition(), PendingOriginX(), PendingOriginY());

    internal Vector2 EffectiveSlotLocal(SlotId id)
    {
        var layout = PendingLayout();
        return layout == LayoutStyle.Custom
            ? ReadSlotLocal(id)
            : InventoryLayoutPresets.Local(id, layout);
    }

    private Vector2 AppliedOriginPercent() =>
        InventoryLayoutPresets.OriginPercent(
            MoreSlugHUDConfig.ParsePosition(Position.Value),
            CustomX.Value,
            CustomY.Value);

    private Vector2 AppliedSlotLocal(SlotId id)
    {
        var layout = MoreSlugHUDConfig.ParseLayout(Layout.Value);
        return layout == LayoutStyle.Custom
            ? new Vector2(SlotX(id).Value, SlotY(id).Value)
            : InventoryLayoutPresets.Local(id, layout);
    }

    private PositionPreset PendingPosition() =>
        MoreSlugHUDConfig.ParsePosition(_positionBox?.value ?? Position.Value);

    private LayoutStyle PendingLayout() =>
        MoreSlugHUDConfig.ParseLayout(_layoutBox?.value ?? Layout.Value);

    private float PendingOriginX() => _originX?.valueFloat ?? CustomX.Value;

    private float PendingOriginY() => _originY?.valueFloat ?? CustomY.Value;

    internal void WriteOriginPercent(Vector2 percent)
    {
        BeginCustomPosition(seedFromPreset: true);
        SetOriginFields(new Vector2(
            Mathf.Clamp(Mathf.Round(percent.x * 10f) / 10f, 0f, 100f),
            Mathf.Clamp(Mathf.Round(percent.y * 10f) / 10f, 0f, 100f)), pending: true);
        RefreshInventoryLocks();
    }

    internal void WriteSlotLocal(SlotId id, Vector2 local)
    {
        BeginCustomLayout(seedFromPreset: true);
        local.x = Mathf.Round(Mathf.Clamp(local.x, InventoryLayoutPresets.LocalMin, InventoryLayoutPresets.LocalMax));
        local.y = Mathf.Round(Mathf.Clamp(local.y, InventoryLayoutPresets.LocalMin, InventoryLayoutPresets.LocalMax));
        SetSlotFields(id, local, pending: true);
        RefreshInventoryLocks();
    }

    private void BeginCustomPosition(bool seedFromPreset)
    {
        if (IsCustomPosition)
        {
            return;
        }

        _syncingPreset = true;
        if (seedFromPreset)
        {
            SetOriginFields(
                InventoryLayoutPresets.OriginPercent(PendingPosition(), 0f, 0f),
                pending: true);
        }

        SelectCombo(_positionBox, Position, "custom");
        _syncingPreset = false;
    }

    private void BeginCustomLayout(bool seedFromPreset)
    {
        if (IsCustomLayout)
        {
            return;
        }

        _syncingPreset = true;
        if (seedFromPreset)
        {
            var layout = PendingLayout();
            foreach (var id in SlotIds)
            {
                SetSlotFields(id, InventoryLayoutPresets.Local(id, layout), pending: true);
            }
        }

        SelectCombo(_layoutBox, Layout, "custom");
        _syncingPreset = false;
    }

    private void SetOriginFields(Vector2 origin, bool pending)
    {
        if (_originX != null && _originY != null)
        {
            SetUpdown(_originX, origin.x, pending, decimals: 1);
            SetUpdown(_originY, origin.y, pending, decimals: 1);
            return;
        }

        CustomX.Value = origin.x;
        CustomY.Value = origin.y;
    }

    private void SetSlotFields(SlotId id, Vector2 local, bool pending)
    {
        var i = (int)id;
        if (_slotXs[i] != null && _slotYs[i] != null)
        {
            SetUpdown(_slotXs[i], local.x, pending, decimals: 0);
            SetUpdown(_slotYs[i], local.y, pending, decimals: 0);
            return;
        }

        SlotX(id).Value = local.x;
        SlotY(id).Value = local.y;
    }

    private static void SetUpdown(OpUpdown field, float value, bool pending, int decimals)
    {
        var rounded = decimals == 0
            ? Mathf.Round(value)
            : Mathf.Round(value * 10f) / 10f;
        if (pending)
        {
            field.valueFloat = rounded;
            return;
        }

        var text = rounded.ToString(decimals == 0 ? "0" : "0.0", System.Globalization.CultureInfo.InvariantCulture);
        field.ForceValue(text);
        field.size = field.size;
    }

    private void BindSlotCheck(SlotId id, OpCheckBox box)
    {
        _slotChecks[(int)id] = box;
        box.OnValueUpdate += (_, _, _) => RefreshInventoryLocks();
        box.OnValueChanged += (_, _, _) => RefreshInventoryLocks();
    }

    internal Vector2 ReadSlotLocal(SlotId id)
    {
        var i = (int)id;
        if (_slotXs[i] != null && _slotYs[i] != null)
        {
            return new Vector2(_slotXs[i].valueFloat, _slotYs[i].valueFloat);
        }

        return new Vector2(SlotX(id).Value, SlotY(id).Value);
    }

    internal bool SlotEnabled(SlotId id)
    {
        var box = _slotChecks[(int)id];
        if (box != null)
        {
            return box.value != "false";
        }

        return id switch
        {
            SlotId.Left => ShowLeft.Value,
            SlotId.Right => ShowRight.Value,
            SlotId.Stomach => ShowStomach.Value,
            SlotId.Back => ShowBack.Value,
            SlotId.Craft => ShowCraft.Value,
            SlotId.Pyro => ShowPyro.Value,
            _ => false,
        };
    }

    private Configurable<bool> ShowOf(SlotId id) => id switch
    {
        SlotId.Left => ShowLeft,
        SlotId.Right => ShowRight,
        SlotId.Stomach => ShowStomach,
        SlotId.Back => ShowBack,
        SlotId.Craft => ShowCraft,
        _ => ShowPyro,
    };

    private Configurable<float> SlotX(SlotId id) => id switch
    {
        SlotId.Left => SlotLeftX,
        SlotId.Right => SlotRightX,
        SlotId.Stomach => SlotStomachX,
        SlotId.Back => SlotBackX,
        SlotId.Craft => SlotCraftX,
        _ => SlotPyroX,
    };

    private Configurable<float> SlotY(SlotId id) => id switch
    {
        SlotId.Left => SlotLeftY,
        SlotId.Right => SlotRightY,
        SlotId.Stomach => SlotStomachY,
        SlotId.Back => SlotBackY,
        SlotId.Craft => SlotCraftY,
        _ => SlotPyroY,
    };

    private void ShowSelectedSlotFields(SlotId id)
    {
        _selectedSlot = id;
        if (!_syncingSlotUi)
        {
            _syncingSlotUi = true;
            _layoutCanvas?.Select(id);
            var key = id.ToString();
            if (_slotBox != null && _slotBox.value != key)
            {
                _slotBox.value = key;
            }

            _syncingSlotUi = false;
        }

        for (var i = 0; i < InventorySlots.Count; i++)
        {
            var on = i == (int)id;
            SetVisible(_slotXs[i], on);
            SetVisible(_slotYs[i], on);
            SetVisible(_slotChecks[i], on);
        }

        var box = _slotChecks[(int)id];
        if (box != null)
        {
            var hint = id == SlotId.Pyro ? LocKeys.OptionsShowPyroHint : LocKeys.OptionsSlotShowHint;
            Hint(box, _slotShowLabel, hint);
        }
    }

    private static SlotId ParseSlot(string? value) =>
        Enum.TryParse(value, out SlotId id) && (int)id >= 0 && (int)id < InventorySlots.Count
            ? id
            : SlotId.Left;

    private static void SetVisible(UIelement? element, bool on)
    {
        if (element == null)
        {
            return;
        }

        if (on)
        {
            element.Show();
            return;
        }

        element.Hide();
    }

    private static void SelectCombo(OpComboBox? box, Configurable<string> config, string value)
    {
        if (box != null)
        {
            box.value = value;
            return;
        }

        config.Value = value;
    }

    private static readonly SlotId[] SlotIds = InventorySlots.All;
}
