using System.Text;

namespace MoreSlugHUD;

internal static class InputHistoryFormatter
{
    internal static string FormatTags(MovementTags tags)
    {
        if (tags == MovementTags.None)
        {
            return "-";
        }

        var builder = new StringBuilder(32);
        AppendTag(builder, tags, MovementTags.Incapacitated, "incap");
        AppendTag(builder, tags, MovementTags.Grabbed, "grab");
        AppendTag(builder, tags, MovementTags.Shortcut, "pipe");
        AppendTag(builder, tags, MovementTags.Corridor, "corr");
        AppendTag(builder, tags, MovementTags.ZeroG, "zg");
        AppendTag(builder, tags, MovementTags.ZeroGPole, "zgpole");
        AppendTag(builder, tags, MovementTags.SurfaceSwim, "surf");
        AppendTag(builder, tags, MovementTags.DeepSwim, "deep");
        AppendTag(builder, tags, MovementTags.WallClimb, "wall");
        AppendTag(builder, tags, MovementTags.Pole, "pole");
        AppendTag(builder, tags, MovementTags.Stand, "stand");
        AppendTag(builder, tags, MovementTags.Crawl, "crawl");
        AppendTag(builder, tags, MovementTags.Air, "air");
        return builder.ToString();
    }

    private static void AppendTag(StringBuilder builder, MovementTags tags, MovementTags flag, string label)
    {
        if ((tags & flag) == 0)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(label);
    }
}
