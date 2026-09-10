using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace MoreSlugHUD;

internal static class MoreSlugHUDLog
{
    private static readonly HashSet<string> Once = new();

    internal static void Info(string message) => Write(LogLevel.Info, message);

    internal static void Warning(string message) => Write(LogLevel.Warning, message);

    internal static void Error(string message, Exception? exception = null)
    {
        Write(LogLevel.Error, exception == null ? message : $"{message}: {exception}");
    }

    internal static void ErrorOnce(string key, string message, Exception? exception = null)
    {
        if (!Once.Add(key))
        {
            return;
        }

        Error($"{message} (further errors of this kind are suppressed)", exception);
    }

    internal static void Reset() => Once.Clear();

    private static void Write(LogLevel level, string message)
    {
        var logger = MoreSlugHUDPlugin.Log;
        if (logger == null)
        {
            UnityEngine.Debug.Log($"[MoreSlugHUD] {message}");
            return;
        }

        logger.Log(level, $"[MoreSlugHUD] {message}");
    }
}
