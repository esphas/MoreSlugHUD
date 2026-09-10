using RWCustom;
using UnityEngine;

namespace MoreSlugHUD;

internal enum IdIntentKind
{
    Neutral = 0,
    Hostile = 1,
    Friendly = 2,
}

internal static class IdIntent
{
    internal const string NeutralElement = "SurvivorA";
    internal const string HostileElement = "miscDangerSymbol";
    internal const string FriendlyElement = "FriendA";

    internal static readonly Color NeutralColor = new(1f, 0.78f, 0.16f);
    internal static readonly Color HostileColor = new(1f, 0.12f, 0.08f);
    internal static readonly Color FriendlyColor = new(0.22f, 0.84f, 0.3f);

    internal static string ElementOf(IdIntentKind kind) => kind switch
    {
        IdIntentKind.Hostile => HostileElement,
        IdIntentKind.Friendly => FriendlyElement,
        _ => NeutralElement,
    };

    internal static Color ColorOf(IdIntentKind kind) => kind switch
    {
        IdIntentKind.Hostile => HostileColor,
        IdIntentKind.Friendly => FriendlyColor,
        _ => NeutralColor,
    };

    internal static float UniformScale(float width, float height, float target)
    {
        var longest = Mathf.Max(width, height);
        return longest > 0f ? target / longest : 1f;
    }

    internal static float FitVisible(float left, float width, float height, float target, out float visibleWidth, out float leftInset)
    {
        var scale = UniformScale(width, height, target);
        visibleWidth = width * scale;
        leftInset = left * scale;
        return scale;
    }

    internal static IdIntentKind Read(Creature creature, Player viewer)
    {
        if (creature.dead)
        {
            return IdIntentKind.Neutral;
        }

        if (IsHostileTo(creature, viewer))
        {
            return IdIntentKind.Hostile;
        }

        if (IsFriendlyTo(creature, viewer))
        {
            return IdIntentKind.Friendly;
        }

        return IdIntentKind.Neutral;
    }

    private static bool IsHostileTo(Creature creature, Player viewer)
    {
        if (Holding(creature, viewer))
        {
            return true;
        }

        if (creature is Lizard lizard)
        {
            return LizardHostile(lizard, viewer);
        }

        if (creature is Scavenger scavenger)
        {
            return ScavengerHostile(scavenger, viewer);
        }

        if (creature is Player pup && (pup.isNPC || pup.isSlugpup))
        {
            return PupHostile(pup, viewer);
        }

        return false;
    }

    private static bool IsFriendlyTo(Creature creature, Player viewer)
    {
        var friend = creature.abstractCreature?.abstractAI?.RealAI?.friendTracker?.friend;
        if (ReferenceEquals(friend, viewer))
        {
            return true;
        }

        var memory = creature.State?.socialMemory;
        var apo = viewer.abstractCreature;
        if (creature is Scavenger scavenger && scavenger.PlayerHasImmunity(viewer))
        {
            return true;
        }

        return memory != null && apo != null && memory.GetLike(apo.ID) > 0.5f;
    }

    private static bool LizardHostile(Lizard lizard, Player viewer)
    {
        if (TongueOn(lizard, viewer))
        {
            return true;
        }

        var behavior = lizard.AI?.behavior;
        if (behavior == LizardAI.Behavior.Hunt
            || behavior == LizardAI.Behavior.Frustrated
            || behavior == LizardAI.Behavior.Lurk
            || behavior == LizardAI.Behavior.GoToSpitPos)
        {
            return Hunting(lizard, viewer);
        }

        if (behavior == LizardAI.Behavior.Fighting)
        {
            return FocusIs(lizard.AI, viewer);
        }

        if (!Hunting(lizard, viewer))
        {
            return false;
        }

        var animation = lizard.animation;
        return animation == Lizard.Animation.PrepareToLounge
            || animation == Lizard.Animation.Lounge
            || animation == Lizard.Animation.FightingStance
            || animation == Lizard.Animation.ShootTongue
            || animation == Lizard.Animation.Spit
            || animation == Lizard.Animation.ShakePrey;
    }

    private static bool Hunting(Lizard lizard, Player viewer)
    {
        if (FocusIs(lizard.AI, viewer))
        {
            return true;
        }

        var prey = lizard.AI?.preyTracker?.MostAttractivePrey?.representedCreature?.realizedCreature;
        return ReferenceEquals(prey, viewer);
    }

    private static bool ScavengerHostile(Scavenger scavenger, Player viewer)
    {
        if (scavenger.animation is Scavenger.ThrowChargeAnimation charge
            && charge.target?.owner is Creature aimed
            && ReferenceEquals(aimed, viewer))
        {
            return true;
        }

        var tracked = scavenger.AI?.preyTracker?.MostAttractivePrey;
        var prey = tracked?.representedCreature?.realizedCreature;
        return scavenger.AI?.behavior == ScavengerAI.Behavior.Attack
            && tracked is { VisualContact: true }
            && ReferenceEquals(prey, viewer)
            && Custom.DistLess(scavenger.mainBodyChunk.pos, viewer.mainBodyChunk.pos, 90f);
    }

    private static bool PupHostile(Player pup, Player viewer)
    {
        var tracked = pup.AI?.preyTracker?.MostAttractivePrey;
        var prey = tracked?.representedCreature?.realizedCreature;
        return tracked is { VisualContact: true }
            && ReferenceEquals(prey, viewer)
            && Custom.DistLess(pup.mainBodyChunk.pos, viewer.mainBodyChunk.pos, 80f);
    }

    private static bool FocusIs(LizardAI? ai, Player viewer)
    {
        var focus = ai?.focusCreature?.representedCreature?.realizedCreature;
        return ReferenceEquals(focus, viewer);
    }

    private static bool TongueOn(Lizard lizard, Player viewer)
    {
        return lizard.tongue?.attached?.owner is Creature held && ReferenceEquals(held, viewer);
    }

    private static bool Holding(Creature creature, Player viewer)
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
}
