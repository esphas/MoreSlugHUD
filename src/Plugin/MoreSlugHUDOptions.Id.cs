using System.Collections.Generic;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

public sealed partial class MoreSlugHUDOptions
{
    public Configurable<bool> IdEnabled = null!;
    public Configurable<bool> ShowId = null!;
    public Configurable<bool> ShowName = null!;
    public Configurable<bool> ShowIntent = null!;
    public Configurable<bool> ShowLabelBackground = null!;
    public Configurable<string> IdArrange = null!;
    public Configurable<int> FamiliarityThreshold = null!;
    public Configurable<bool> IdStrictWatch = null!;
    public Configurable<int> IdWatchSeconds = null!;
    public Configurable<bool> ShowDead = null!;

    private readonly List<UIelement> _idLockables = new();
    private readonly List<OpLabel> _idLockLabels = new();

    private void BindId()
    {
        IdEnabled = config.Bind("IdEnabled", true);
        ShowId = config.Bind("ShowId", true);
        ShowName = config.Bind("ShowName", true);
        ShowIntent = config.Bind("ShowIntent", false);
        ShowLabelBackground = config.Bind("ShowLabelBackground", false);
        IdArrange = config.Bind("IdArrange", "row", new ConfigAcceptableList<string>("row", "stack", "cycle"));
        FamiliarityThreshold = config.Bind("FamiliarityThreshold", 50, new ConfigAcceptableRange<int>(0, 100));
        IdStrictWatch = config.Bind("IdStrictWatch", true);
        IdWatchSeconds = config.Bind("IdWatchSeconds", 20, new ConfigAcceptableRange<int>(0, 120));
        ShowDead = config.Bind("ShowDead", true);
    }

    private void ApplyId()
    {
        MoreSlugHUDConfig.IdEnabled = IdEnabled.Value;
        MoreSlugHUDConfig.ShowId = ShowId.Value;
        MoreSlugHUDConfig.ShowName = ShowName.Value;
        MoreSlugHUDConfig.ShowIntent = ShowIntent.Value;
        MoreSlugHUDConfig.ShowLabelBackground = ShowLabelBackground.Value;
        MoreSlugHUDConfig.IdArrange = MoreSlugHUDConfig.ParseIdArrange(IdArrange.Value);
        MoreSlugHUDConfig.FamiliarityThreshold = FamiliarityThreshold.Value / 100f;
        MoreSlugHUDConfig.IdStrictWatch = IdStrictWatch.Value;
        MoreSlugHUDConfig.IdWatchSeconds = IdWatchSeconds.Value;
        MoreSlugHUDConfig.ShowDead = ShowDead.Value;
    }

