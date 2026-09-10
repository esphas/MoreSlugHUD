using System;

namespace MoreSlugHUD;

internal static class InventoryReader
{
    internal static void Read(Player player, InventorySnapshot snapshot, InventoryFailState fails)
    {
        var expedition = MoreSlugHUDConfig.ShowCraft
            && !DownpourCompat.IsArtificer(player)
            && !DownpourCompat.IsGourmand(player)
            && DownpourCompat.HasExpeditionCrafting();
        snapshot.Craft = ReadCraft(player, fails, expedition);
        snapshot.Left = ReadGrasp(player, SlotId.Left, 0, fails);
        snapshot.Right = ReadGrasp(player, SlotId.Right, 1, fails);
        snapshot.Stomach = ReadStomach(player, fails);
        snapshot.Back = ReadBack(player, fails);
        try
        {
            ApplyTwoHandedHold(player, ref snapshot.Left, ref snapshot.Right, fails);
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("inventory-twohand", "two-hand hold failed", exception);
        }
    }

    private static SlotContent ReadCraft(Player player, InventoryFailState fails, bool expeditionCrafting)
    {
        if (!MoreSlugHUDConfig.ShowCraft)
        {
            return SlotContent.Hidden(SlotId.Craft);
        }

        if (fails.ShouldSkipCraft(player, expeditionCrafting))
        {
            return Fallback(SlotId.Craft);
        }

        try
        {
            var icon = CraftPredictor.Predict(player, expeditionCrafting);
            LogRecover(SlotId.Craft, fails.CraftSucceeded());
            if (!icon.HasValue)
            {
                return SlotContent.Hidden(SlotId.Craft);
            }

            return Occupied(SlotId.Craft, icon.Value, MoreSlugHUDConfig.MainHandAlpha);
        }
        catch (Exception exception)
        {
            if (fails.CraftFailed(player, expeditionCrafting))
            {
                MoreSlugHUDLog.Error("inventory slot Craft failed", exception);
            }

            return Fallback(SlotId.Craft);
        }
    }

    private static SlotContent ReadGrasp(Player player, SlotId id, int index, InventoryFailState fails)
    {
        if (!MoreSlugHUDConfig.IsSlotEnabled(id))
        {
            return SlotContent.Hidden(id);
        }

        AbstractPhysicalObject? held = null;
        if (player.grasps != null && index < player.grasps.Length)
        {
            held = player.grasps[index]?.grabbed?.abstractPhysicalObject;
        }

        return FromObject(id, held, MoreSlugHUDConfig.MainHandAlpha, fails);
    }

    private static SlotContent ReadStomach(Player player, InventoryFailState fails)
    {
        if (!MoreSlugHUDConfig.ShowStomach)
        {
            return SlotContent.Hidden(SlotId.Stomach);
        }

        var held = player.objectInStomach ?? DownpourCompat.TryNpcStomach(player);
        return FromObject(SlotId.Stomach, held, MoreSlugHUDConfig.MainHandAlpha, fails);
    }

    private static SlotContent ReadBack(Player player, InventoryFailState fails)
    {
        if (!MoreSlugHUDConfig.ShowBack)
        {
            return SlotContent.Hidden(SlotId.Back);
        }

        var spear = player.spearOnBack?.spear?.abstractPhysicalObject;
        var slug = player.slugOnBack?.slugcat?.abstractPhysicalObject;
        var lizard = LizardOnBackAdapter.TryGetLizard(player);
        if (fails.ShouldSkipBack(spear, slug, lizard))
        {
            return Fallback(SlotId.Back);
        }

        try
        {
            var content = default(SlotContent);
            content.Id = SlotId.Back;
            content.OccupiesLayout = true;
            content.ShowPlaceholder = false;
            content.ContentAlpha = MoreSlugHUDConfig.MainHandAlpha;
            AddBack(ref content, spear);
            AddBack(ref content, slug);
            AddBack(ref content, lizard);
            LogRecover(SlotId.Back, fails.BackSucceeded());
            if (content.StackCount == 0)
            {
                return FromObject(SlotId.Back, null, MoreSlugHUDConfig.MainHandAlpha, fails);
            }

            return content;
        }
        catch (Exception exception)
        {
            if (fails.BackFailed(spear, slug, lizard))
            {
                MoreSlugHUDLog.Error("inventory slot Back failed", exception);
            }

            return Fallback(SlotId.Back);
        }
    }

