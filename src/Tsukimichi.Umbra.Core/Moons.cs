namespace Tsukimichi.Umbra.Core;

/// <summary>
/// The theme moons embedded in the add-on (assets/moons, one per <c>Tsukimichi.GetTheme</c> key). An unknown or empty
/// key, or a Tsukimichi too old to say, gets Medallion's, as docs/ipc.md asks.
/// </summary>
public static class Moons
{
    public const string Default = "medallion";

    /// <summary>Every theme key the add-on has a moon for.</summary>
    public static IReadOnlyList<string> Keys { get; } =
        ["medallion", "classic", "ishgard-glass", "aether-crystal", "astrologian-orrery", "sumi-to-kinpaku"];

    /// <summary>The key whose moon to draw for a theme key from Tsukimichi.</summary>
    public static string KeyFor(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return Default;
        }

        var trimmed = theme.Trim();
        foreach (var key in Keys)
        {
            if (string.Equals(key, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return key;
            }
        }

        return Default;
    }

    /// <summary>The manifest resource name of a moon's PNG (the project embeds assets/moons/*.png under this name).</summary>
    public static string ResourceName(string key) => $"Tsukimichi.Umbra.Moons.{key}.png";
}
