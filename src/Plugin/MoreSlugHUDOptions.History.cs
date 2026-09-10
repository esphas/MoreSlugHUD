using System.Collections.Generic;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

public sealed partial class MoreSlugHUDOptions
{
    public Configurable<bool> HistoryEnabled = null!;
    public Configurable<string> HistorySide = null!;
    public Configurable<string> HistoryDensity = null!;
    public Configurable<bool> HistoryLetters = null!;
    public Configurable<int> HistoryMaxRows = null!;
    public Configurable<float> HistoryOpacity = null!;
    public Configurable<bool> HistoryTrackDirection = null!;
    public Configurable<bool> HistoryTrackActions = null!;
    public Configurable<bool> HistoryTrackStates = null!;

    private readonly List<UIelement> _historyLockables = new();
    private readonly List<OpLabel> _historyLockLabels = new();
    private HistoryLegendPopup? _directionLegend;
    private HistoryLegendPopup? _actionLegend;
    private HistoryLegendPopup? _stateLegend;

    private void BindHistory()
    {
        HistoryEnabled = config.Bind("HistoryEnabled", true);
        HistorySide = config.Bind("HistorySide", "left", new ConfigAcceptableList<string>("left", "right"));
        HistoryDensity = config.Bind("HistoryDensity", "loose", new ConfigAcceptableList<string>("compact", "loose"));
        HistoryLetters = config.Bind("HistoryLetters", true);
        HistoryMaxRows = config.Bind("HistoryMaxRows", 17, new ConfigAcceptableRange<int>(1, 20));
        HistoryOpacity = config.Bind("HistoryOpacity", 0.85f, new ConfigAcceptableRange<float>(0f, 1f));
        HistoryTrackDirection = config.Bind("HistoryTrackDirection", true);
        HistoryTrackActions = config.Bind("HistoryTrackActions", true);
        HistoryTrackStates = config.Bind("HistoryTrackStates", true);
    }

    private void ApplyHistory()
    {
        InputHistoryConfig.Enabled = HistoryEnabled.Value;
        InputHistoryConfig.Layout = HistorySide.Value == "right" ? LayoutMode.Right : LayoutMode.Left;
        InputHistoryConfig.Density = HistoryDensity.Value == "compact" ? MoreSlugHUD.HistoryDensity.Compact : MoreSlugHUD.HistoryDensity.Loose;
        InputHistoryConfig.DisplayMode = HistoryLetters.Value ? DisplayMode.Letters : DisplayMode.Icons;
        InputHistoryConfig.MaxRows = HistoryMaxRows.Value;
        InputHistoryConfig.Opacity = HistoryOpacity.Value;
        InputHistoryConfig.TrackDirection = HistoryTrackDirection.Value;
        InputHistoryConfig.TrackActions = HistoryTrackActions.Value;
        InputHistoryConfig.TrackedStateMask = HistoryTrackStates.Value
            ? InputHistoryConfig.AllStates
            : MovementTags.None;
    }

