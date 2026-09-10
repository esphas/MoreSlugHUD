using System.Collections.Generic;
using UnityEngine;

namespace MoreSlugHUD;

internal readonly struct IdIntentDrawState
{
    internal IdIntentDrawState(IdIntentKind shown, Vector2 shakeOffset)
    {
        Shown = shown;
        ShakeOffset = shakeOffset;
    }

    internal IdIntentKind Shown { get; }
    internal Vector2 ShakeOffset { get; }

    internal static IdIntentDrawState Hidden => default;
}

internal sealed class IdIntentStates
{
    private readonly Dictionary<(int Spawner, int Number), ViewerIntent> _intents = new();

    internal int Count => _intents.Count;

    internal void Clear() => _intents.Clear();

    internal IdIntentDrawState Peek((int Spawner, int Number) key)
    {
        return _intents.TryGetValue(key, out var state)
            ? new IdIntentDrawState(state.Shown, state.ShakeOffset)
            : IdIntentDrawState.Hidden;
    }

    internal void Tick((int Spawner, int Number) key, IdIntentKind raw, ref uint shakeRng)
    {
        if (!_intents.TryGetValue(key, out var state))
        {
            state = new ViewerIntent();
            _intents[key] = state;
        }

        state.Advance(raw, ref shakeRng);
    }

    private sealed class ViewerIntent
    {
        internal IdIntentKind Shown;
        internal IdIntentKind Pending;
        internal IdIntentKind ShakeTo;
        internal int PendingTicks;
        internal int ShakeTicks;
        internal bool HasShown;
        internal Vector2 ShakeOffset;

        internal void Advance(IdIntentKind raw, ref uint shakeRng)
        {
            if (ShakeTicks > 0)
            {
                ShakeTicks--;
                ShakeOffset = NextShakeDir(ref shakeRng) * MoreSlugHUDConfig.IdIntentShakePx
                    * (ShakeTicks / (float)MoreSlugHUDConfig.IdIntentShakeTicks);
                if (ShakeTicks == 0)
                {
                    Shown = ShakeTo;
                    ShakeOffset = Vector2.zero;
                }

                return;
            }

            if (raw != Pending)
            {
                Pending = raw;
                PendingTicks = 1;
            }
            else
            {
                PendingTicks++;
            }

            if (Pending == Shown || PendingTicks < SettleTicks(Pending, Shown))
            {
                return;
            }

            if (!HasShown)
            {
                Shown = Pending;
                HasShown = true;
                return;
            }

            ShakeTo = Pending;
            ShakeTicks = MoreSlugHUDConfig.IdIntentShakeTicks;
            ShakeOffset = NextShakeDir(ref shakeRng) * MoreSlugHUDConfig.IdIntentShakePx;
        }
    }

    private static Vector2 NextShakeDir(ref uint shakeRng)
    {
        var vector = new Vector2(NextUnit(ref shakeRng), NextUnit(ref shakeRng));
        if (vector.sqrMagnitude < 0.0001f)
        {
            return Vector2.up;
        }

        return vector.normalized;
    }

    private static float NextUnit(ref uint shakeRng)
    {
        shakeRng ^= shakeRng << 13;
        shakeRng ^= shakeRng >> 17;
        shakeRng ^= shakeRng << 5;
        return (shakeRng & 0xFFFF) / 32768f - 1f;
    }

    private static int SettleTicks(IdIntentKind pending, IdIntentKind shown)
    {
        if (pending == IdIntentKind.Hostile)
        {
            return MoreSlugHUDConfig.IdIntentHostileEnterTicks;
        }

        if (shown == IdIntentKind.Hostile)
        {
            return MoreSlugHUDConfig.IdIntentLeaveHostileTicks;
        }

        return MoreSlugHUDConfig.IdIntentSettleTicks;
    }
}
