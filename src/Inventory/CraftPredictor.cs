namespace MoreSlugHUD;

internal static class CraftPredictor
{
    internal static CraftPrediction Predict(Player player, bool? expeditionCrafting = null)
    {
        if (player.grasps == null)
        {
            return CraftPrediction.Hidden(CraftUnavailableReason.NotApplicable);
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

        return CraftPrediction.Hidden(CraftUnavailableReason.NotApplicable);
    }

    private static CraftPrediction PredictArtificer(Player player)
    {
        if (player.FoodInStomach <= 0)
        {
            return CraftPrediction.Hidden(CraftUnavailableReason.NoFood);
        }

        foreach (var grasp in player.grasps)
        {
            if (grasp?.grabbed is IPlayerEdible edible && edible.Edible)
            {
                return CraftPrediction.Hidden(CraftUnavailableReason.HoldingEdible);
            }
        }

        if (CanCraftExplosiveSpear(player.grasps[0])
            || (player.grasps[0] == null
                && CanCraftExplosiveSpear(player.grasps[1])
                && player.objectInStomach == null))
        {
            return CraftPrediction.Item(AbstractPhysicalObject.AbstractObjectType.Spear, 1);
        }

        return CraftPrediction.Hidden(CraftUnavailableReason.NoCraftableSpear);
    }

    private static CraftPrediction PredictCombo(Player player, bool expeditionCrafting)
    {
        var type = player.CraftingResults();
        if (type == null)
        {
            return CraftPrediction.Hidden(CraftUnavailableReason.NoRecipe);
        }

        if (type == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
        {
            if (DownpourCompat.IsSpearmaster(player) && expeditionCrafting)
            {
                return CraftPrediction.Hidden(CraftUnavailableReason.SpearmasterFoodBlocked);
            }

            return CraftPrediction.Food();
        }

        if (type == AbstractPhysicalObject.AbstractObjectType.Creature)
        {
            var crit = DownpourCompat.ComboCreature(player.grasps[0], player.grasps[1]);
            return crit == null
                ? CraftPrediction.Hidden(CraftUnavailableReason.UnknownCreature)
                : CraftPrediction.Creature(crit);
        }

        return CraftPrediction.Item(type, 0);
    }

    private static bool CanCraftExplosiveSpear(Creature.Grasp? grasp) =>
        grasp?.grabbed is Spear spear
        && spear.abstractSpear != null
        && !spear.abstractSpear.explosive
        && !(spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0);
}
