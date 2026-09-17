namespace MoreSlugHUD;

internal enum CraftResultKind
{
    None = 0,
    Item = 1,
    Creature = 2,
    Food = 3,
}

internal enum CraftUnavailableReason
{
    None = 0,
    NotApplicable = 1,
    NoFood = 2,
    HoldingEdible = 3,
    NoCraftableSpear = 4,
    NoRecipe = 5,
    SpearmasterFoodBlocked = 6,
    UnknownCreature = 7,
}

internal readonly struct CraftPrediction
{
    private CraftPrediction(
        bool available,
        CraftResultKind kind,
        CraftUnavailableReason reason,
        AbstractPhysicalObject.AbstractObjectType? itemType,
        int itemData,
        CreatureTemplate.Type? creatureType)
    {
        Available = available;
        Kind = kind;
        Reason = reason;
        ItemType = itemType;
        ItemData = itemData;
        CreatureType = creatureType;
    }

    internal bool Available { get; }
    internal CraftResultKind Kind { get; }
    internal CraftUnavailableReason Reason { get; }
    internal AbstractPhysicalObject.AbstractObjectType? ItemType { get; }
    internal int ItemData { get; }
    internal CreatureTemplate.Type? CreatureType { get; }

    internal static CraftPrediction Hidden(CraftUnavailableReason reason) =>
        new(false, CraftResultKind.None, reason, null, 0, null);

    internal static CraftPrediction Item(AbstractPhysicalObject.AbstractObjectType type, int itemData) =>
        new(true, CraftResultKind.Item, CraftUnavailableReason.None, type, itemData, null);

    internal static CraftPrediction Creature(CreatureTemplate.Type type) =>
        new(true, CraftResultKind.Creature, CraftUnavailableReason.None, null, 0, type);

    internal static CraftPrediction Food() =>
        new(true, CraftResultKind.Food, CraftUnavailableReason.None, null, 0, null);

    internal IconDraw? ToIcon()
    {
        if (!Available)
        {
            return null;
        }

        if (Kind == CraftResultKind.Item && ItemType != null)
        {
            return IconLookup.FromItemType(ItemType, ItemData);
        }

        if (Kind == CraftResultKind.Creature && CreatureType != null)
        {
            return IconLookup.FromCreatureType(CreatureType);
        }

        return Kind == CraftResultKind.Food ? IconLookup.FoodResult() : null;
    }
}
