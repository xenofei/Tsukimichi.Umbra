using System.Globalization;

namespace Tsukimichi.Umbra.Core;

/// <summary>Why the popup can't show tonight, in one reason per case (spec-1.22 A3).</summary>
/// <param name="Title">The reason, in Umbra's own text colour.</param>
/// <param name="Line">What to do about it; empty when there is nothing to do.</param>
/// <param name="OffersInstaller">Whether "Open the plugin installer" helps.</param>
public readonly record struct MissingReason(string Title, string Line, bool OffersInstaller);

/// <summary>The Ready group's rows.</summary>
/// <param name="Quests">The named quests, at most <see cref="SummaryText.ReadyQuestsShown"/>; each opens that quest.</param>
/// <param name="More">"9 more", which opens Tonight; empty when every Ready quest is named.</param>
/// <param name="CountRow">The fallback row when no quest is named; empty when quests are named.</param>
public readonly record struct ReadyPlan(IReadOnlyList<ReadyQuest> Quests, string More, string CountRow);

/// <summary>
/// Every word the widgets and the popup show (spec-1.22 A1–A3, umbra-widgets-1.22.png). English only, as Tsukimichi.
/// Pure: the widgets call these only when the summary or the minute changes, never per frame.
/// </summary>
public static class SummaryText
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>The bar's name when there is no count to show.</summary>
    public const string Name = "Tsukimichi";

    /// <summary>The Tsukimichi widget's text: "12 Ready", "Nothing Ready", or "Tsukimichi" with no count.</summary>
    public static string BarText(Summary summary)
    {
        if (!summary.IsReady)
        {
            return Name;
        }

        return summary.ReadyCount > 0
            ? string.Create(Invariant, $"{summary.ReadyCount} Ready")
            : "Nothing Ready";
    }

    /// <summary>The popup's header: "Tsukimichi · Kiri · WHM 100".</summary>
    public static string Header(Summary summary)
    {
        if (!summary.IsReady || summary.Character.Length == 0)
        {
            return Name;
        }

        if (summary.Job.Length == 0)
        {
            return $"{Name} · {summary.Character}";
        }

        return summary.Level > 0
            ? string.Create(Invariant, $"{Name} · {summary.Character} · {summary.Job} {summary.Level}")
            : $"{Name} · {summary.Character} · {summary.Job}";
    }

    /// <summary>Whether the summary names an Up next.</summary>
    public static bool HasUpNext(Summary summary) =>
        summary.IsReady && summary.UpNextRowId != 0 && summary.UpNextName.Length > 0;

    /// <summary>
    /// The Up next widget's text: the quest's name; "Nothing up next" when there is none; "Up next" when Tsukimichi
    /// can't say (the tooltip gives the reason).
    /// </summary>
    public static string UpNextName(Summary summary)
    {
        if (!summary.IsReady)
        {
            return "Up next";
        }

        return HasUpNext(summary) ? summary.UpNextName : "Nothing up next";
    }

    /// <summary>The Ready group's label: "Ready tonight · 12".</summary>
    public static string ReadyGroup(int ready) => string.Create(Invariant, $"Ready tonight · {Math.Max(0, ready)}");

    /// <summary>How many Ready quests the popup names before "N more" (spec-1.22 A1).</summary>
    public const int ReadyQuestsShown = 3;

    /// <summary>
    /// What the Ready group shows (spec-1.22 A1): the first three Ready quests by name with their place, then "9 more".
    /// When the installed Tsukimichi lacks <c>GetReadyTonight</c> (or it answered nothing while the count says some are
    /// Ready), the group falls back to one count row, <see cref="ReadyRow"/>.
    /// </summary>
    public static ReadyPlan PlanReady(Summary summary)
    {
        if (!summary.IsReady)
        {
            return new ReadyPlan([], string.Empty, string.Empty);
        }

        var list = summary.ReadyTonight;
        if (list is null || (list.Count == 0 && summary.ReadyCount > 0))
        {
            return new ReadyPlan([], string.Empty, ReadyRow(summary.ReadyCount, summary.Job));
        }

        if (list.Count == 0)
        {
            return new ReadyPlan([], string.Empty, ReadyRow(0, summary.Job));
        }

        IReadOnlyList<ReadyQuest> shown = list.Count <= ReadyQuestsShown ? list : list.Take(ReadyQuestsShown).ToArray();
        var more = summary.ReadyCount - shown.Count;
        return new ReadyPlan(shown, more > 0 ? MoreReady(more) : string.Empty, string.Empty);
    }

    /// <summary>The row after the named Ready quests: "9 more".</summary>
    public static string MoreReady(int more) => string.Create(Invariant, $"{more} more");

    /// <summary>
    /// The Ready group's count row, when the quests can't be named: "See them in Tonight", or "Nothing Ready on WHM
    /// tonight". It opens Tonight, where they are listed.
    /// </summary>
    public static string ReadyRow(int ready, string job)
    {
        if (ready > 0)
        {
            return "See them in Tonight";
        }

        return job.Length == 0 ? "Nothing Ready tonight" : $"Nothing Ready on {job} tonight";
    }

    /// <summary>The Ready row's alt text: "3 can start here", or empty.</summary>
    public static string HereCount(int here) => here > 0 ? string.Create(Invariant, $"{here} can start here") : string.Empty;

    /// <summary>The popup's journal line: "27/30 · 3 slots left", "29/30 · 1 slot left", "30/30 · full".</summary>
    public static string JournalLine(int used, int cap)
    {
        if (used < 0 || cap <= 0)
        {
            return "Journal not read yet";
        }

        var left = cap - used;
        return left switch
        {
            <= 0 => string.Create(Invariant, $"{used}/{cap} · full"),
            1 => string.Create(Invariant, $"{used}/{cap} · 1 slot left"),
            _ => string.Create(Invariant, $"{used}/{cap} · {left} slots left"),
        };
    }

    /// <summary>The Journal widget's text: "Journal 27/30", or "Journal" before the journal is read.</summary>
    public static string JournalBar(Summary summary) =>
        summary.IsReady && summary.HasJournal
            ? string.Create(Invariant, $"Journal {summary.JournalUsed}/{summary.JournalCap}")
            : "Journal";

    /// <summary>An event's days left: "today", "1 day", "2 days".</summary>
    public static string DaysLeft(int days) => days switch
    {
        <= 0 => "today",
        1 => "1 day",
        _ => string.Create(Invariant, $"{days} days"),
    };

    /// <summary>The Story meter widget's first line: the expansion ("Dawntrail"), or "Story".</summary>
    public static string StoryTop(Summary summary) =>
        summary.IsReady && summary.StoryPart.Length > 0 ? summary.StoryPart : "Story";

    /// <summary>
    /// The Story meter widget's second line: "91 to the latest story", "Caught up". It says what is left, never a
    /// percentage (spec-1.22 decision 16).
    /// </summary>
    public static string StoryBottom(Summary summary)
    {
        if (!summary.IsReady || summary.StoryPart.Length == 0)
        {
            return string.Empty;
        }

        if (summary.CaughtUp || summary.StoryLeft <= 0)
        {
            return "Caught up";
        }

        return string.Create(Invariant, $"{summary.StoryLeft} to the latest story");
    }

    /// <summary>The Next reset widget's second line: "daily in 3 h 12 m", "weekly in 45 m".</summary>
    public static string ResetLine(ResetKind kind, int minutesLeft)
    {
        var name = kind == ResetKind.Weekly ? "weekly" : "daily";
        var minutes = Math.Max(1, minutesLeft);
        var h = minutes / 60;
        var m = minutes % 60;

        if (h == 0)
        {
            return string.Create(Invariant, $"{name} in {m} m");
        }

        return m == 0
            ? string.Create(Invariant, $"{name} in {h} h")
            : string.Create(Invariant, $"{name} in {h} h {m} m");
    }

    /// <summary>The one reason the popup gives when it can't show tonight; null when the summary is ready.</summary>
    public static MissingReason? Missing(LinkState state) => state switch
    {
        LinkState.NotRunning => new("Tsukimichi isn't running", "Turn it on in Dalamud's plugin installer, or install it there.", true),
        LinkState.TooOld => new("This widget needs Tsukimichi 1.22 or later", "Update Tsukimichi in Dalamud's plugin installer.", true),
        LinkState.TooNew => new("This widget is older than your Tsukimichi", "Update Tsukimichi for Umbra in Umbra's Plugins settings.", false),
        LinkState.NotReady => new("Tonight shows here once you're logged in", string.Empty, false),
        _ => null,
    };
}
