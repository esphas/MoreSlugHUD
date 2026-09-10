namespace MoreSlugHUD;

internal static class IdEligibility
{
    internal static bool Matches(Creature creature)
    {
        if (creature.Template is { IsLizard: true })
        {
            return true;
        }

        if (creature is Scavenger)
        {
            return true;
        }

        return creature is Player player && (player.isNPC || player.isSlugpup);
    }
}
