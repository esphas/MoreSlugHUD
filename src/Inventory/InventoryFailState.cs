using UnityEngine;

namespace MoreSlugHUD;

internal sealed class InventoryFailState
{
    private const float SlotRetrySeconds = 2f;

    internal readonly AbstractPhysicalObject?[] Objects = new AbstractPhysicalObject?[5];
    private readonly float[] _heldRetryAt = new float[5];
    private readonly bool[] _heldFailing = new bool[5];
    private readonly int[] _heldAttempts = new int[5];
    private readonly float[] _heldSince = new float[5];

    private int _craftAttempts;
    private float _craftSince;
    private bool _craftBroken;
    private AbstractPhysicalObject? _craftLeft;
    private AbstractPhysicalObject? _craftRight;
    private AbstractPhysicalObject? _craftStomach;
    private int _craftExtra;
    private SlugcatStats.Name? _craftClass;
    private float _craftRetryAt;

    private int _backAttempts;
    private float _backSince;
    private bool _backBroken;
    private AbstractPhysicalObject? _backSpear;
    private AbstractPhysicalObject? _backSlug;
    private AbstractPhysicalObject? _backLizard;
    private float _backRetryAt;

    internal void Reset()
    {
        for (var i = 0; i < Objects.Length; i++)
        {
            Objects[i] = null;
            _heldRetryAt[i] = 0f;
            _heldFailing[i] = false;
            _heldAttempts[i] = 0;
            _heldSince[i] = 0f;
        }

        _craftBroken = false;
        _craftAttempts = 0;
        _craftSince = 0f;
        _craftLeft = null;
        _craftRight = null;
        _craftStomach = null;
        _craftExtra = 0;
        _craftClass = null;
        _craftRetryAt = 0f;
        _backBroken = false;
        _backAttempts = 0;
        _backSince = 0f;
        _backSpear = null;
        _backSlug = null;
        _backLizard = null;
        _backRetryAt = 0f;
    }

    internal bool ShouldSkipHeld(SlotId id, AbstractPhysicalObject? held)
    {
        if (held == null)
        {
            return false;
        }

        var index = (int)id;
        if (!ReferenceEquals(Objects[index], held))
        {
            return false;
        }

        return Time.realtimeSinceStartup < _heldRetryAt[index];
    }

    internal FailSummary HeldSucceeded(SlotId id)
    {
        var index = (int)id;
        var summary = Summary(_heldFailing[index], _heldAttempts[index], _heldSince[index]);
        Objects[index] = null;
        _heldRetryAt[index] = 0f;
        _heldFailing[index] = false;
        _heldAttempts[index] = 0;
        _heldSince[index] = 0f;
        return summary;
    }

    internal bool HeldFailed(SlotId id, AbstractPhysicalObject? held)
    {
        var index = (int)id;
        var log = !_heldFailing[index] || !ReferenceEquals(Objects[index], held);
        if (!_heldFailing[index])
        {
            _heldSince[index] = Time.realtimeSinceStartup;
            _heldAttempts[index] = 0;
        }

        Objects[index] = held;
        _heldRetryAt[index] = Time.realtimeSinceStartup + SlotRetrySeconds;
        _heldFailing[index] = true;
        _heldAttempts[index]++;
        return log;
    }

    internal bool ShouldSkipCraft(Player player, bool expeditionCrafting)
    {
        if (!_craftBroken)
        {
            return false;
        }

        if (!SameCraft(player, expeditionCrafting))
        {
            return false;
        }

        return Time.realtimeSinceStartup < _craftRetryAt;
    }

    internal FailSummary CraftSucceeded()
    {
        var summary = Summary(_craftBroken, _craftAttempts, _craftSince);
        _craftBroken = false;
        _craftRetryAt = 0f;
        _craftAttempts = 0;
        _craftSince = 0f;
        return summary;
    }

