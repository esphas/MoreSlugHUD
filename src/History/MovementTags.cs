using System;

namespace MoreSlugHUD;

[Flags]
internal enum MovementTags : ushort
{
    None = 0,
    Incapacitated = 1 << 0,
    Grabbed = 1 << 1,
    Shortcut = 1 << 2,
    Corridor = 1 << 3,
    ZeroG = 1 << 4,
    ZeroGPole = 1 << 5,
    SurfaceSwim = 1 << 6,
    DeepSwim = 1 << 7,
    WallClimb = 1 << 8,
    Pole = 1 << 9,
    Stand = 1 << 10,
    Crawl = 1 << 11,
    Air = 1 << 12,
}
