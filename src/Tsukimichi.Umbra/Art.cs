using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Tsukimichi.Umbra.Core;
using Umbra.Common;

namespace Tsukimichi.Umbra;

/// <summary>The theme moons, read once from the DLL's resources (assets/moons, made by tools/make-moons.mjs).</summary>
internal static class MoonImages
{
    private static readonly Dictionary<string, byte[]> Cache = new(StringComparer.Ordinal);

    /// <summary>The PNG of a theme's moon; Medallion's for an unknown theme.</summary>
    public static byte[]? For(string? theme)
    {
        var key = Moons.KeyFor(theme);
        if (Cache.TryGetValue(key, out var bytes))
        {
            return bytes;
        }

        using var stream = typeof(MoonImages).Assembly.GetManifestResourceStream(Moons.ResourceName(key));
        if (stream is null)
        {
            return null;
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        bytes = memory.ToArray();
        Cache[key] = bytes;
        return bytes;
    }
}

/// <summary>
/// The quest marker a quest shows in the game (<c>EventIconType.NpcIconAvailable</c> + 1: the main scenario, side
/// quest or feature marker), as Tsukimichi draws Up next. It reads only the marker kind, never a name, so the spoiler
/// shield on Tsukimichi's side is untouched.
/// </summary>
internal static class QuestMarkers
{
    /// <summary>The side quest marker, for a quest the sheet doesn't know.</summary>
    public const uint SidequestMarker = 71221;

    // Up next and the Ready rows ask for a handful of quests an evening; a marker never changes, so keep each one.
    private static readonly Dictionary<uint, uint> Cache = [];

    public static uint For(uint questRowId)
    {
        if (questRowId == 0)
        {
            return SidequestMarker;
        }

        if (Cache.TryGetValue(questRowId, out var cached))
        {
            return cached;
        }

        var icon = SidequestMarker;
        try
        {
            var quest = Framework.Service<IDataManager>().GetExcelSheet<Quest>().GetRowOrDefault(questRowId);
            var available = quest?.EventIconType.ValueNullable?.NpcIconAvailable ?? 0;
            if (available != 0)
            {
                icon = available + 1;
            }
        }
        catch (InvalidOperationException)
        {
            // No data manager or no sheet: keep the side quest marker.
        }

        Cache[questRowId] = icon;
        return icon;
    }
}
