using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MoreSlugHUD;

internal static class MeadowOwnership
{
    private const string MeadowModId = "henpemaz_rainmeadow";
    private const int FailThreshold = 3;
    private const float RetrySeconds = 5f;

    private sealed class FailState
    {
        internal int Generation;
        internal int Consecutive;
        internal float RetryAt;
    }

    private static bool _probed;
    private static bool _modPresent;
    private static Func<AbstractPhysicalObject, bool>? _isLocal;
    private static int _generation;
    private static readonly ConditionalWeakTable<AbstractPhysicalObject, FailState> Fails = new();

    internal static bool IsMeadowActive
    {
        get
        {
            EnsureProbed();
            return _modPresent;
        }
    }

    internal static bool TryIsLocal(Player player, out bool isLocal)
    {
        isLocal = false;
        EnsureProbed();
        if (!_modPresent || _isLocal == null)
        {
            return false;
        }

        var apo = player.abstractCreature;
        if (apo == null)
        {
            return false;
        }

        var state = Fails.GetValue(apo, _ => new FailState());
        if (state.Generation != _generation)
        {
            state.Generation = _generation;
            state.Consecutive = 0;
            state.RetryAt = 0f;
        }

        if (state.RetryAt > 0f && Time.realtimeSinceStartup < state.RetryAt)
        {
            return false;
        }

        try
        {
            isLocal = _isLocal(apo);
            state.Consecutive = 0;
            state.RetryAt = 0f;
            return true;
        }
        catch (Exception exception)
        {
            state.Consecutive++;
            MoreSlugHUDLog.ErrorOnce("meadow-islocal", "Meadow IsLocal failed; will retry", exception);
            if (state.Consecutive >= FailThreshold)
            {
                state.RetryAt = Time.realtimeSinceStartup + RetrySeconds;
                state.Consecutive = 0;
            }

            return false;
        }
    }

    internal static void Probe() => EnsureProbed();

    internal static void Reset()
    {
        _probed = false;
        _modPresent = false;
        _isLocal = null;
        _generation++;
    }

    private static void EnsureProbed()
    {
        if (_probed)
        {
            return;
        }

        _probed = true;
        try
        {
            _modPresent = ModManager.ActiveMods != null
                && ModManager.ActiveMods.Any(mod => mod.id == MeadowModId);
            if (!_modPresent)
            {
                return;
            }

            BindIsLocal();
            if (_isLocal == null)
            {
                MoreSlugHUDLog.Warning("Rain Meadow is active but IsLocal was not found; remote players will be excluded");
            }
        }
        catch (Exception exception)
        {
            _modPresent = false;
            MoreSlugHUDLog.Error("Meadow probe failed; treating as absent", exception);
        }
    }

    private static void BindIsLocal()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? extensions;
            try
            {
                extensions = assembly.GetType("RainMeadow.Extensions", throwOnError: false);
            }
            catch
            {
                continue;
            }

            if (extensions == null)
            {
                continue;
            }

            foreach (var method in extensions.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != "IsLocal" || method.ReturnType != typeof(bool))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 1
                    || !typeof(AbstractPhysicalObject).IsAssignableFrom(parameters[0].ParameterType))
                {
                    continue;
                }

                _isLocal = (Func<AbstractPhysicalObject, bool>)Delegate.CreateDelegate(
                    typeof(Func<AbstractPhysicalObject, bool>),
                    method);
                return;
            }
        }
    }
}
