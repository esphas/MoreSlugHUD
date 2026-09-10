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
        const float labelX = 40f;
        var items = new List<UIelement>();
        var y = 540f;
        var enabled = new OpCheckBox(IdEnabled, new Vector2(labelX, y));
        var enabledLabel = new OpLabel(75f, y + 3f, L(LocKeys.OptionsIdEnabled));
        Hint(enabled, enabledLabel, LocKeys.OptionsIdEnabledHint);
        enabled.OnValueChanged += (_, _, _) => RefreshLocks();
        items.Add(enabled);
        items.Add(enabledLabel);
        y -= 38f;
        items.Add(Wrapped(labelX, y, 520f, L(LocKeys.OptionsIdToggleHint)));
        y -= 52f;
        var showId = Check(ShowId, labelX, y, LocKeys.OptionsShowId, out var showIdLabel);
        var showName = Check(ShowName, 300f, y, LocKeys.OptionsShowName, out var showNameLabel);
        Hint(showName, showNameLabel, LocKeys.OptionsShowNameHint);
        LockId(items, showId, showIdLabel);
        LockId(items, showName, showNameLabel);
        y -= 34f;
        var showIntent = Check(ShowIntent, labelX, y, LocKeys.OptionsShowIntent, out var showIntentLabel);
        Hint(showIntent, showIntentLabel, LocKeys.OptionsShowIntentHint);
        var showBackground = Check(ShowLabelBackground, 300f, y, LocKeys.OptionsShowLabelBackground, out var showBackgroundLabel);
        Hint(showBackground, showBackgroundLabel, LocKeys.OptionsShowLabelBackgroundHint);
        LockId(items, showIntent, showIntentLabel);
        LockId(items, showBackground, showBackgroundLabel);
        y -= 44f;
        const float controlX = 220f;
        var arrange = RaiseCombo(new OpComboBox(
            IdArrange,
            new Vector2(controlX, y),
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
        var arrangeLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsIdArrange));
        Hint(arrange, arrangeLabel, LocKeys.OptionsIdArrangeHint);
        LockId(items, arrange, arrangeLabel);
        y -= 72f;
        var familiarity = new OpSlider(FamiliarityThreshold, new Vector2(labelX, y), 520);
        var familiarityLabel = new OpLabel(labelX, y + 34f, L(LocKeys.OptionsFamiliarity));
        Hint(familiarity, familiarityLabel, LocKeys.OptionsFamiliarityHint);
        LockId(items, familiarity, familiarityLabel);
        y -= 44f;
        var strictWatch = Check(IdStrictWatch, labelX, y, LocKeys.OptionsIdStrictWatch, out var strictWatchLabel);
        Hint(strictWatch, strictWatchLabel, LocKeys.OptionsIdStrictWatchHint);
        LockId(items, strictWatch, strictWatchLabel);
        y -= 38f;
        var watchSeconds = new OpUpdown(IdWatchSeconds, new Vector2(controlX, y), 80f);
        var watchSecondsLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsIdWatchSeconds));
        Hint(watchSeconds, watchSecondsLabel, LocKeys.OptionsIdWatchSecondsHint);
        LockId(items, watchSeconds, watchSecondsLabel);
        y -= 40f;
        var showDead = Check(ShowDead, labelX, y, LocKeys.OptionsShowDead, out var showDeadLabel);
        LockId(items, showDead, showDeadLabel);
        tab.AddItems(items.ToArray());
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
