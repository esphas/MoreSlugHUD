using System.Collections.Generic;
using Menu;
using UnityEngine;

namespace MoreSlugHUD;

internal static class IconLookup
{
    internal const string FallbackSprite = "Futile_White";
    internal const string PlaceholderSprite = "smallEmptyCircle";
    internal const string FoodSprite = "FoodCircleA";
    internal const string FlowerSprite = "FlowerMarker";

    private static readonly HashSet<string> LoggedMissing = new();

    internal static IconDraw? FromObject(AbstractPhysicalObject? held)
    {
        if (held?.type == null)
        {
            return null;
        }

        if (held.type == AbstractPhysicalObject.AbstractObjectType.Creature
            && held is AbstractCreature creature)
        {
            var data = CreatureSymbol.SymbolDataFromCreature(creature);
            return FromNames(
                CreatureSymbol.SpriteNameOfCreature(data),
                CreatureSymbol.ColorOfCreature(data),
                held);
        }

        if (held.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
        {
            return FromNames(FlowerSprite, RainWorld.GoldRGB, held);
        }

        if (held is AbstractSpear spear && spear.stuckInWall)
        {
            return FromItemType(held.type, SpearIntData(spear), held);
        }

        var symbol = ItemSymbol.SymbolDataFromItem(held);
        if (symbol.HasValue)
        {
            return FromItemType(symbol.Value.itemType, symbol.Value.intData, held);
        }

        LogMissing(held.ToString());
        return new IconDraw(FallbackSprite, MenuColorEffect.rgbMediumGrey);
    }

    internal static IconDraw FromItemType(
        AbstractPhysicalObject.AbstractObjectType type,
        int intData,
        AbstractPhysicalObject? held = null) =>
        FromNames(
            ItemSymbol.SpriteNameForItem(type, intData),
            ItemSymbol.ColorForItem(type, intData),
            held,
            type.value);

    internal static IconDraw FromCreatureType(CreatureTemplate.Type type)
    {
        var data = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
        return FromNames(
            CreatureSymbol.SpriteNameOfCreature(data),
            CreatureSymbol.ColorOfCreature(data),
            logKey: type.value);
    }

    internal static IconDraw FoodResult() => FromNames(FoodSprite, Color.white, logKey: FoodSprite);

    internal static string PlaceholderElement() =>
        HasElement(PlaceholderSprite) ? PlaceholderSprite : FallbackSprite;

    internal static Color PlaceholderColor() => MenuColorEffect.rgbMediumGrey;

    internal static bool HasElement(string name)
    {
        try
        {
            return Futile.atlasManager != null && Futile.atlasManager.DoesContainElementWithName(name);
        }
        catch
        {
            return false;
        }
    }

    private static IconDraw FromNames(
        string? spriteName,
        Color color,
        AbstractPhysicalObject? held = null,
        string? logKey = null)
    {
        if (string.IsNullOrEmpty(spriteName) || !HasElement(spriteName!))
        {
            LogMissing(logKey ?? held?.ToString() ?? spriteName ?? FallbackSprite);
            spriteName = FallbackSprite;
        }

        return new IconDraw(spriteName!, color);
    }

    private static int SpearIntData(AbstractSpear spear)
    {
        if (ModManager.MSC && spear.hue != 0f)
        {
            return 3;
        }

        if (ModManager.MSC && spear.electric)
        {
            return 2;
        }

        return spear.explosive ? 1 : 0;
    }

    private static void LogMissing(string key)
    {
        if (LoggedMissing.Add(key))
        {
            MoreSlugHUDLog.Warning($"icon missing for {key}");
        }
    }
}
