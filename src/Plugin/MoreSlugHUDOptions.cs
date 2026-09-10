using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

public sealed partial class MoreSlugHUDOptions : OptionInterface
{
    public MoreSlugHUDOptions()
    {
        BindInventory();
        BindId();
        BindHistory();
        OnConfigChanged += ApplyToRuntime;
        OnActivate += ApplyToRuntime;
    }

    public override void Initialize()
    {
        base.Initialize();
        Tabs = [BuildInventoryTab(), BuildIdTab(), BuildHistoryTab()];
        RefreshLocks();
    }

    internal void ApplyToRuntime()
    {
        ApplyInventory();
        ApplyId();
        ApplyHistory();
        RefreshLocks();
    }

    private void RefreshLocks()
    {
        RefreshInventoryLocks();
        RefreshIdLocks();
        RefreshHistoryLocks();
    }

    private static void SetLabelColor(OpLabel? label, Color color)
    {
        if (label != null)
        {
            label.color = color;
        }
    }

    private static OpCheckBox Check(
        Configurable<bool> config,
        float x,
        float y,
        string labelKey,
        out OpLabel label)
    {
        var box = new OpCheckBox(config, new Vector2(x, y));
        label = new OpLabel(x + 35f, y + 3f, L(labelKey));
        return box;
    }

    private static void Hint(UIelement control, OpLabel? label, string hintKey)
    {
        var text = L(hintKey);
        control.description = text;
        if (label == null)
        {
            return;
        }

        label.description = text;
        if (control is UIfocusable focusable)
        {
            label.bumpBehav = focusable.bumpBehav;
        }
    }

    private static ListItem Item(string name, string displayKey, int value, string? hintKey = null) =>
        new(name, L(displayKey), value)
        {
            desc = hintKey == null ? "" : L(hintKey),
        };

    private static OpComboBox RaiseCombo(OpComboBox combo)
    {
        combo.OnListOpen += _ => combo.MoveToFront();
        combo.OnListClose += _ => combo.MoveToBack();
        return combo;
    }

    private static OpLabel Wrapped(float x, float y, float width, string text) =>
        new(new Vector2(x, y), new Vector2(width, 30f), text, FLabelAlignment.Left)
        {
            autoWrap = true,
        };

    private static string L(string key) => Translate(key);
}
