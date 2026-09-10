using System.Collections.Generic;
using UnityEngine;

namespace MoreSlugHUD;

internal static class IdFamiliarity
{
    private static readonly Dictionary<WatchKey, float> Watch = new();

    internal static void Reset()
    {
        Watch.Clear();
        IdObservers.Clear();
    }

    internal static void TickWatch(Creature creature)
    {
        var game = creature.room?.game ?? creature.abstractCreature?.world?.game;
        IdObservers.Refresh(game);
        if (creature.abstractCreature == null)
        {
            return;
        }

        var creatureKey = Key(creature.abstractCreature.ID);
        for (var i = 0; i < IdObservers.PlayerCount; i++)
        {
            var viewer = IdObservers.PlayerAt(i);
            if (!IdObservers.IsWatchedBy(creature, viewer))
            {
                continue;
            }

            var key = new WatchKey(creatureKey.Spawner, creatureKey.Number, PlayerKey(viewer));
            Watch.TryGetValue(key, out var seconds);
            var cap = MoreSlugHUDConfig.IdWatchSeconds;
            Watch[key] = cap <= 0f ? 1f : Mathf.Min(cap, seconds + 1f / 40f);
        }
    }

    internal static bool ShouldShow(Creature creature, Player viewer)
    {
        if (creature.dead && !MoreSlugHUDConfig.ShowDead)
        {
            return false;
        }

        if (MoreSlugHUDConfig.FamiliarityThreshold <= 0f)
        {
            return true;
        }

        return Score(creature, viewer) + 0.0001f >= MoreSlugHUDConfig.FamiliarityThreshold;
    }

    private static float Score(Creature creature, Player viewer)
    {
        return Mathf.Max(WatchScore(creature, viewer), SocialScore(creature, viewer), RecognizedScore(creature, viewer));
    }

    private static float WatchScore(Creature creature, Player viewer)
    {
        if (creature.abstractCreature == null)
        {
            return 0f;
        }

        var creatureId = Key(creature.abstractCreature.ID);
        var key = new WatchKey(creatureId.Spawner, creatureId.Number, PlayerKey(viewer));
        if (!Watch.TryGetValue(key, out var seconds))
        {
            return 0f;
        }

        var cap = MoreSlugHUDConfig.IdWatchSeconds;
        if (cap <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(seconds / cap);
    }

    private static float SocialScore(Creature creature, Player viewer)
    {
        var memory = creature.State?.socialMemory;
        var apo = viewer.abstractCreature;
        if (memory == null || apo == null)
        {
            return 0f;
        }

        var rel = memory.GetRelationship(apo.ID);
        if (rel == null)
        {
            return 0f;
        }

        var impact = Mathf.Max(
            Mathf.Abs(rel.like),
            Mathf.Abs(rel.tempLike),
            Mathf.Abs(rel.fear),
            Mathf.Abs(rel.tempFear));
        return Mathf.Clamp01(0.55f * impact + 0.45f * rel.know);
    }

    private static float RecognizedScore(Creature creature, Player viewer)
    {
        if (creature is Player pup && (pup.isNPC || pup.isSlugpup))
        {
            var friend = pup.abstractCreature.abstractAI?.RealAI?.friendTracker?.friend;
            return ReferenceEquals(friend, viewer) ? 1f : 0f;
        }

        if (creature.Template is not { IsLizard: true })
        {
            return 0f;
        }

        var memory = creature.State?.socialMemory;
        var apo = viewer.abstractCreature;
        if (memory == null || apo == null)
        {
            return 0f;
        }

        return memory.GetLike(apo.ID) > 0.5f ? 1f : 0f;
    }

    internal static bool IsLocalSlugcat(Player player)
    {
        if (player.isNPC || player.isSlugpup)
        {
            return false;
        }

        if (!MeadowOwnership.IsMeadowActive)
        {
            return true;
        }

        return MeadowOwnership.TryIsLocal(player, out var isLocal) && isLocal;
    }

    private static (int Spawner, int Number) Key(EntityID id) => (id.spawner, id.number);

    private static (int Spawner, int Number) PlayerKey(Player player)
    {
        var apo = player.abstractCreature;
        return apo == null ? (0, player.playerState?.playerNumber ?? 0) : (apo.ID.spawner, apo.ID.number);
    }

    private readonly struct WatchKey
    {
        internal WatchKey(int creatureSpawner, int creatureNumber, (int Spawner, int Number) player)
        {
            CreatureSpawner = creatureSpawner;
            CreatureNumber = creatureNumber;
            PlayerSpawner = player.Spawner;
            PlayerNumber = player.Number;
        }

        internal int CreatureSpawner { get; }
        internal int CreatureNumber { get; }
        internal int PlayerSpawner { get; }
        internal int PlayerNumber { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = CreatureSpawner;
                hash = hash * 31 + CreatureNumber;
                hash = hash * 31 + PlayerSpawner;
                return hash * 31 + PlayerNumber;
            }
        }

        public override bool Equals(object? obj) => obj is WatchKey other && Equals(other);

        public bool Equals(WatchKey other) =>
            CreatureSpawner == other.CreatureSpawner
            && CreatureNumber == other.CreatureNumber
            && PlayerSpawner == other.PlayerSpawner
            && PlayerNumber == other.PlayerNumber;
    }
}
