using System;
using System.Collections.Generic;
using System.Reflection;
using ImprovedInput;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MoreSlugHUD;

public sealed partial class MoreSlugHUDOptions : OptionInterface
{
    private OpLabel? _inventoryToggleHint;
    private OpLabel? _idToggleHint;
    private OpLabel? _historyToggleHint;

    public Configurable<int> ConfigVersion = null!;

    public MoreSlugHUDOptions()
    {
        BindSave();
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
        MigrateSave();
        ApplyInventory();
        ApplyId();
        ApplyHistory();
        RefreshLocks();
    }

    internal const int CurrentSaveVersion = 1;

    private void BindSave()
    {
        ConfigVersion = config.Bind("ConfigVersion", 0);
    }

    private void MigrateSave()
    {
        var version = ConfigVersion.Value;
        if (version >= CurrentSaveVersion)
        {
            return;
        }

        if (version < 1)
        {
            MigrateSaveTo1();
            version = 1;
        }

        ConfigVersion.Value = version;
    }

    private void RefreshLocks()
    {
        RefreshInventoryLocks();
        RefreshIdLocks();
        RefreshHistoryLocks();
        RefreshToggleHints();
    }

    private void RefreshToggleHints()
    {
        SetToggleHint(_inventoryToggleHint, MoreSlugHUDPlugin.ToggleKeybind);
        SetToggleHint(_idToggleHint, MoreSlugHUDPlugin.ToggleIdKeybind);
        SetToggleHint(_historyToggleHint, MoreSlugHUDPlugin.ToggleHistoryKeybind);
    }

    private static void SetToggleHint(OpLabel? label, PlayerKeybind? key)
    {
        if (label != null)
        {
            label.text = ToggleHintText(key);
        }
    }

    private static string ToggleHintText(PlayerKeybind? key)
    {
        var names = BindingNames(key);
        if (names.Count == 0)
        {
            return L(LocKeys.OptionsToggleHint) + L(LocKeys.OptionsToggleHintUnbound);
        }

        return L(LocKeys.OptionsToggleHint)
            + string.Format(L(LocKeys.OptionsToggleHintBound), string.Join(", ", names));
    }

    private static List<string> BindingNames(PlayerKeybind? key)
    {
        var names = new List<string>();
        if (key == null)
        {
            return names;
        }

        try
        {
            AddBindingName(names, FormatKey(key.Keyboard(0)));
            AddBindingName(names, FormatKey(key.Gamepad(0)));
        }
        catch
        {
        }

        try
        {
            var method = key.GetType().GetMethod("CurrentBindingName", new[] { typeof(int) });
            if (method?.Invoke(key, new object[] { 0 }) is string name)
            {
                AddBindingName(names, name);
            }
        }
        catch
        {
        }

        return names;
    }

    private static void AddBindingName(List<string> names, string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name == "None")
        {
            return;
        }

        for (var i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        names.Add(name!);
    }

    private static string? FormatKey(KeyCode code)
    {
        if (code == KeyCode.None)
        {
            return null;
        }

        return code switch
        {
            KeyCode.JoystickButton4 or KeyCode.Joystick1Button4 => "LB",
            KeyCode.JoystickButton5 or KeyCode.Joystick1Button5 => "RB",
            KeyCode.JoystickButton8 or KeyCode.Joystick1Button8 => "L3",
            KeyCode.JoystickButton9 or KeyCode.Joystick1Button9 => "R3",
            KeyCode.Mouse0 => "Mouse1",
            KeyCode.Mouse1 => "Mouse2",
            KeyCode.Mouse2 => "Mouse3",
            _ => PrettyKey(code),
        };
    }

    private static string PrettyKey(KeyCode code)
    {
        var name = code.ToString();
        if (name.StartsWith("Alpha", StringComparison.Ordinal))
        {
            return name.Substring(5);
        }

        if (name.StartsWith("Keypad", StringComparison.Ordinal))
        {
            return "Num " + name.Substring(6);
        }

        if (name.StartsWith("Joystick", StringComparison.Ordinal))
        {
            return name;
        }

        var text = new System.Text.StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            {
                text.Append(' ');
            }

            text.Append(name[i]);
        }

        return text.ToString();
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

    private static string L(string key) => Translate(key);
}
