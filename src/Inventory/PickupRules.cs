namespace MoreSlugHUD;

internal enum PickupDestination
{
    None = 0,
    Left = 1,
    Right = 2,
    Back = 3,
}

internal static class PickupRules
{
    internal static PickupDestination Resolve(Player player, PhysicalObject candidate)
    {
        if (GoesToBack(player, candidate))
        {
            return PickupDestination.Back;
        }

        if (GraspEmpty(player, 0))
        {
            return PickupDestination.Left;
        }

        if (GraspEmpty(player, 1))
        {
            return PickupDestination.Right;
        }

        return PickupDestination.None;
    }

    private static bool GoesToBack(Player player, PhysicalObject candidate)
    {
        if (candidate is Spear && player.CanPutSpearToBack)
        {
            return HasGrabAtLeast(player, Player.ObjectGrabability.BigOneHand) || BothHandsOccupied(player);
        }

        if (candidate is not Player slug || !player.CanPutSlugToBack)
        {
            return false;
        }

        if (slug.dead && !player.CanIPutDeadSlugOnBack(slug))
        {
            return false;
        }

        return HasSlugBackHand(player)
            || BothHandsOccupied(player)
            || player.bodyMode == Player.BodyModeIndex.Crawl;
    }

    private static bool HasGrabAtLeast(Player player, Player.ObjectGrabability minimum)
    {
        var left = Grasp(player, 0);
        var right = Grasp(player, 1);
        return (left != null && player.Grabability(left) >= minimum)
            || (right != null && player.Grabability(right) >= minimum);
    }

    private static bool HasSlugBackHand(Player player)
    {
        return SlugBackHand(player, 0) || SlugBackHand(player, 1);
    }

    private static bool SlugBackHand(Player player, int index)
    {
        var grabbed = Grasp(player, index);
        if (grabbed == null)
        {
            return false;
        }

        return player.Grabability(grabbed) > Player.ObjectGrabability.BigOneHand || grabbed is Player;
    }

    private static bool BothHandsOccupied(Player player) =>
        Grasp(player, 0) != null && Grasp(player, 1) != null;

    internal static bool GraspEmpty(Player player, int index) => Grasp(player, index) == null;

    private static PhysicalObject? Grasp(Player player, int index)
    {
        if (player.grasps == null || index >= player.grasps.Length)
        {
            return null;
        }

        return player.grasps[index]?.grabbed;
    }
}
