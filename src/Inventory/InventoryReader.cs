using System;
using System.Reflection;

namespace MoreSlugHUD;

internal static class InventoryReader
{
    private static FieldInfo? _pickUpCandidate;
    private static bool _pickUpResolved;

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
        snapshot.Pyro = ReadPyro(player);
        try
        {
            ApplyTwoHandedHold(player, ref snapshot.Left, ref snapshot.Right, fails);
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("inventory-twohand", "two-hand hold failed", exception);
        }

        try
        {
            ApplyPickUpCandidate(player, snapshot, fails);
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("inventory-pickup", "pick-up candidate failed", exception);
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
            var prediction = CraftPredictor.Predict(player, expeditionCrafting);
            LogRecover(SlotId.Craft, fails.CraftSucceeded());
            var icon = prediction.ToIcon();
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

    private static SlotContent ReadPyro(Player player)
    {
        try
        {
            if (!MoreSlugHUDConfig.ShowPyro || !DownpourCompat.HasPyroMechanics(player))
            {
                return SlotContent.Hidden(SlotId.Pyro);
            }

            return new SlotContent
            {
                Id = SlotId.Pyro,
                OccupiesLayout = true,
                ShowPlaceholder = false,
                ContentAlpha = 1f,
                PyroHeat = player.pyroJumpCounter,
                PyroCapacity = DownpourCompat.ExplosionCapacity(),
                PyroCooldown = player.pyroJumpCooldown,
            };
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("inventory-pyro", "pyro slot failed", exception);
            return SlotContent.Hidden(SlotId.Pyro);
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

    private static void ApplyPickUpCandidate(Player player, InventorySnapshot snapshot, InventoryFailState fails)
    {
        if (!MoreSlugHUDConfig.ShowPickUpCandidate || player.inShortcut)
        {
            return;
        }

        var candidate = ReadPickUpCandidate(player);
        var held = candidate?.abstractPhysicalObject;
        if (candidate == null || held == null || AlreadyHeld(player, candidate, held))
        {
            return;
        }

        if (!PickupRules.TryResolve(player, candidate, out var destination))
        {
            return;
        }

        switch (destination)
        {
            case PickupDestination.Left:
                FillPickUp(ref snapshot.Left, held, fails);
                return;
            case PickupDestination.Right:
                FillPickUp(ref snapshot.Right, held, fails);
                return;
            case PickupDestination.Back:
                FillPickUp(ref snapshot.Back, held, fails);
                return;
        }
    }

    private static PhysicalObject? ReadPickUpCandidate(Player player)
    {
        if (!_pickUpResolved)
        {
            _pickUpResolved = true;
            try
            {
                _pickUpCandidate = typeof(Player).GetField(
                    "pickUpCandidate",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }
            catch (Exception exception)
            {
                MoreSlugHUDLog.ErrorOnce("inventory-pickup-field", "pick-up candidate field missing", exception);
                return null;
            }

            if (_pickUpCandidate == null)
            {
                MoreSlugHUDLog.ErrorOnce("inventory-pickup-field", "pick-up candidate field missing");
                return null;
            }
        }

        if (_pickUpCandidate == null)
        {
            return null;
        }

        return _pickUpCandidate.GetValue(player) as PhysicalObject;
    }

    private static bool CanAcceptPreview(SlotContent slot) =>
        slot.OccupiesLayout && (slot.Id != SlotId.Back || !slot.HasContent);

    private static bool AlreadyHeld(Player player, PhysicalObject candidate, AbstractPhysicalObject held)
    {
        if (SameGrasp(player, 0, candidate, held) || SameGrasp(player, 1, candidate, held))
        {
            return true;
        }

        if (ReferenceEquals(player.objectInStomach, held))
        {
            return true;
        }

        var spear = player.spearOnBack?.spear;
        if (SameObject(candidate, held, spear, spear?.abstractPhysicalObject))
        {
            return true;
        }

        var slug = player.slugOnBack?.slugcat;
        if (SameObject(candidate, held, slug, slug?.abstractPhysicalObject))
        {
            return true;
        }

        var lizard = LizardOnBackAdapter.TryGetLizard(player);
        return lizard != null && ReferenceEquals(lizard, held);
    }

    private static bool SameGrasp(Player player, int index, PhysicalObject candidate, AbstractPhysicalObject held)
    {
        if (player.grasps == null || index >= player.grasps.Length)
        {
            return false;
        }

        var grabbed = player.grasps[index]?.grabbed;
        return SameObject(candidate, held, grabbed, grabbed?.abstractPhysicalObject);
    }

    private static bool SameObject(
        PhysicalObject candidate,
        AbstractPhysicalObject held,
        PhysicalObject? other,
        AbstractPhysicalObject? otherHeld)
    {
        return ReferenceEquals(candidate, other) || ReferenceEquals(held, otherHeld);
    }

    private static void FillPickUp(ref SlotContent slot, AbstractPhysicalObject held, InventoryFailState fails)
    {
        if (!CanAcceptPreview(slot) || fails.ShouldSkipPickup(slot.Id, held))
        {
            return;
        }

        try
        {
            var icon = IconLookup.FromObject(held);
            LogRecover(slot.Id, fails.PickupSucceeded(slot.Id));
            if (!icon.HasValue)
            {
                return;
            }

            var filled = Occupied(slot.Id, icon.Value, MoreSlugHUDConfig.PickUpCandidateAlpha);
            filled.IsPickUpCandidate = true;
            slot = filled;
        }
        catch (Exception exception)
        {
            if (fails.PickupFailed(slot.Id, held))
            {
                MoreSlugHUDLog.Error($"inventory pick-up {slot.Id} failed", exception);
            }
        }
    }
}
