namespace MoreSlugHUD;

internal sealed class IdLabelModel
{
    internal readonly int[] Glyphs;
    private readonly IdIntentStates _intents = new();
    private int _numberValue = int.MinValue;
    private string _numberText = "0";
    private uint _shakeRng;

    internal IdLabelModel(Creature creature)
    {
        Glyphs = IdName.For(creature);
        var id = creature.abstractPhysicalObject?.ID;
        _shakeRng = (uint)((id?.spawner ?? 0) * -1640531527 + (id?.number ?? 0) * 265443576 + 0x9E3779B9);
        if (_shakeRng == 0)
        {
            _shakeRng = 0xA341316Cu;
        }
    }

    internal int IntentCount => _intents.Count;

    internal string NumberText(Creature creature)
    {
        var number = creature.abstractPhysicalObject?.ID.number ?? 0;
        if (number != _numberValue)
        {
            _numberValue = number;
            _numberText = number.ToString();
        }

        return _numberText;
    }

    internal bool CycleShowsNumber(Creature creature)
    {
        var clock = creature.room?.game?.clock ?? 0;
        var number = creature.abstractPhysicalObject?.ID.number ?? 0;
        var phase = (clock / MoreSlugHUDConfig.IdCycleTicks + number) % 2;
        if (phase < 0)
        {
            phase += 2;
        }

        return phase == 0;
    }

    internal void Tick(Creature creature)
    {
        IdFamiliarity.TickWatch(creature);
        if (!MoreSlugHUDConfig.ShowIntent || creature.dead)
        {
            _intents.Clear();
            return;
        }

        for (var i = 0; i < IdObservers.PlayerCount; i++)
        {
            var viewer = IdObservers.PlayerAt(i);
            _intents.Tick(ViewerKey(viewer), IdIntent.Read(creature, viewer), ref _shakeRng);
        }
    }

    internal IdIntentDrawState PeekIntent(Player viewer) => _intents.Peek(ViewerKey(viewer));

    internal static (int Spawner, int Number) ViewerKey(Player viewer)
    {
        var apo = viewer.abstractCreature;
        return apo == null
            ? (0, viewer.playerState?.playerNumber ?? 0)
            : (apo.ID.spawner, apo.ID.number);
    }
}