    private OpTab BuildHistoryTab()
    {
        HudIcons.EnsureLoaded();
        _historyLockables.Clear();
        _historyLockLabels.Clear();
        var tab = new OpTab(this, L(LocKeys.OptionsTabHistory));
        const float labelX = 40f;
        const float controlX = 155f;
        const float col2 = 310f;
        const float control2 = 430f;
        var items = new List<UIelement>();
        var y = 540f;

        var enabled = new OpCheckBox(HistoryEnabled, new Vector2(labelX, y));
        var enabledLabel = new OpLabel(75f, y + 3f, L(LocKeys.OptionsHistoryEnabled));
        Hint(enabled, enabledLabel, LocKeys.OptionsHistoryEnabledHint);
        enabled.OnValueChanged += (_, _, _) => RefreshLocks();
        items.Add(enabled);
        items.Add(enabledLabel);
        y -= 38f;
        items.Add(Wrapped(labelX, y, 520f, L(LocKeys.OptionsHistoryToggleHint)));
        y -= 42f;

        var side = RaiseCombo(new OpComboBox(
            HistorySide,
            new Vector2(controlX, y),
            120f,
            new List<ListItem>
            {
                Item("left", LocKeys.OptionsHistorySideLeft, 0),
                Item("right", LocKeys.OptionsHistorySideRight, 1),
            })
        {
            listHeight = 2,
        });
        var sideLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsHistorySide));
        Hint(side, sideLabel, LocKeys.OptionsHistorySideHint);
        var density = RaiseCombo(new OpComboBox(
            HistoryDensity,
            new Vector2(control2, y),
            120f,
            new List<ListItem>
            {
                Item("compact", LocKeys.OptionsHistoryDensityCompact, 0, LocKeys.OptionsHistoryDensityCompactHint),
                Item("loose", LocKeys.OptionsHistoryDensityLoose, 1, LocKeys.OptionsHistoryDensityLooseHint),
            })
        {
            listHeight = 2,
        });
        var densityLabel = new OpLabel(col2, y + 5f, L(LocKeys.OptionsHistoryDensity));
        Lock(items, side, sideLabel);
        Lock(items, density, densityLabel);
        y -= 40f;

        var maxRows = new OpUpdown(HistoryMaxRows, new Vector2(controlX, y), 80f);
        var maxRowsLabel = new OpLabel(labelX, y + 5f, L(LocKeys.OptionsHistoryMaxRows));
        var opacity = new OpFloatSlider(HistoryOpacity, new Vector2(control2, y + 3f), 120, 2);
        var opacityLabel = new OpLabel(col2, y + 5f, L(LocKeys.OptionsHistoryOpacity));
        Lock(items, maxRows, maxRowsLabel);
        Lock(items, opacity, opacityLabel);
        y -= 40f;

        var letters = Check(HistoryLetters, labelX, y, LocKeys.OptionsHistoryLetters, out var lettersLabel);
        Hint(letters, lettersLabel, LocKeys.OptionsHistoryLettersHint);
        Lock(items, letters, lettersLabel);
        y -= 36f;

        _directionLegend = AddTrackRow(
            items, HistoryTrackDirection, LocKeys.OptionsHistoryTrackDirection, y, iconsOnly: true,
            HudIcons.DirectionElements, null, null, 3);
        y -= 34f;
        _actionLegend = AddTrackRow(
            items, HistoryTrackActions, LocKeys.OptionsHistoryTrackActions, y, iconsOnly: false,
            HudIcons.ActionElements, HudIcons.ActionLetters, LocKeys.HistoryActions, 1);
        y -= 34f;
        _stateLegend = AddTrackRow(
            items, HistoryTrackStates, LocKeys.OptionsHistoryTrackStates, y, iconsOnly: false,
            HudIcons.StateElements, HudIcons.StateLetters, LocKeys.HistoryStates, 2);

        tab.AddItems(items.ToArray());
        HideLegends();
        return tab;
    }

    private HistoryLegendPopup AddTrackRow(
        List<UIelement> items,
        Configurable<bool> config,
        string locKey,
        float y,
        bool iconsOnly,
        string[] elements,
        string[]? letters,
        string[]? locKeys,
        int columns)
    {
        var box = Check(config, 40f, y, locKey, out var label);
        Lock(items, box, label);
        var helpPos = new Vector2(160f, y);
        var helpSize = new Vector2(24f, 24f);
        var help = new OpSimpleButton(helpPos, helpSize, "?")
        {
            description = L(LocKeys.OptionsHistoryLegend),
        };
        var popup = new HistoryLegendPopup(helpPos, helpSize, elements, letters, locKeys, columns, iconsOnly, L);
        help.OnClick += _ => ToggleLegend(popup);
        items.Add(help);
        items.AddRange(popup.Items);
        _historyLockables.Add(help);
        return popup;
    }

    private void ToggleLegend(HistoryLegendPopup target)
    {
        var show = !target.Shown;
        HideLegends();
        if (show)
        {
            target.Show();
        }
    }

    private void HideLegends()
    {
        _directionLegend?.Hide();
        _actionLegend?.Hide();
        _stateLegend?.Hide();
    }

    private void RefreshHistoryLocks()
    {
        var on = HistoryEnabled.Value;
        var text = on ? MenuColorEffect.rgbMediumGrey : MenuColorEffect.rgbDarkGrey;
        for (var i = 0; i < _historyLockables.Count; i++)
        {
            if (_historyLockables[i] is UIfocusable focusable)
            {
                focusable.greyedOut = !on;
            }
        }

        for (var i = 0; i < _historyLockLabels.Count; i++)
        {
            _historyLockLabels[i].color = text;
        }

        if (!on)
        {
            HideLegends();
        }
    }

    private void Lock(List<UIelement> items, UIelement control, OpLabel label)
    {
        items.Add(control);
        items.Add(label);
        _historyLockables.Add(control);
        _historyLockLabels.Add(label);
    }

    private sealed class HistoryLegendPopup
    {
        private readonly List<UIelement> _items = new();
        internal bool Shown { get; private set; }

        internal IReadOnlyList<UIelement> Items => _items;

        internal HistoryLegendPopup(
            Vector2 buttonPos,
            Vector2 buttonSize,
            string[] elements,
            string[]? letters,
            string[]? locKeys,
            int columns,
            bool iconsOnly,
            System.Func<string, string> translate)
        {
            var rows = (elements.Length + columns - 1) / columns;
            Vector2 size;
            if (iconsOnly)
            {
                const float cell = 22f;
                const float pad = 8f;
                size = new Vector2(pad * 2f + columns * cell, pad * 2f + rows * cell);
            }
            else
            {
                var width = columns == 2 ? 280f : 230f;
                size = new Vector2(width, 16f + rows * 24f);
            }

            var pos = Place(buttonPos, buttonSize, size);
            _items.Add(new OpRect(pos, size, 0.85f));
            for (var i = 0; i < elements.Length; i++)
            {
                var col = i % columns;
                var row = i / columns;
                if (iconsOnly)
                {
                    const float cell = 22f;
                    const float pad = 8f;
                    var ix = pos.x + pad + col * cell + (cell - HudIcons.Slot) * 0.5f;
                    var iy = pos.y + size.y - pad - (row + 1) * cell + (cell - HudIcons.Slot) * 0.5f;
                    if (HudIcons.HasElement(elements[i]))
                    {
                        _items.Add(new OpImage(new Vector2(ix, iy), elements[i]));
                    }

                    continue;
                }

                var x = pos.x + 10f + col * (size.x / columns);
                var y = pos.y + size.y - 28f - row * 24f;
                if (HudIcons.HasElement(elements[i]))
                {
                    _items.Add(new OpImage(new Vector2(x, y + 4f), elements[i]));
                    x += HudIcons.Slot + 4f;
                }

                if (letters != null && i < letters.Length)
                {
                    _items.Add(new OpLabel(
                        new Vector2(x, y),
                        new Vector2(28f, 24f),
                        letters[i],
                        FLabelAlignment.Left));
                    x += 30f;
                }

                if (locKeys != null && i < locKeys.Length)
                {
                    _items.Add(new OpLabel(x, y + 3f, translate(locKeys[i])));
                }
            }
        }

        private static Vector2 Place(Vector2 buttonPos, Vector2 buttonSize, Vector2 popupSize)
        {
            const float gap = 6f;
            var x = buttonPos.x + buttonSize.x + gap;
            var y = buttonPos.y + buttonSize.y - popupSize.y;
            if (y < 12f)
            {
                y = 12f;
            }

            if (x + popupSize.x > 588f)
            {
                x = buttonPos.x - gap - popupSize.x;
            }

            return new Vector2(x, y);
        }

        internal void Show()
        {
            Shown = true;
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].Show();
                _items[i].MoveToFront();
            }
        }

        internal void Hide()
        {
            Shown = false;
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].Hide();
            }
        }
    }
}
