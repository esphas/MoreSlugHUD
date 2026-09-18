using System;
using System.Reflection;
using UnityEngine;

namespace MoreSlugHUD;

internal static class PlayerGrabability
{
    private const float RetrySeconds = 2f;
    private const BindingFlags Flags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly object[] Args = new object[1];
    private static MethodInfo? _method;
    private static bool _resolved;
    private static bool _missing;
    private static float _retryAt;
    private static int _invokeBusy;

    internal static bool TryGet(Player player, PhysicalObject obj, out Player.ObjectGrabability result)
    {
        result = default;
        if (!EnsureMethod() || _method == null)
        {
            return false;
        }

        if (_retryAt > 0f && Time.realtimeSinceStartup < _retryAt)
        {
            return false;
        }

        var nested = _invokeBusy > 0;
        var args = nested ? new object[1] : Args;
        _invokeBusy++;
        args[0] = obj;
        try
        {
            var raw = _method.Invoke(player, args);
            if (raw == null)
            {
                return false;
            }

            result = (Player.ObjectGrabability)raw;
            _retryAt = 0f;
            return true;
        }
        catch (Exception exception)
        {
            _retryAt = Time.realtimeSinceStartup + RetrySeconds;
            MoreSlugHUDLog.ErrorOnce(
                "player-grabability-invoke",
                "Player.Grabability invoke failed; will retry",
                Unwrap(exception));
            return false;
        }
        finally
        {
            args[0] = null!;
            _invokeBusy--;
        }
    }

    private static bool EnsureMethod()
    {
        if (_resolved)
        {
            return !_missing;
        }

        _resolved = true;
        try
        {
            _method = Find();
            if (_method == null)
            {
                _missing = true;
                MoreSlugHUDLog.Error(
                    "Player.Grabability(PhysicalObject) was not found; pick-up candidate preview is disabled");
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            _missing = true;
            _method = null;
            MoreSlugHUDLog.Error(
                "Player.Grabability(PhysicalObject) lookup failed; pick-up candidate preview is disabled",
                exception);
            return false;
        }
    }

    private static MethodInfo? Find()
    {
        foreach (var method in typeof(Player).GetMethods(Flags))
        {
            if (method.Name != "Grabability")
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(PhysicalObject))
            {
                continue;
            }

            var returned = method.ReturnType;
            if (returned != typeof(Player.ObjectGrabability) && returned.Name != "ObjectGrabability")
            {
                continue;
            }

            return method;
        }

        return null;
    }

    private static Exception Unwrap(Exception exception) =>
        exception is TargetInvocationException target && target.InnerException != null
            ? target.InnerException
            : exception;
}
