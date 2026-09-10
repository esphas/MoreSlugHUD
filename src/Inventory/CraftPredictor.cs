namespace MoreSlugHUD;

internal static class CraftPredictor
{
    internal static IconDraw? Predict(Player player, bool? expeditionCrafting = null)
    {
        if (player.grasps == null)
        {
            return null;
        }

        if (DownpourCompat.IsArtificer(player))
        {
            return PredictArtificer(player);
        }

        var expedition = expeditionCrafting ?? DownpourCompat.HasExpeditionCrafting();
        if (DownpourCompat.IsGourmand(player) || expedition)
        {
            return PredictCombo(player, expedition);
        }

        return null;
    }

    private static IconDraw? PredictArtificer(Player player)
    {
        if (player.FoodInStomach <= 0)
        {
            return null;
        }

        foreach (var grasp in player.grasps)
        {
            if (grasp?.grabbed is IPlayerEdible edible && edible.Edible)
            {
                return null;
            }
        }

        if (CanCraftExplosiveSpear(player.grasps[0])
            || (player.grasps[0] == null
                && CanCraftExplosiveSpear(player.grasps[1])
                && player.objectInStomach == null))
        {
            return IconLookup.FromItemType(AbstractPhysicalObject.AbstractObjectType.Spear, 1);
        }

        return null;
    }

    private static IconDraw? PredictCombo(Player player, bool expeditionCrafting)
    {
        var type = player.CraftingResults();
        if (type == null)
        {
            return null;
        }

        if (type == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
        {
            if (DownpourCompat.IsSpearmaster(player) && expeditionCrafting)
            {
                return null;
            }

            return IconLookup.FoodResult();
        }

        if (type == AbstractPhysicalObject.AbstractObjectType.Creature)
        {
            var crit = DownpourCompat.ComboCreature(player.grasps[0], player.grasps[1]);
            return crit == null ? null : IconLookup.FromCreatureType(crit);
        }

        return IconLookup.FromItemType(type, 0);
    }

    private static bool CanCraftExplosiveSpear(Creature.Grasp? grasp) =>
        grasp?.grabbed is Spear spear
        && spear.abstractSpear != null
        && !spear.abstractSpear.explosive
        && !(spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0);
}
