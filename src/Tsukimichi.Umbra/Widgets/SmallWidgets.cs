using Dalamud.Interface;
using Tsukimichi.Umbra.Core;
using Umbra.Common;
using Umbra.Widgets;

namespace Tsukimichi.Umbra.Widgets;

/// <summary>A2, "Up next": the quest's own marker and its name; its step on hover.</summary>
[ToolbarWidget(
    "TsukimichiUpNext",
    "Tsukimichi: Up next",
    "Tonight's Up next from Tsukimichi: the quest's marker and name, with its step on hover. Needs Tsukimichi 1.22 or later.",
    ["tsukimichi", "quest", "up next", "next", "tonight"]
)]
public sealed class UpNextWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : TsukimichiWidgetBase(info, guid, configValues)
{
    private const string UseQuestMarkerOption = "UseQuestMarker";

    private uint marker;

    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override uint DefaultGameIconId => QuestMarkers.SidequestMarker;

    protected override ClickAction DefaultClickAction => ClickAction.OpenTonight;

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables() =>
    [
        ..base.GetConfigVariables(),
        new BooleanWidgetConfigVariable(
            UseQuestMarkerOption,
            "Use the quest's marker",
            "Show Up next's own marker (main scenario, side quest or feature) in place of the icon chosen here.",
            true
        ) { Category = I18N.Translate("Widgets.Standard.Config.Category.Icon") },
    ];

    protected override void OnSummary(Summary summary)
    {
        SetLabel(SummaryText.UpNextName(summary));
        SetTooltipText(summary.IsReady ? summary.UpNextStep : SummaryText.Missing(summary.State)?.Title);
        marker = SummaryText.HasUpNext(summary) ? QuestMarkers.For(summary.UpNextRowId) : 0;
    }

    protected override void OnFrame()
    {
        // Umbra sets the chosen icon each frame before this runs; the quest's marker replaces it in the same frame.
        if (marker != 0 && GetConfigValue<bool>(UseQuestMarkerOption))
        {
            SetGameIconId(marker);
        }
    }
}

/// <summary>A2, "Journal 27/30".</summary>
[ToolbarWidget(
    "TsukimichiJournal",
    "Tsukimichi: Journal",
    "Your journal's room from Tsukimichi: \"Journal 27/30\", with the slots left on hover. Needs Tsukimichi 1.22 or later.",
    ["tsukimichi", "journal", "quest", "slots"]
)]
public sealed class JournalWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : TsukimichiWidgetBase(info, guid, configValues)
{
    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override string DefaultIconType => IconTypeFontAwesome;

    protected override FontAwesomeIcon DefaultFontAwesomeIcon => FontAwesomeIcon.Book;

    protected override string DefaultDisplayMode => DisplayModeTextOnly;

    protected override void OnSummary(Summary summary)
    {
        SetLabel(SummaryText.JournalBar(summary));
        SetTooltipText(summary.IsReady
            ? SummaryText.JournalLine(summary.JournalUsed, summary.JournalCap)
            : SummaryText.Missing(summary.State)?.Title);
    }
}

/// <summary>A2, "Story meter": the expansion over "91 to the latest story". No bar and no percentage (spec decision 16).</summary>
[ToolbarWidget(
    "TsukimichiStoryMeter",
    "Tsukimichi: Story meter",
    "Where your main scenario stands, from Tsukimichi: the expansion over the quests left to the latest story. Needs Tsukimichi 1.22 or later.",
    ["tsukimichi", "story", "msq", "main scenario", "meter"]
)]
public sealed class StoryMeterWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : TsukimichiWidgetBase(info, guid, configValues)
{
    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.SubText |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override string DefaultIconType => IconTypeFontAwesome;

    protected override FontAwesomeIcon DefaultFontAwesomeIcon => FontAwesomeIcon.Flag;

    protected override string DefaultDisplayMode => DisplayModeTextOnly;

    protected override void OnSummary(Summary summary)
    {
        SetLabel(SummaryText.StoryTop(summary));
        SetSubLabel(SummaryText.StoryBottom(summary));
        SetTooltipText(summary.IsReady ? null : SummaryText.Missing(summary.State)?.Title);
    }
}

/// <summary>A2, "Next reset": "Next reset" over "daily in 3 h 12 m". Needs no Tsukimichi: the reset times are the game's.</summary>
[ToolbarWidget(
    "TsukimichiNextReset",
    "Tsukimichi: Next reset",
    "The next daily or weekly reset and how long until it.",
    ["tsukimichi", "reset", "daily", "weekly", "timer"]
)]
public sealed class NextResetWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : TsukimichiWidgetBase(info, guid, configValues)
{
    private int shownKey = -1;

    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.SubText |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override string DefaultIconType => IconTypeFontAwesome;

    protected override FontAwesomeIcon DefaultFontAwesomeIcon => FontAwesomeIcon.Clock;

    protected override ClickAction DefaultClickAction => ClickAction.OpenTonight;

    protected override void OnLoad()
    {
        base.OnLoad();
        SetLabel("Next reset");
    }

    protected override void OnSummary(Summary summary) { }

    protected override void OnFrame()
    {
        // Integer work each frame; the line is rebuilt once a minute.
        var (kind, left) = ResetClock.Next(DateTime.UtcNow);
        var minutes = ResetClock.MinutesLeft(left);
        var key = ((int)kind * 100_000) + minutes;
        if (key != shownKey)
        {
            shownKey = key;
            SetSubLabel(SummaryText.ResetLine(kind, minutes));
        }
    }
}
