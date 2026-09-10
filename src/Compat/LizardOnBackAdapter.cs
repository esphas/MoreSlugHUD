using System;
using System.Linq;
using System.Reflection;

namespace MoreSlugHUD;

internal static class LizardOnBackAdapter
{
    private const string ModId = "ShanKa.LizardOnBack";

    private static bool _probed;
    private static bool _available;
    private static FieldInfo? _lizardField;
    private static MethodInfo? _tryGetValue;
    private static object? _table;
    private static readonly object?[] TryGetArgs = new object?[2];
    private static int _tryBusy;

    internal static void Reset()
    {
        _probed = false;
        _available = false;
        _lizardField = null;
        _tryGetValue = null;
        _table = null;
        TryGetArgs[0] = null;
        TryGetArgs[1] = null;
        _tryBusy = 0;
    }

    internal static void Probe()
    {
        if (_probed)
        {
            return;
        }

        _probed = true;
        try
        {
            if (ModManager.ActiveMods == null || !ModManager.ActiveMods.Any(mod => mod.id == ModId))
            {
                return;
            }

            Type? dataType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    dataType = assembly.GetType("LizardOnBackMod.LizardOnBackHook+LizardOnBack", throwOnError: false);
                }
                catch
                {
                    continue;
                }

                if (dataType != null)
                {
                    break;
                }
            }

            if (dataType == null)
            {
                MoreSlugHUDLog.Warning("Piggyback Lizard is active but LizardOnBack type was not found; using vanilla back slots");
                return;
            }

            var tableField = dataType.GetField("ExPlayerLizardOnBackData", BindingFlags.Public | BindingFlags.Static);
            _table = tableField?.GetValue(null);
            if (_table == null)
            {
                MoreSlugHUDLog.Warning("Piggyback Lizard table was missing; using vanilla back slots");
                return;
            }

            _tryGetValue = _table.GetType().GetMethod("TryGetValue", BindingFlags.Public | BindingFlags.Instance);
            _lizardField = dataType.GetField("lizard", BindingFlags.Public | BindingFlags.Instance);
            if (_tryGetValue == null || _lizardField == null)
            {
                MoreSlugHUDLog.Warning("Piggyback Lizard members were incompatible; using vanilla back slots");
                return;
            }

            _available = true;
            MoreSlugHUDLog.Info("Piggyback Lizard adapter ready");
        }
        catch (Exception exception)
        {
            _available = false;
            MoreSlugHUDLog.Error("Piggyback Lizard probe failed; using vanilla back slots", exception);
        }
    }

    internal static AbstractPhysicalObject? TryGetLizard(Player player)
    {
        Probe();
        if (!_available || _table == null || _tryGetValue == null || _lizardField == null)
        {
            return null;
        }

        var nested = _tryBusy > 0;
        var args = nested ? new object?[2] : TryGetArgs;
        _tryBusy++;
        args[0] = player;
        args[1] = null;
        try
        {
            if (_tryGetValue.Invoke(_table, args) is not true)
            {
                return null;
            }

            var data = args[1];
            if (data == null)
            {
                return null;
            }

            return _lizardField.GetValue(data) is Lizard lizard ? lizard.abstractPhysicalObject : null;
        }
        catch (Exception exception)
        {
            _available = false;
            MoreSlugHUDLog.Error("Piggyback Lizard read failed; disabling adapter", exception);
            return null;
        }
        finally
        {
            args[0] = null;
            args[1] = null;
            _tryBusy--;
        }
    }
}
