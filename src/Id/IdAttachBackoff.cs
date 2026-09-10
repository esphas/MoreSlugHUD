using System;

namespace MoreSlugHUD;

internal sealed class IdAttachBackoff
{
    internal static readonly float[] Delays = [0.5f, 1f, 2f, 5f];

    internal string Stage = "";
    internal int Attempts;
    internal float FirstFailedAt;
    internal float RetryAt;
    internal string? LastExceptionType;

    internal bool IsWaiting(float now) => Attempts > 0 && now < RetryAt;

    internal void Record(string stage, Exception exception, float now)
    {
        if (Attempts > 0 && Stage != stage)
        {
            Attempts = 0;
            FirstFailedAt = 0f;
        }

        if (Attempts == 0)
        {
            FirstFailedAt = now;
        }

        Stage = stage;
        Attempts++;
        LastExceptionType = exception.GetType().FullName;
        RetryAt = now + Delay(Attempts);
    }

    internal void Clear()
    {
        Stage = "";
        Attempts = 0;
        FirstFailedAt = 0f;
        RetryAt = 0f;
        LastExceptionType = null;
    }

    internal static float Delay(int attempts)
    {
        var index = Math.Min(Math.Max(attempts, 1), Delays.Length) - 1;
        return Delays[index];
    }
}
