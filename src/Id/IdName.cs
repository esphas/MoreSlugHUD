namespace MoreSlugHUD;

internal enum IdNameKind
{
    Lizard,
    Scavenger,
    Slugpup,
}

internal static class IdName
{
    internal const int Space = -1;
    internal const int MaxLength = 7;

    private static readonly int[] Vowels = [0, 1, 3, 5, 7, 9, 10, 12, 13];
    private static readonly int[] Consonants = [30, 31, 32, 33, 34, 35, 38, 39, 40, 41, 42, 43, 44, 45, 46, 48];

    private static readonly string[] PupWords = ["CV", "VCV", "CVC"];
    private static readonly string[] PupNames = ["CV CV"];
    private static readonly string[] LizardWords = ["CVC", "VCV", "CVCV", "VCVC", "CVCVC", "VCVCV"];
    private static readonly string[] LizardNames = ["CV CVC", "CVC CV"];
    private static readonly string[] ScavengerWords = ["CVCV", "VCVC", "CVCC", "CVCVC", "CVCCV", "CCVCV"];
    private static readonly string[] ScavengerNames = ["CV CVC", "CVC CV", "CVC CVC"];

    internal static int[] For(Creature creature)
    {
        if (creature.abstractCreature == null || !TryKind(creature, out var kind))
        {
            return [];
        }

        var id = creature.abstractCreature.ID;
        var type = creature.Template?.type.Index ?? 0;
        return Generate(id.spawner, id.number, type, kind);
    }

    internal static int[] Generate(int spawner, int number, int typeIndex, IdNameKind kind)
    {
        var rng = new Rng(spawner, number, typeIndex * 17 + (int)kind * 1021);
        var twoWord = rng.Chance(TwoWordChance(kind));
        var template = Pick(rng, twoWord ? Names(kind) : Words(kind));
        var glyphs = new int[template.Length];
        for (var i = 0; i < template.Length; i++)
        {
            glyphs[i] = template[i] switch
            {
                'V' => Vowels[rng.Next(Vowels.Length)],
                'C' => Consonants[rng.Next(Consonants.Length)],
                _ => Space,
            };
        }

        return glyphs;
    }

    internal static bool TryKind(Creature creature, out IdNameKind kind)
    {
        if (creature.Template is { IsLizard: true })
        {
            kind = IdNameKind.Lizard;
            return true;
        }

        if (creature is Scavenger)
        {
            kind = IdNameKind.Scavenger;
            return true;
        }

        if (creature is Player player && (player.isNPC || player.isSlugpup))
        {
            kind = IdNameKind.Slugpup;
            return true;
        }

        kind = default;
        return false;
    }

    private static float TwoWordChance(IdNameKind kind) => kind switch
    {
        IdNameKind.Slugpup => 0.05f,
        IdNameKind.Lizard => 0.10f,
        _ => 0.18f,
    };

    private static string[] Words(IdNameKind kind) => kind switch
    {
        IdNameKind.Slugpup => PupWords,
        IdNameKind.Lizard => LizardWords,
        _ => ScavengerWords,
    };

    private static string[] Names(IdNameKind kind) => kind switch
    {
        IdNameKind.Slugpup => PupNames,
        IdNameKind.Lizard => LizardNames,
        _ => ScavengerNames,
    };

    private static string Pick(Rng rng, string[] pool) => pool[rng.Next(pool.Length)];

    private struct Rng
    {
        private uint _state;

        internal Rng(int spawner, int number, int salt)
        {
            unchecked
            {
                _state = (uint)(spawner * -1640531527 + number * 265443576 + salt * 1597334677);
            }

            if (_state == 0)
            {
                _state = 0xA341316Cu;
            }
        }

        internal int Next(int max)
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)max);
        }

        internal bool Chance(float probability) => Next(10000) < (int)(probability * 10000f);
    }
}