    private static void AddBack(ref SlotContent content, AbstractPhysicalObject? held)
    {
        if (content.StackCount >= MoreSlugHUDConfig.MaxStack)
        {
            return;
        }

        var icon = IconLookup.FromObject(held);
        if (!icon.HasValue)
        {
            return;
        }

        SetIcon(ref content, content.StackCount, icon.Value);
        content.StackCount++;
    }

    private static SlotContent FromObject(SlotId id, AbstractPhysicalObject? held, float alpha, InventoryFailState fails)
    {
        if (fails.ShouldSkipHeld(id, held))
        {
            return Fallback(id);
        }

        try
        {
            var icon = IconLookup.FromObject(held);
            LogRecover(id, fails.HeldSucceeded(id));
            if (icon.HasValue)
            {
                return Occupied(id, icon.Value, alpha);
            }

            return MoreSlugHUDConfig.ShowEmpty(id) ? Empty(id) : SlotContent.Hidden(id);
        }
        catch (Exception exception)
        {
            return Failed(id, held, fails, exception);
        }
    }

    private static SlotContent Occupied(SlotId id, IconDraw icon, float alpha)
    {
        var content = new SlotContent
        {
            Id = id,
            OccupiesLayout = true,
            ShowPlaceholder = false,
            ContentAlpha = alpha,
            StackCount = 1,
        };
        content.Icon0 = icon;
        return content;
    }

    private static SlotContent Empty(SlotId id) => new()
    {
        Id = id,
        OccupiesLayout = true,
        ShowPlaceholder = true,
        ContentAlpha = MoreSlugHUDConfig.EmptyAlpha,
    };

    private static SlotContent Failed(SlotId id, AbstractPhysicalObject? held, InventoryFailState fails, Exception exception)
    {
        if (fails.HeldFailed(id, held))
        {
            MoreSlugHUDLog.Error($"inventory slot {id} failed", exception);
        }

        return Fallback(id);
    }

    private static SlotContent Fallback(SlotId id) =>
        MoreSlugHUDConfig.ShowEmpty(id) ? Empty(id) : SlotContent.Hidden(id);

    private static void LogRecover(SlotId id, InventoryFailState.FailSummary summary)
    {
#if DEBUG
        if (summary.ShouldLog)
        {
            MoreSlugHUDLog.Info($"inventory slot {id} recovered after {summary.Attempts} failures in {summary.Seconds:0.0}s");
        }
#else
        _ = id;
        _ = summary;
#endif
    }

    private static void SetIcon(ref SlotContent content, int index, IconDraw icon)
    {
        switch (index)
        {
            case 0:
                content.Icon0 = icon;
                break;
            case 1:
                content.Icon1 = icon;
                break;
            default:
                content.Icon2 = icon;
                break;
        }
    }

    private static void ApplyTwoHandedHold(Player player, ref SlotContent left, ref SlotContent right, InventoryFailState fails)
    {
        if (player.grasps == null)
        {
            return;
        }

        var leftObj = player.grasps.Length > 0 ? player.grasps[0]?.grabbed : null;
        var rightObj = player.grasps.Length > 1 ? player.grasps[1]?.grabbed : null;
        if (leftObj != null && rightObj != null && ReferenceEquals(leftObj, rightObj))
        {
            if (left.HasContent)
            {
                left.ContentAlpha = MoreSlugHUDConfig.MainHandAlpha;
            }

            if (right.HasContent)
            {
                right.ContentAlpha = MoreSlugHUDConfig.OffHandAlpha;
            }

            return;
        }

        if (leftObj != null && rightObj == null && player.HeavyCarry(leftObj))
        {
            FillSharedOffHand(SlotId.Right, leftObj.abstractPhysicalObject, ref right, fails);
            return;
        }

        if (rightObj != null && leftObj == null && player.HeavyCarry(rightObj))
        {
            FillSharedOffHand(SlotId.Left, rightObj.abstractPhysicalObject, ref left, fails);
        }
    }

    private static void FillSharedOffHand(SlotId id, AbstractPhysicalObject? held, ref SlotContent slot, InventoryFailState fails)
    {
        if (!MoreSlugHUDConfig.IsSlotEnabled(id) || slot.HasContent)
        {
            return;
        }

        slot = FromObject(id, held, MoreSlugHUDConfig.OffHandAlpha, fails);
        if (slot.HasContent)
        {
            slot.ContentAlpha = MoreSlugHUDConfig.OffHandAlpha;
        }
    }
}
