namespace Tsukimichi.Umbra.Core;

/// <summary>
/// The words <c>Tsukimichi.OpenAt</c> takes (docs/ipc.md). Opening never starts travel, a route or a run: anything that
/// does needs the player's click inside Tsukimichi.
/// </summary>
public static class Places
{
    public const string Main = "main";
    public const string Tonight = "tonight";
    public const string UpNext = "upnext";
    public const string Route = "route";
    public const string Settings = "settings";

    /// <summary>The main window with Make room open (since Tsukimichi 1.22.0; an older build answers false).</summary>
    public const string MakeRoom = "makeroom";
}

/// <summary>What clicking a small widget does (spec-1.22 A2: open Tsukimichi, Tonight, the popup, or nothing).</summary>
public enum ClickAction
{
    None,
    OpenTsukimichi,
    OpenTonight,
    Popup,
}

/// <summary>The click-action option as Umbra saves it (a string per widget instance).</summary>
public static class ClickActions
{
    public const string NoneValue = "None";
    public const string OpenTsukimichiValue = "OpenTsukimichi";
    public const string OpenTonightValue = "OpenTonight";
    public const string PopupValue = "Popup";

    /// <summary>The option's choices in the order the widget settings list them, with their labels.</summary>
    public static IReadOnlyList<(string Value, string Label)> Choices { get; } =
    [
        (OpenTsukimichiValue, "Open Tsukimichi"),
        (OpenTonightValue, "Open Tonight"),
        (PopupValue, "Show the Tsukimichi popup"),
        (NoneValue, "Nothing"),
    ];

    /// <summary>The saved value's action; anything unknown reads as <paramref name="fallback"/>.</summary>
    public static ClickAction Parse(string? value, ClickAction fallback) => value switch
    {
        NoneValue => ClickAction.None,
        OpenTsukimichiValue => ClickAction.OpenTsukimichi,
        OpenTonightValue => ClickAction.OpenTonight,
        PopupValue => ClickAction.Popup,
        _ => fallback,
    };

    /// <summary>The value an action is saved as.</summary>
    public static string ValueOf(ClickAction action) => action switch
    {
        ClickAction.None => NoneValue,
        ClickAction.OpenTonight => OpenTonightValue,
        ClickAction.Popup => PopupValue,
        _ => OpenTsukimichiValue,
    };

    /// <summary>The <c>OpenAt</c> place an action opens; null for the popup and for nothing.</summary>
    public static string? PlaceOf(ClickAction action) => action switch
    {
        ClickAction.OpenTsukimichi => Places.Main,
        ClickAction.OpenTonight => Places.Tonight,
        _ => null,
    };
}
