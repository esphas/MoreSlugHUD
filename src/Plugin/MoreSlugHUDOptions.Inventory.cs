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
    public Configurable<string> Layout = null!;
    public Configurable<string> Position = null!;
    public Configurable<float> CustomX = null!;
    public Configurable<float> CustomY = null!;

    private OpComboBox? _positionBox;
    private OpLabel? _positionLabel;
    private OpUpdown? _customX;
    private OpUpdown? _customY;
    private OpLabel? _customXLabel;
    private OpLabel? _customYLabel;
    private OpLabel? _customXPercent;
    private OpLabel? _customYPercent;

    private void BindInventory()
    {
        Enabled = config.Bind("Enabled", true);
        ShowLeft = config.Bind("ShowLeft", true);
        ShowRight = config.Bind("ShowRight", true);
        ShowStomach = config.Bind("ShowStomach", true);
        ShowBack = config.Bind("ShowBack", true);
        ShowCraft = config.Bind("ShowCraft", true);
        Layout = config.Bind("Layout", "row", new ConfigAcceptableList<string>("row", "cross"));
        Position = config.Bind("Position", "bottomCenter", new ConfigAcceptableList<string>(
            "bottomCenter", "topCenter", "topLeft", "topRight", "bottomRight", "custom"));
        CustomX = config.Bind("CustomX", 10f, new ConfigAcceptableRange<float>(0f, 100f));
        CustomY = config.Bind("CustomY", 10f, new ConfigAcceptableRange<float>(0f, 100f));
    }

    private void ApplyInventory()
    {
        MoreSlugHUDConfig.Enabled = Enabled.Value;
        MoreSlugHUDConfig.ShowLeft = ShowLeft.Value;
        MoreSlugHUDConfig.ShowRight = ShowRight.Value;
        MoreSlugHUDConfig.ShowStomach = ShowStomach.Value;
        MoreSlugHUDConfig.ShowBack = ShowBack.Value;
        MoreSlugHUDConfig.ShowCraft = ShowCraft.Value;
        MoreSlugHUDConfig.Layout = MoreSlugHUDConfig.ParseLayout(Layout.Value);
        MoreSlugHUDConfig.Position = MoreSlugHUDConfig.ParsePosition(Position.Value);
        MoreSlugHUDConfig.CustomX = CustomX.Value;
        MoreSlugHUDConfig.CustomY = CustomY.Value;
    }

    private OpTab BuildInventoryTab()
    {
        var tab = new OpTab(this, L(LocKeys.OptionsTabInventory));
        const float labelX = 40f;
        const float controlX = 220f;
        const float col2 = 300f;
        const float row = 44f;
        const float checkRow = 34f;
        var items = new List<UIelement>();
        var y = 540f;

        var enabled = new OpCheckBox(Enabled, new Vector2(labelX, y));
        var enabledLabel = new OpLabel(75f, y + 3f, L(LocKeys.OptionsEnabled));
        Hint(enabled, enabledLabel, LocKeys.OptionsEnabledHint);
        items.Add(enabled);
        items.Add(enabledLabel);
        y -= 38f;
        items.Add(Wrapped(labelX, y, 520f, L(LocKeys.OptionsToggleHint)));
        y -= 52f;

        items.Add(Section(labelX, y, LocKeys.OptionsSectionLayout));
        y -= 34f;
        var layoutBox = RaiseCombo(new OpComboBox(
            Layout,
            new Vector2(controlX, y),
            180f,
            new List<ListItem>
            {
                Item("row", LocKeys.OptionsStyleRow, 0, LocKeys.OptionsStyleRowHint),
                Item("cross", LocKeys.OptionsStyleCross, 1, LocKeys.OptionsStyleCrossHint),
            })
        {
            listHeight = 2,
        });
        var layoutLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsStyle));
        items.Add(layoutBox);
        items.Add(layoutLabel);
        y -= row;
        _positionBox = RaiseCombo(new OpComboBox(
            Position,
            new Vector2(controlX, y),
            180f,
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
        _positionLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsPosition));
        Hint(_positionBox, _positionLabel, LocKeys.OptionsPositionHint);
        _positionBox.OnValueChanged += (_, _, _) => RefreshLocks();
        items.Add(_positionBox);
        items.Add(_positionLabel);
        y -= row;
        _customX = new OpUpdown(CustomX, new Vector2(controlX, y), 100f, 1);
        _customXLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsCustomX));
        _customXPercent = PercentLabel(controlX + 108f, y + 5f);
        Hint(_customX, _customXLabel, LocKeys.OptionsCustomXHint);
        _customXPercent.description = _customX.description;
        items.Add(_customX);
        items.Add(_customXLabel);
        items.Add(_customXPercent);
        y -= row;
        _customY = new OpUpdown(CustomY, new Vector2(controlX, y), 100f, 1);
        _customYLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsCustomY));
        _customYPercent = PercentLabel(controlX + 108f, y + 5f);
        Hint(_customY, _customYLabel, LocKeys.OptionsCustomYHint);
        _customYPercent.description = _customY.description;
        items.Add(_customY);
        items.Add(_customYLabel);
        items.Add(_customYPercent);
        y -= 52f;

        items.Add(Section(labelX, y, LocKeys.OptionsSectionSlots));
        y -= 34f;
        var left = Check(ShowLeft, labelX, y, LocKeys.OptionsShowLeft, out var leftLabel);
        var right = Check(ShowRight, col2, y, LocKeys.OptionsShowRight, out var rightLabel);
        y -= checkRow;
        var stomach = Check(ShowStomach, labelX, y, LocKeys.OptionsShowStomach, out var stomachLabel);
        var back = Check(ShowBack, col2, y, LocKeys.OptionsShowBack, out var backLabel);
        y -= checkRow;
        var craft = Check(ShowCraft, labelX, y, LocKeys.OptionsShowCraft, out var craftLabel);
        items.Add(left);
        items.Add(right);
        items.Add(stomach);
        items.Add(back);
        items.Add(craft);
        items.Add(leftLabel);
        items.Add(rightLabel);
        items.Add(stomachLabel);
        items.Add(backLabel);
        items.Add(craftLabel);

        tab.AddItems(items.ToArray());
        return tab;
    }

    private void RefreshInventoryLocks()
    {
        var custom = (_positionBox?.value ?? Position.Value) == "custom";
        if (_customX != null)
        {
            _customX.greyedOut = !custom;
        }

        if (_customY != null)
        {
            _customY.greyedOut = !custom;
        }

        var customColor = custom ? MenuColorEffect.rgbMediumGrey : MenuColorEffect.rgbDarkGrey;
        SetLabelColor(_positionLabel, MenuColorEffect.rgbMediumGrey);
        SetLabelColor(_customXLabel, customColor);
        SetLabelColor(_customYLabel, customColor);
        SetLabelColor(_customXPercent, customColor);
        SetLabelColor(_customYPercent, customColor);
    }

    private static OpLabel Section(float x, float y, string key) =>
        new(x, y, L(key))
        {
            color = MenuColorEffect.rgbWhite,
        };

    private static OpLabel PercentLabel(float x, float y) => new(x, y, "%");
}