    internal bool CraftFailed(Player player, bool expeditionCrafting)
    {
        var log = !_craftBroken || !SameCraft(player, expeditionCrafting);
        if (!_craftBroken)
        {
            _craftSince = Time.realtimeSinceStartup;
            _craftAttempts = 0;
        }

        _craftBroken = true;
        _craftAttempts++;
        _craftLeft = Grasp(player, 0);
        _craftRight = Grasp(player, 1);
        _craftStomach = player.objectInStomach;
        _craftExtra = CraftExtra(player, expeditionCrafting);
        _craftClass = player.SlugCatClass;
        _craftRetryAt = Time.realtimeSinceStartup + SlotRetrySeconds;
        return log;
    }

    internal bool ShouldSkipBack(
        AbstractPhysicalObject? spear,
        AbstractPhysicalObject? slug,
        AbstractPhysicalObject? lizard)
    {
        if (!_backBroken)
        {
            return false;
        }

        if (!ReferenceEquals(_backSpear, spear)
            || !ReferenceEquals(_backSlug, slug)
            || !ReferenceEquals(_backLizard, lizard))
        {
            return false;
        }

        return Time.realtimeSinceStartup < _backRetryAt;
    }

    internal FailSummary BackSucceeded()
    {
        var summary = Summary(_backBroken, _backAttempts, _backSince);
        _backBroken = false;
        _backRetryAt = 0f;
        _backAttempts = 0;
        _backSince = 0f;
        return summary;
    }

    internal bool BackFailed(
        AbstractPhysicalObject? spear,
        AbstractPhysicalObject? slug,
        AbstractPhysicalObject? lizard)
    {
        var log = !_backBroken
            || !ReferenceEquals(_backSpear, spear)
            || !ReferenceEquals(_backSlug, slug)
            || !ReferenceEquals(_backLizard, lizard);
        if (!_backBroken)
        {
            _backSince = Time.realtimeSinceStartup;
            _backAttempts = 0;
        }

        _backBroken = true;
        _backAttempts++;
        _backSpear = spear;
        _backSlug = slug;
        _backLizard = lizard;
        _backRetryAt = Time.realtimeSinceStartup + SlotRetrySeconds;
        return log;
    }

    private bool SameCraft(Player player, bool expeditionCrafting) =>
        ReferenceEquals(_craftLeft, Grasp(player, 0))
        && ReferenceEquals(_craftRight, Grasp(player, 1))
        && ReferenceEquals(_craftStomach, player.objectInStomach)
        && _craftExtra == CraftExtra(player, expeditionCrafting)
        && Equals(_craftClass, player.SlugCatClass);

    private static AbstractPhysicalObject? Grasp(Player player, int index)
    {
        if (player.grasps == null || index >= player.grasps.Length)
        {
            return null;
        }

        return player.grasps[index]?.grabbed?.abstractPhysicalObject;
    }

    private static int CraftExtra(Player player, bool expeditionCrafting)
    {
        unchecked
        {
            var extra = player.FoodInStomach;
            extra = extra * 397 + SpearCharge(player, 0);
            extra = extra * 397 + SpearCharge(player, 1);
            extra = extra * 397 + (expeditionCrafting ? 1 : 0);
            return extra;
        }
    }

    internal readonly struct FailSummary
    {
        internal FailSummary(int attempts, float seconds)
        {
            Attempts = attempts;
            Seconds = seconds;
        }

        internal int Attempts { get; }
        internal float Seconds { get; }
        internal bool ShouldLog => Attempts >= 2;
    }

    private static FailSummary Summary(bool failing, int attempts, float since)
    {
        if (!failing)
        {
            return default;
        }

        return new FailSummary(attempts, Time.realtimeSinceStartup - since);
    }

    private static int SpearCharge(Player player, int index)
    {
        if (player.grasps == null || index >= player.grasps.Length)
        {
            return -1;
        }

        return player.grasps[index]?.grabbed is Spear spear
            ? spear.abstractSpear?.electricCharge ?? 0
            : -1;
    }
}
