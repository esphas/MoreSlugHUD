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
    internal static bool TryResolve(Player player, PhysicalObject candidate, out PickupDestination destination)
    {
        destination = PickupDestination.None;
        if (!TryGoesToBack(player, candidate, out var back))
        {
            return false;
        }

        if (back)
        {
            destination = PickupDestination.Back;
            return true;
        }

        var left = Grasp(player, 0);
        var right = Grasp(player, 1);
        var empty = (left == null ? 1 : 0) + (right == null ? 1 : 0);
        if (!PlayerGrabability.TryGet(player, candidate, out var candidateGrab))
        {
            return false;
        }

        var dropLeft = false;
        var dropRight = false;
        if (candidateGrab == Player.ObjectGrabability.TwoHands && empty < 4)
        {
            dropLeft = left != null;
            dropRight = right != null;
        }
        else if (empty == 0)
        {
            if (left is Fly)
            {
                dropLeft = true;
            }
            else if (right is Fly)
            {
                dropRight = true;
            }
        }

        if (left == null || dropLeft)
        {
            destination = PickupDestination.Left;
            return true;
        }

        if (right == null || dropRight)
        {
            destination = PickupDestination.Right;
            return true;
        }

        return true;
    }

    private static bool TryGoesToBack(Player player, PhysicalObject candidate, out bool goes)
    {
        goes = false;
        if (candidate is Spear && player.CanPutSpearToBack)
        {
            if (!TryHasGrabAtLeast(player, Player.ObjectGrabability.BigOneHand, out var has))
            {
                return false;
            }

            goes = has || BothHandsOccupied(player);
            return true;
        }

        if (candidate is not Player slug || !player.CanPutSlugToBack)
        {
            return true;
        }

        if (slug.dead && !player.CanIPutDeadSlugOnBack(slug))
        {
            return true;
        }

        if (!TrySlugBackHand(player, 0, out var left) || !TrySlugBackHand(player, 1, out var right))
        {
            return false;
        }

        goes = left
            || right
            || BothHandsOccupied(player)
            || player.bodyMode == Player.BodyModeIndex.Crawl;
        return true;
    }

    private static bool TryHasGrabAtLeast(Player player, Player.ObjectGrabability minimum, out bool has)
    {
        has = false;
        var left = Grasp(player, 0);
        var right = Grasp(player, 1);
        if (left != null)
        {
            if (!PlayerGrabability.TryGet(player, left, out var leftGrab))
            {
                return false;
            }

            if (leftGrab >= minimum)
            {
                has = true;
                return true;
            }
        }

        if (right != null)
        {
            if (!PlayerGrabability.TryGet(player, right, out var rightGrab))
            {
                return false;
            }

            if (rightGrab >= minimum)
            {
                has = true;
                return true;
            }
        }

        return true;
    }

    private static bool TrySlugBackHand(Player player, int index, out bool match)
    {
        match = false;
        var grabbed = Grasp(player, index);
        if (grabbed == null)
        {
            return true;
        }

        if (grabbed is Player)
        {
            match = true;
            return true;
        }

        if (!PlayerGrabability.TryGet(player, grabbed, out var grab))
        {
            return false;
        }

        match = grab > Player.ObjectGrabability.BigOneHand;
        return true;
    }

    private static bool BothHandsOccupied(Player player) =>
        Grasp(player, 0) != null && Grasp(player, 1) != null;

    private static PhysicalObject? Grasp(Player player, int index)
    {
        if (player.grasps == null || index >= player.grasps.Length)
        {
            return null;
        }

        return player.grasps[index]?.grabbed;
    }
}
