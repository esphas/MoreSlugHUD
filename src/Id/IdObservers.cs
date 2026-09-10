using System;

namespace MoreSlugHUD;

internal readonly struct IdObserver
{
    internal IdObserver(Player player, RoomCamera? camera)
    {
        Player = player;
        Camera = camera;
    }

    internal Player Player { get; }
    internal RoomCamera? Camera { get; }
}

internal static class IdObservers
{
    private static IdObserver[] _cameras = new IdObserver[4];
    private static Player[] _players = new Player[4];
    private static int _cameraCount;
    private static int _playerCount;
    private static int _clock = int.MinValue;
    private static RainWorldGame? _game;

    internal static int CameraCount => _cameraCount;

    internal static int PlayerCount => _playerCount;

    internal static IdObserver CameraAt(int index) => _cameras[index];

    internal static Player PlayerAt(int index) => _players[index];

    internal static void Refresh(RainWorldGame? game)
    {
        if (game == null)
        {
            Clear();
            return;
        }

        if (ReferenceEquals(game, _game) && game.clock == _clock)
        {
            return;
        }

        _game = game;
        _clock = game.clock;
        _cameraCount = 0;
        _playerCount = 0;
        if (game.cameras == null)
        {
            return;
        }

        EnsureCapacity(game.cameras.Length);
        for (var i = 0; i < game.cameras.Length; i++)
        {
            var camera = game.cameras[i];
            if (camera?.hud == null)
            {
                continue;
            }

            var player = LocalPlayerBinder.Bind(camera.hud);
            if (player == null)
            {
                continue;
            }

            _cameras[_cameraCount++] = new IdObserver(player, camera);
            if (!ContainsPlayer(player))
            {
                _players[_playerCount++] = player;
            }
        }
    }

    internal static void Clear()
    {
        _cameraCount = 0;
        _playerCount = 0;
        _clock = int.MinValue;
        _game = null;
        for (var i = 0; i < _cameras.Length; i++)
        {
            _cameras[i] = default;
        }

        for (var i = 0; i < _players.Length; i++)
        {
            _players[i] = null!;
        }
    }

    internal static bool IsWatchedBy(Creature creature, Player player)
    {
        var room = creature.room;
        if (room == null)
        {
            return false;
        }

        var chunk = creature.mainBodyChunk ?? creature.firstChunk;
        var visible = false;
        for (var i = 0; i < _cameraCount; i++)
        {
            var observer = _cameras[i];
            if (!ReferenceEquals(observer.Player, player))
            {
                continue;
            }

            var camera = observer.Camera;
            if (camera == null || camera.room != room)
            {
                continue;
            }

            if (camera.PositionCurrentlyVisible(chunk.pos, 0f, widescreen: true))
            {
                visible = true;
                break;
            }
        }

        if (!visible)
        {
            return false;
        }

        if (!MoreSlugHUDConfig.IdStrictWatch)
        {
            return true;
        }

        if (player.room != room)
        {
            return false;
        }

        var from = player.mainBodyChunk ?? player.firstChunk;
        return room.VisualContact(from.pos, chunk.pos);
    }

    private static void EnsureCapacity(int cameras)
    {
        if (_cameras.Length >= cameras)
        {
            return;
        }

        Array.Resize(ref _cameras, cameras);
        Array.Resize(ref _players, cameras);
    }

    private static bool ContainsPlayer(Player player)
    {
        for (var i = 0; i < _playerCount; i++)
        {
            if (ReferenceEquals(_players[i], player))
            {
                return true;
            }
        }

        return false;
    }
}
