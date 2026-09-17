namespace MoreSlugHUD;

internal enum IntentReason
{
    None = 0,
    Dead,
    HoldingViewer,
    TongueOnViewer,
    HuntingViewer,
    FightingViewer,
    LizardCombatAnimation,
    ScavengerThrowCharge,
    ScavengerAttackRelation,
    PupHuntingViewer,
    CurrentFriend,
    PersonalLikeThreshold,
    Immunity,
    ScavengerPack,
    ScavengerOpinionThreshold,
    NoMatchingEvidence,
}

internal enum IntentEvidenceSource
{
    None = 0,
    Action,
    DynamicRelation,
    PersonalMemory,
    Immunity,
}

internal readonly struct IntentDecision
{
    internal IntentDecision(IdIntentKind kind, IntentReason reason, IntentEvidenceSource evidence)
    {
        Kind = kind;
        Reason = reason;
        Evidence = evidence;
    }

    internal IdIntentKind Kind { get; }
    internal IntentReason Reason { get; }
    internal IntentEvidenceSource Evidence { get; }

    internal static IntentDecision Hostile(IntentReason reason, IntentEvidenceSource evidence) =>
        new(IdIntentKind.Hostile, reason, evidence);

    internal static IntentDecision Friendly(IntentReason reason, IntentEvidenceSource evidence) =>
        new(IdIntentKind.Friendly, reason, evidence);

    internal static IntentDecision Neutral(IntentReason reason) =>
        new(IdIntentKind.Neutral, reason, IntentEvidenceSource.None);
}
