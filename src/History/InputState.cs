using System;

namespace MoreSlugHUD;

internal readonly struct InputState : IEquatable<InputState>
{
    internal readonly sbyte X;
    internal readonly sbyte Y;
    internal readonly bool Jump;
    internal readonly bool Throw;
    internal readonly bool Pickup;
    internal readonly bool Special;
    internal readonly MovementTags Movement;

    internal InputState(Player.InputPackage input, MovementTags movement)
    {
        if (InputHistoryConfig.TrackDirection)
        {
            X = (sbyte)ClampDirection(input.x);
            Y = (sbyte)ClampDirection(input.y);
        }
        else
        {
            X = 0;
            Y = 0;
        }

        var actions = InputHistoryConfig.TrackActions;
        Jump = actions && input.jmp;
        Throw = actions && input.thrw;
        Pickup = actions && input.pckp;
        Special = actions && input.spec;
        Movement = movement & InputHistoryConfig.TrackedStateMask;
    }

    public bool Equals(InputState other) =>
        X == other.X && Y == other.Y
        && Jump == other.Jump && Throw == other.Throw
        && Pickup == other.Pickup && Special == other.Special
        && Movement == other.Movement;

    public override bool Equals(object? obj) => obj is InputState other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (int)X;
            hash = hash * 31 + Y;
            hash = hash * 31 + (Jump ? 1 : 0);
            hash = hash * 31 + (Throw ? 1 : 0);
            hash = hash * 31 + (Pickup ? 1 : 0);
            hash = hash * 31 + (Special ? 1 : 0);
            return hash * 31 + (int)Movement;
        }
    }

    public override string ToString() =>
        $"xy={X},{Y} jmp={Jump} thrw={Throw} pckp={Pickup} spec={Special} tags={InputHistoryFormatter.FormatTags(Movement)}";

    private static int ClampDirection(int value) => value < 0 ? -1 : value > 0 ? 1 : 0;
}
