namespace Tsukimichi.Umbra.Core;

/// <summary>What the add-on can say about Tsukimichi right now (spec-1.22 A3).</summary>
public enum LinkState
{
    /// <summary>Tsukimichi is not installed, not loaded, or reloading.</summary>
    NotRunning,

    /// <summary>Tsukimichi is loaded but has no summary (older than 1.22.0) or an older summary version.</summary>
    TooOld,

    /// <summary>Tsukimichi speaks a newer summary version than this add-on understands.</summary>
    TooNew,

    /// <summary>The summary is there but empty: nobody is logged in, or the catalog is still being built.</summary>
    NotReady,

    /// <summary>The summary is there and describes the logged-in character.</summary>
    Ready,
}

/// <summary>The version check (docs/ipc.md, "The summary": check the summary version once when Tsukimichi appears).</summary>
public static class Link
{
    /// <summary>The internal name Dalamud knows Tsukimichi by.</summary>
    public const string TsukimichiInternalName = "Tsukimichi";

    /// <summary>The name this add-on gives <c>Tsukimichi.AddonHello</c>.</summary>
    public const string AddonName = "Tsukimichi.Umbra";

    /// <summary>The API version this add-on was written for (<c>Tsukimichi.ApiVersion</c>).</summary>
    public const int SupportedApiVersion = 1;

    /// <summary>The summary version this add-on was written for (<c>Tsukimichi.GetSummaryVersion</c>).</summary>
    public const int SupportedSummaryVersion = 1;

    /// <summary>
    /// Classifies what the gates answered. <paramref name="apiVersion"/> is null when <c>Tsukimichi.ApiVersion</c> did not
    /// answer (Tsukimichi absent); <paramref name="summaryVersion"/> is null when <c>Tsukimichi.GetSummaryVersion</c> did
    /// not answer (a Tsukimichi before 1.22.0); <paramref name="characterName"/> is the summary's character, empty
    /// before one is evaluated.
    /// </summary>
    public static LinkState Classify(int? apiVersion, int? summaryVersion, string? characterName)
    {
        if (apiVersion is null)
        {
            return LinkState.NotRunning;
        }

        if (apiVersion != SupportedApiVersion)
        {
            return apiVersion < SupportedApiVersion ? LinkState.TooOld : LinkState.TooNew;
        }

        if (summaryVersion is null || summaryVersion < SupportedSummaryVersion)
        {
            return LinkState.TooOld;
        }

        if (summaryVersion > SupportedSummaryVersion)
        {
            return LinkState.TooNew;
        }

        return string.IsNullOrEmpty(characterName) ? LinkState.NotReady : LinkState.Ready;
    }

    /// <summary>The version this add-on tells Tsukimichi ("1.0.0"): the assembly version's first three parts.</summary>
    public static string FormatVersion(Version? version) =>
        version is null ? "0.0.0" : $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
}
