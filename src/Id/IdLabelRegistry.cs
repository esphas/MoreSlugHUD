using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MoreSlugHUD;

internal static class IdLabelRegistry
{
    private static readonly Dictionary<Creature, IdLabel> Instances = new();
    private static ConditionalWeakTable<Creature, IdAttachBackoff> Failed = new();
    private static readonly Func<IdLabel, bool> Live = static label => !label.Abandoned;

    internal static void EnsureAttached(Creature creature)
    {
        if (!MoreSlugHUDConfig.IdAttachable || creature.room == null || creature.slatedForDeletetion)
        {
            return;
        }

        if (Instances.TryGetValue(creature, out var existing) && !existing.Abandoned)
        {
            return;
        }

        var now = Time.realtimeSinceStartup;
        if (IdAttachGate.ShouldSkip(Failed, creature, now, live: false))
        {
            return;
        }

        TryAttach(creature, now);
    }

    private static void TryAttach(Creature creature, float now)
    {
        IdAttachAttempt.Run(
            Instances,
            Failed,
            creature,
            Live,
            () => new IdLabel(creature),
            label => creature.room.AddObject(label),
            Rollback,
            now);
    }

    internal static void ConfirmJoined(Creature creature, IdLabel label)
    {
        IdAttachAttempt.ConfirmJoined(Instances, Failed, creature, label, Live);
    }

    internal static void ClearAll()
    {
        var copy = Instances.Values.ToArray();
        Instances.Clear();
        for (var i = 0; i < copy.Length; i++)
        {
            copy[i].Destroy();
        }

        Failed = new ConditionalWeakTable<Creature, IdAttachBackoff>();
        IdFamiliarity.Reset();
        IdLabelView.ResetGlyphCache();
    }

    internal static void Unindex(Creature creature, IdLabel label)
    {
        IdAttachIndex.RemoveIfCurrent(Instances, creature, label);
    }

    internal static void Rollback(Creature creature, IdLabel? label, Exception exception, float now)
    {
        if (label != null)
        {
            IdAttachIndex.RemoveIfCurrent(Instances, creature, label);
            var hosted = label.room;
            try
            {
                hosted?.RemoveObject(label);
            }
            catch (Exception cleanup)
            {
                LogCleanup(cleanup);
            }

            try
            {
                label.Abandon();
            }
            catch (Exception cleanup)
            {
                LogCleanup(cleanup);
            }
        }

        IdAttachFail.Record(Failed, creature, exception, now);
        MoreSlugHUDLog.ErrorOnce(
            $"id-attach-{IdAttachFail.AttachStage}-{exception.GetType().FullName}",
            "ID label attach failed",
            exception);
    }

    private static void LogCleanup(Exception exception)
    {
        MoreSlugHUDLog.ErrorOnce(
            $"id-attach-DestroyCleanup-{exception.GetType().FullName}",
            "ID label cleanup failed",
            exception);
    }
}