    private OpTab BuildIdTab()
    {
        _idLockables.Clear();
        _idLockLabels.Clear();
        var tab = new OpTab(this, L(LocKeys.OptionsTabId));
        var page = new SettingsPageBuilder();
        var labelX = SettingsPageBuilder.LabelX;
        var enabled = new OpCheckBox(IdEnabled, new Vector2(labelX, page.Y));
        var enabledLabel = new OpLabel(75f, page.Y + 3f, L(LocKeys.OptionsIdEnabled));
        Hint(enabled, enabledLabel, LocKeys.OptionsIdEnabledHint);
        enabled.OnValueChanged += (_, _, _) => RefreshLocks();
        page.Add(enabled, enabledLabel);
        page.Advance(SettingsPageBuilder.Row);
        _idToggleHint = page.Wrapped(520f, ToggleHintText(MoreSlugHUDPlugin.ToggleIdKeybind));
        page.Advance(SettingsPageBuilder.Row);
        var showId = Check(ShowId, labelX, page.Y, LocKeys.OptionsShowId, out var showIdLabel);
        var showName = Check(ShowName, page.Col2, page.Y, LocKeys.OptionsShowName, out var showNameLabel);
        Hint(showName, showNameLabel, LocKeys.OptionsShowNameHint);
        LockId(page.Items, showId, showIdLabel);
        LockId(page.Items, showName, showNameLabel);
        page.Advance(SettingsPageBuilder.Row);
        var showIntent = Check(ShowIntent, labelX, page.Y, LocKeys.OptionsShowIntent, out var showIntentLabel);
        Hint(showIntent, showIntentLabel, LocKeys.OptionsShowIntentHint);
        var showBackground = Check(ShowLabelBackground, page.Col2, page.Y, LocKeys.OptionsShowLabelBackground, out var showBackgroundLabel);
        Hint(showBackground, showBackgroundLabel, LocKeys.OptionsShowLabelBackgroundHint);
        LockId(page.Items, showIntent, showIntentLabel);
        LockId(page.Items, showBackground, showBackgroundLabel);
        page.Advance(SettingsPageBuilder.Row);
        var arrange = RaiseCombo(new OpComboBox(
            IdArrange,
            new Vector2(page.ControlX, page.Y),
            180f,
            new List<ListItem>
            {
                Item("row", LocKeys.OptionsIdArrangeRow, 0, LocKeys.OptionsIdArrangeRowHint),
                Item("stack", LocKeys.OptionsIdArrangeStack, 1, LocKeys.OptionsIdArrangeStackHint),
                Item("cycle", LocKeys.OptionsIdArrangeCycle, 2, LocKeys.OptionsIdArrangeCycleHint),
            })
        {
            listHeight = 3,
        });
        var arrangeLabel = new OpLabel(labelX, page.Y + 5f, L(LocKeys.OptionsIdArrange));
        Hint(arrange, arrangeLabel, LocKeys.OptionsIdArrangeHint);
        LockId(page.Items, arrange, arrangeLabel);
        page.Advance(SettingsPageBuilder.Row * 2f);
        var familiarity = new OpSlider(FamiliarityThreshold, new Vector2(labelX, page.Y), 520);
        var familiarityLabel = new OpLabel(labelX, page.Y + 34f, L(LocKeys.OptionsFamiliarity));
        Hint(familiarity, familiarityLabel, LocKeys.OptionsFamiliarityHint);
        LockId(page.Items, familiarity, familiarityLabel);
        page.Advance(SettingsPageBuilder.Row);
        var strictWatch = Check(IdStrictWatch, labelX, page.Y, LocKeys.OptionsIdStrictWatch, out var strictWatchLabel);
        Hint(strictWatch, strictWatchLabel, LocKeys.OptionsIdStrictWatchHint);
        LockId(page.Items, strictWatch, strictWatchLabel);
        page.Advance(SettingsPageBuilder.Row);
        var watchSeconds = new OpUpdown(IdWatchSeconds, new Vector2(page.ControlX, page.Y), 80f);
        var watchSecondsLabel = new OpLabel(labelX, page.Y + 5f, L(LocKeys.OptionsIdWatchSeconds));
        Hint(watchSeconds, watchSecondsLabel, LocKeys.OptionsIdWatchSecondsHint);
        LockId(page.Items, watchSeconds, watchSecondsLabel);
        page.Advance(SettingsPageBuilder.Row);
        var showDead = Check(ShowDead, labelX, page.Y, LocKeys.OptionsShowDead, out var showDeadLabel);
        LockId(page.Items, showDead, showDeadLabel);
        tab.AddItems(page.Items.ToArray());
        return tab;
    }

    private void RefreshIdLocks()
    {
        var on = IdEnabled.Value;
        var text = on ? MenuColorEffect.rgbMediumGrey : MenuColorEffect.rgbDarkGrey;
        for (var i = 0; i < _idLockables.Count; i++)
        {
            if (_idLockables[i] is UIfocusable focusable)
            {
                focusable.greyedOut = !on;
            }
        }

        for (var i = 0; i < _idLockLabels.Count; i++)
        {
            _idLockLabels[i].color = text;
        }
    }

    private void LockId(List<UIelement> items, UIelement control, OpLabel label)
    {
        items.Add(control);
        items.Add(label);
        _idLockables.Add(control);
        _idLockLabels.Add(label);
    }
}
