namespace MoreSlugHUD;

internal static class CreatureIntentRules
{
    internal const float PersonalFriendlyLikeThreshold = 0.5f;
    internal const float ScavengerFriendlyOpinionThreshold = 0.8f;
    internal const float PupHostileDistancePx = 80f;

    internal static IntentDecision Decide(Creature creature, Player viewer)
    {
        if (creature.dead)
        {
            return IntentDecision.Neutral(IntentReason.Dead);
        }

        if (CreatureIntentReader.HoldingViewer(creature, viewer))
        {
            return IntentDecision.Hostile(IntentReason.HoldingViewer, IntentEvidenceSource.Action);
        }

        if (TryHostile(creature, viewer, out var hostile))
        {
            return hostile;
        }

        if (TryFriendly(creature, viewer, out var friendly))
        {
            return friendly;
        }

        return IntentDecision.Neutral(IntentReason.NoMatchingEvidence);
    }

    private static bool TryHostile(Creature creature, Player viewer, out IntentDecision decision)
    {
        decision = default;
        if (creature is Lizard lizard)
        {
            return TryLizardHostile(lizard, viewer, out decision);
        }

        if (creature is Scavenger scavenger)
        {
            return TryScavengerHostile(scavenger, viewer, out decision);
        }

        if (creature is Player pup && (pup.isNPC || pup.isSlugpup))
        {
            return TryPupHostile(pup, viewer, out decision);
        }

        return false;
    }

    private static bool TryLizardHostile(Lizard lizard, Player viewer, out IntentDecision decision)
    {
        decision = default;
        if (CreatureIntentReader.LizardFollowingViewer(lizard, viewer))
        {
            return false;
        }

        if (CreatureIntentReader.LizardTongueOnViewer(lizard, viewer))
        {
            decision = IntentDecision.Hostile(IntentReason.TongueOnViewer, IntentEvidenceSource.Action);
            return true;
        }

        var behavior = lizard.AI?.behavior;
        if (behavior == LizardAI.Behavior.Hunt
            || behavior == LizardAI.Behavior.Frustrated
            || behavior == LizardAI.Behavior.Lurk
            || behavior == LizardAI.Behavior.GoToSpitPos)
        {
            if (!CreatureIntentReader.LizardHuntingViewer(lizard, viewer))
            {
                return false;
            }

            decision = IntentDecision.Hostile(IntentReason.HuntingViewer, IntentEvidenceSource.Action);
            return true;
        }

        if (behavior == LizardAI.Behavior.Fighting)
        {
            if (!CreatureIntentReader.LizardFocusIsViewer(lizard.AI, viewer))
            {
                return false;
            }

            decision = IntentDecision.Hostile(IntentReason.FightingViewer, IntentEvidenceSource.Action);
            return true;
        }

        if (!CreatureIntentReader.LizardHuntingViewer(lizard, viewer))
        {
            return false;
        }

        var animation = lizard.animation;
        if (animation != Lizard.Animation.PrepareToLounge
            && animation != Lizard.Animation.Lounge
            && animation != Lizard.Animation.FightingStance
            && animation != Lizard.Animation.ShootTongue
            && animation != Lizard.Animation.Spit
            && animation != Lizard.Animation.ShakePrey)
        {
            return false;
        }

        decision = IntentDecision.Hostile(IntentReason.LizardCombatAnimation, IntentEvidenceSource.Action);
        return true;
    }

    private static bool TryScavengerHostile(Scavenger scavenger, Player viewer, out IntentDecision decision)
    {
        decision = default;
        if (CreatureIntentReader.ScavengerThrowChargingViewer(scavenger, viewer))
        {
            decision = IntentDecision.Hostile(IntentReason.ScavengerThrowCharge, IntentEvidenceSource.Action);
            return true;
        }

        var rel = CreatureIntentReader.ScavengerPlayerRel(scavenger, viewer);
        if (rel == null || rel.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks)
        {
            return false;
        }

        decision = IntentDecision.Hostile(IntentReason.ScavengerAttackRelation, IntentEvidenceSource.DynamicRelation);
        return true;
    }

    private static bool TryPupHostile(Player pup, Player viewer, out IntentDecision decision)
    {
        decision = default;
        var tracked = pup.AI?.preyTracker?.MostAttractivePrey;
        var prey = tracked?.representedCreature?.realizedCreature;
        if (tracked is not { VisualContact: true }
            || !ReferenceEquals(prey, viewer)
            || !CreatureIntentReader.WithinPixels(pup.mainBodyChunk.pos, viewer.mainBodyChunk.pos, PupHostileDistancePx))
        {
            return false;
        }

        decision = IntentDecision.Hostile(IntentReason.PupHuntingViewer, IntentEvidenceSource.Action);
        return true;
    }

    private static bool TryFriendly(Creature creature, Player viewer, out IntentDecision decision)
    {
        decision = default;
        if (CreatureIntentReader.IsCurrentFriend(creature, viewer))
        {
            decision = IntentDecision.Friendly(IntentReason.CurrentFriend, IntentEvidenceSource.DynamicRelation);
            return true;
        }

        if (creature is Scavenger scavenger)
        {
            return TryScavengerFriendly(scavenger, viewer, out decision);
        }

        var like = CreatureIntentReader.PersonalLike(creature, viewer);
        if (float.IsNaN(like) || like <= PersonalFriendlyLikeThreshold)
        {
            return false;
        }

        decision = IntentDecision.Friendly(IntentReason.PersonalLikeThreshold, IntentEvidenceSource.PersonalMemory);
        return true;
    }

    private static bool TryScavengerFriendly(Scavenger scavenger, Player viewer, out IntentDecision decision)
    {
        decision = default;
        if (scavenger.PlayerHasImmunity(viewer))
        {
            decision = IntentDecision.Friendly(IntentReason.Immunity, IntentEvidenceSource.Immunity);
            return true;
        }

        var rel = CreatureIntentReader.ScavengerPlayerRel(scavenger, viewer);
        if (rel == null || scavenger.AI == null)
        {
            return false;
        }

        if (rel.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
        {
            decision = IntentDecision.Friendly(IntentReason.ScavengerPack, IntentEvidenceSource.DynamicRelation);
            return true;
        }

        if (scavenger.AI.LikeOfPlayer(rel) < ScavengerFriendlyOpinionThreshold)
        {
            return false;
        }

        decision = IntentDecision.Friendly(IntentReason.ScavengerOpinionThreshold, IntentEvidenceSource.DynamicRelation);
        return true;
    }
}
