using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using ImprovedInput;
using UnityEngine;

[assembly: AssemblyTitle("MoreSlugHUD")]
[assembly: AssemblyDescription("Configurable HUDs for Rain World")]
[assembly: AssemblyVersion("0.1.0")]
[assembly: AssemblyFileVersion("0.1.0")]
[assembly: ComVisible(false)]

namespace MoreSlugHUD;

internal static class PluginInfo
{
    internal const string Guid = "more_slug_hud";
    internal const string ModId = "more_slug_hud";
    internal const string Name = "MoreSlugHUD";
    internal const string Version = "0.1.0";
    internal const string ToggleKeybindId = "more-slug-hud:toggle";
    internal const string ToggleIdKeybindId = "more-slug-hud:toggle-id";
    internal const string ToggleHistoryKeybindId = "more-slug-hud:toggle-history";
}

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
[BepInDependency("com.dual.improved-input-config", BepInDependency.DependencyFlags.HardDependency)]
public sealed class MoreSlugHUDPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static PlayerKeybind? ToggleKeybind { get; private set; }
    internal static PlayerKeybind? ToggleIdKeybind { get; private set; }
    internal static PlayerKeybind? ToggleHistoryKeybind { get; private set; }
    internal static bool Active { get; private set; }

    private static bool _optionsRegistered;
    private static bool _hooksApplied;

    private void OnEnable()
    {
        Log = Logger;
        Active = true;
        On.RainWorld.OnModsInit += OnModsInit;

        try
        {
            ToggleKeybind = PlayerKeybind.Get(PluginInfo.ToggleKeybindId)
                ?? PlayerKeybind.Register(
                    PluginInfo.ToggleKeybindId,
                    PluginInfo.Name,
                    LocKeys.InputToggleHud,
                    KeyCode.None,
                    KeyCode.None);
            ToggleIdKeybind = PlayerKeybind.Get(PluginInfo.ToggleIdKeybindId)
                ?? PlayerKeybind.Register(
                    PluginInfo.ToggleIdKeybindId,
                    PluginInfo.Name,
                    LocKeys.InputToggleId,
                    KeyCode.None,
                    KeyCode.None);
            ToggleHistoryKeybind = PlayerKeybind.Get(PluginInfo.ToggleHistoryKeybindId)
                ?? PlayerKeybind.Register(
                    PluginInfo.ToggleHistoryKeybindId,
                    PluginInfo.Name,
                    LocKeys.InputToggleHistory,
                    KeyCode.H,
                    KeyCode.JoystickButton8);
        }
        catch (Exception exception)
        {
            ToggleKeybind = null;
            ToggleIdKeybind = null;
            ToggleHistoryKeybind = null;
            Log.LogError($"Keybind registration failed: {exception}");
        }

        ApplyHooks();
    }

    private void OnDisable()
    {
        Active = false;
        On.RainWorld.OnModsInit -= OnModsInit;
        RemoveHooks();
        MeadowOwnership.Reset();
        DownpourCompat.Reset();
        LizardOnBackAdapter.Reset();
        HudIcons.Reset();
        IdObservers.Clear();
        MoreSlugHUDLog.Reset();
    }

    private static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        MeadowOwnership.Probe();
        LizardOnBackAdapter.Probe();
        DownpourCompat.Probe();
        HudIcons.EnsureLoaded();
        if (_optionsRegistered)
        {
            return;
        }

        try
        {
            var options = new MoreSlugHUDOptions();
            if (MachineConnector.SetRegisteredOI(PluginInfo.ModId, options))
            {
                options.ApplyToRuntime();
                _optionsRegistered = true;
                MoreSlugHUDLog.Info("Remix options registered");
            }
            else
            {
                MoreSlugHUDLog.Warning("Remix options registration failed; using defaults");
            }
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.Error("Remix options initialization failed", exception);
        }
    }

    private static void ApplyHooks()
    {
        if (_hooksApplied)
        {
            return;
        }

        HudHooks.Apply();
        IdHooks.Apply();
        PlayerInputHooks.Apply();
        _hooksApplied = true;
    }

    private static void RemoveHooks()
    {
        if (!_hooksApplied)
        {
            return;
        }

        HudHooks.Remove();
        IdHooks.Remove();
        PlayerInputHooks.Remove();
        _hooksApplied = false;
    }
}
