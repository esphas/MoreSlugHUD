using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal static class CreatureIntentReader
{
    internal static bool HoldingViewer(Creature creature, Player viewer)
    {
        var grasps = creature.grasps;
        if (grasps == null)
        {
            return false;
        }

        for (var i = 0; i < grasps.Length; i++)
        {
            if (ReferenceEquals(grasps[i]?.grabbed, viewer))
            {
                return true;
            }
        }

        return false;
    }

    internal static Creature? FriendOf(Creature creature) =>
        creature.abstractCreature?.abstractAI?.RealAI?.friendTracker?.friend;

    internal static bool IsCurrentFriend(Creature creature, Player viewer) =>
        ReferenceEquals(FriendOf(creature), viewer);

    internal static bool LizardTongueOnViewer(Lizard lizard, Player viewer) =>
        lizard.tongue?.attached?.owner is Creature held && ReferenceEquals(held, viewer);

    internal static bool LizardFocusIsViewer(LizardAI? ai, Player viewer)
    {
        var focus = ai?.focusCreature?.representedCreature?.realizedCreature;
        return ReferenceEquals(focus, viewer);
    }

    internal static bool LizardPreyIsViewer(Lizard lizard, Player viewer)
    {
        var prey = lizard.AI?.preyTracker?.MostAttractivePrey?.representedCreature?.realizedCreature;
        return ReferenceEquals(prey, viewer);
    }

    internal static bool LizardHuntingViewer(Lizard lizard, Player viewer) =>
        LizardFocusIsViewer(lizard.AI, viewer) || LizardPreyIsViewer(lizard, viewer);

    internal static bool LizardFollowingViewer(Lizard lizard, Player viewer) =>
        lizard.AI?.behavior == LizardAI.Behavior.FollowFriend
        && ReferenceEquals(lizard.AI.friendTracker?.friend, viewer);

    internal static RelationshipTracker.DynamicRelationship? ScavengerPlayerRel(Scavenger scavenger, Player viewer)
    {
        var apo = viewer.abstractCreature;
        var tracker = scavenger.AI?.tracker;
        if (tracker == null || apo == null)
        {
            return null;
        }

        return tracker.RepresentationForCreature(apo, false)?.dynamicRelationship;
    }

    internal static bool ScavengerThrowChargingViewer(Scavenger scavenger, Player viewer) =>
        scavenger.animation is Scavenger.ThrowChargeAnimation charge
        && charge.target?.owner is Creature aimed
        && ReferenceEquals(aimed, viewer);

    internal static bool WithinPixels(Vector2 a, Vector2 b, float pixels) => Custom.DistLess(a, b, pixels);

    internal static float PersonalLike(Creature creature, Player viewer)
    {
        var memory = creature.State?.socialMemory;
        var apo = viewer.abstractCreature;
        if (memory == null || apo == null)
        {
            return float.NaN;
        }

        return memory.GetLike(apo.ID);
    }
}
