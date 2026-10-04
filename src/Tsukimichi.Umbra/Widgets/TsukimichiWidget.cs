using Tsukimichi.Umbra.Core;
using Umbra.Widgets;
using Una.Drawing;

namespace Tsukimichi.Umbra.Widgets;

/// <summary>
/// A1, the Tsukimichi widget: the current theme's moon and "12 Ready" on the bar; a click opens the Tsukimichi popup,
/// as Umbra's own popup widgets do. When Tsukimichi isn't running or is too old, the bar reads "Tsukimichi" with no
/// count and the popup gives the reason (A3).
/// </summary>
[ToolbarWidget(
    "Tsukimichi",
    "Tsukimichi",
    "Tonight from Tsukimichi: the Ready count on the bar, and Up next, the journal, events ending soon and shortcuts in its popup. Needs Tsukimichi 1.22 or later.",
    ["tsukimichi", "quest", "quests", "ready", "tonight", "moon", "journal"]
)]
public sealed class TsukimichiWidget(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : TsukimichiWidgetBase(info, guid, configValues)
{
    // Any game icon, so Umbra counts the widget as having one; the moon image drawn over it takes precedence.
    private const uint PlaceholderIcon = QuestMarkers.SidequestMarker;

    private Node? iconNode;
    private string? moonKey;

    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon;

    protected override bool HasClickActionOption => false;

    protected override ClickAction DefaultClickAction => ClickAction.Popup;

    protected override void OnLoad()
    {
        base.OnLoad();
        SetGameIconId(PlaceholderIcon);
        iconNode = IconNode;
    }

    protected override void OnSummary(Summary summary)
    {
        SetLabel(SummaryText.BarText(summary));

        var key = Moons.KeyFor(summary.Theme);
        if (iconNode is not null && !string.Equals(key, moonKey, StringComparison.Ordinal))
        {
            moonKey = key;
            iconNode.Style.ImageBytes = MoonImages.For(key);
        }
    }
}
