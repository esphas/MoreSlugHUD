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
        var order = MovementTagCatalog.DisplayOrder;
        for (var i = 0; i < order.Length; i++)
        {
            AppendTag(builder, tags, order[i].Tag, order[i].DebugName);
        }

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
