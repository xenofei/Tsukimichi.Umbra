using Dalamud.Interface;
using Tsukimichi.Umbra.Core;
using Umbra.Widgets;
using Una.Drawing;

namespace Tsukimichi.Umbra;

/// <summary>
/// The Tsukimichi popup (spec-1.22 A1 and A3, umbra-widgets-1.22.png), made only of Umbra's own MenuPopup header,
/// groups, buttons and separators, so Umbra's stylesheet and the player's colour profile draw it. Every item is made
/// once; <see cref="Apply"/> changes labels and visibility when the summary changes, so the layout never rebuilds.
/// </summary>
internal sealed class TsukimichiMenu
{
    private const int EndingRows = 3;

    private readonly TsukimichiLink link;

    private readonly MenuPopup.Header header = new(SummaryText.Name);

    // A3: one reason per case, in Umbra's text colour (a statement, not a disabled button).
    private readonly MenuPopup.Group missingGroup = new(string.Empty);
    private readonly MenuPopup.Button missingTitle = Statement(FontAwesomeIcon.Moon);
    private readonly MenuPopup.Button missingLine = Statement(null);

    // A1: tonight.
    private readonly MenuPopup.Group upNextGroup = new("Up next");
    private readonly MenuPopup.Button upNext = new(string.Empty);
    private readonly MenuPopup.Button upNextStep = Statement(null);
    private readonly MenuPopup.Group readyGroup = new(SummaryText.ReadyGroup(0));
    private readonly MenuPopup.Button readyRow = new(string.Empty) { Icon = QuestMarkers.SidequestMarker };
    private readonly MenuPopup.Group journalGroup = new("Journal");
    private readonly MenuPopup.Button journalRow = new(string.Empty) { Icon = FontAwesomeIcon.Book };
    private readonly MenuPopup.Group endingGroup = new("Ending soon");
    private readonly MenuPopup.Button[] endingRows = new MenuPopup.Button[EndingRows];

    private readonly MenuPopup.Separator footerSeparator = new();
    private readonly MenuPopup.Button openMain = new("Open Tsukimichi") { Icon = FontAwesomeIcon.Moon };
    private readonly MenuPopup.Button openRoute = new("Route") { Icon = FontAwesomeIcon.Route };
    private readonly MenuPopup.Button openSettings = new("Settings") { Icon = FontAwesomeIcon.Cog };

    private readonly MenuPopup.Separator installerSeparator = new();
    private readonly MenuPopup.Button openInstaller = new("Open the plugin installer") { Icon = FontAwesomeIcon.ExternalLinkAlt };

    private bool hasUpNext;

    public TsukimichiMenu(MenuPopup popup, TsukimichiLink link)
    {
        this.link = link;

        missingLine.TextColor = new Color("Widget.PopupMenuTextMuted");
        upNextStep.TextColor = new Color("Widget.PopupMenuTextMuted");

        // Go opens Tsukimichi on Up next with its travel pill focused; it never starts travel or a run by itself.
        upNext.OnClick = () => link.OpenAt(hasUpNext ? Places.UpNext : Places.Tonight);
        readyRow.OnClick = () => link.OpenAt(Places.Tonight);
        journalRow.OnClick = () => link.OpenAt(Places.Main);
        openMain.OnClick = () => link.OpenAt(Places.Main);
        openRoute.OnClick = () => link.OpenAt(Places.Route);
        openSettings.OnClick = () => link.OpenAt(Places.Settings);
        openInstaller.OnClick = link.OpenPluginInstaller;

        missingGroup.Add(missingTitle);
        missingGroup.Add(missingLine);
        upNextGroup.Add(upNext);
        upNextGroup.Add(upNextStep);
        readyGroup.Add(readyRow);
        journalGroup.Add(journalRow);

        for (var i = 0; i < EndingRows; i++)
        {
            var row = new MenuPopup.Button(string.Empty) { Icon = FontAwesomeIcon.Clock };
            row.OnClick = () => link.OpenAt(Places.Tonight);
            endingRows[i] = row;
            endingGroup.Add(row);
        }

        popup.Add(header);
        popup.Add(missingGroup);
        popup.Add(upNextGroup);
        popup.Add(readyGroup);
        popup.Add(journalGroup);
        popup.Add(endingGroup);
        popup.Add(footerSeparator);
        popup.Add(openMain);
        popup.Add(openRoute);
        popup.Add(openSettings);
        popup.Add(installerSeparator);
        popup.Add(openInstaller);

        Apply(link.Current);
    }

    /// <summary>Shows a summary. Called when the summary changes, never per frame.</summary>
    public void Apply(Summary summary)
    {
        var ready = summary.IsReady;
        var running = summary.State is LinkState.Ready or LinkState.NotReady;
        var missing = SummaryText.Missing(summary.State);

        header.Label = SummaryText.Header(summary);

        missingTitle.IsVisible = missing is not null;
        missingTitle.Label = missing?.Title ?? string.Empty;
        missingLine.IsVisible = missing is { Line.Length: > 0 };
        missingLine.Label = missing?.Line ?? string.Empty;

        hasUpNext = SummaryText.HasUpNext(summary);
        upNext.IsVisible = ready;
        upNext.Label = SummaryText.UpNextName(summary);
        upNext.AltText = hasUpNext ? "Go" : string.Empty;
        upNext.Icon = hasUpNext ? QuestMarkers.For(summary.UpNextRowId) : null;
        upNextStep.IsVisible = hasUpNext && summary.UpNextStep.Length > 0;
        upNextStep.Label = summary.UpNextStep;

        readyGroup.Label = SummaryText.ReadyGroup(summary.ReadyCount);
        readyRow.IsVisible = ready;
        readyRow.Label = SummaryText.ReadyRow(summary.ReadyCount, summary.Job);
        readyRow.AltText = SummaryText.HereCount(summary.HereCount);

        journalRow.IsVisible = ready && summary.HasJournal;
        journalRow.Label = SummaryText.JournalLine(summary.JournalUsed, summary.JournalCap);

        for (var i = 0; i < EndingRows; i++)
        {
            var row = endingRows[i];
            var shown = ready && i < summary.EndingSoon.Count;
            row.IsVisible = shown;
            row.Label = shown ? summary.EndingSoon[i].Name : string.Empty;
            row.AltText = shown ? SummaryText.DaysLeft(summary.EndingSoon[i].DaysLeft) : string.Empty;
        }

        footerSeparator.IsVisible = running;
        openMain.IsVisible = running;
        openRoute.IsVisible = ready;
        openSettings.IsVisible = running;

        installerSeparator.IsVisible = missing is { OffersInstaller: true };
        openInstaller.IsVisible = missing is { OffersInstaller: true };
    }

    /// <summary>A line that says something and does nothing when clicked: no hover, and the popup stays open.</summary>
    private static MenuPopup.Button Statement(FontAwesomeIcon? icon)
    {
        var button = new MenuPopup.Button(string.Empty) { ClosePopupOnClick = false };
        if (icon is { } fa)
        {
            button.Icon = fa;
        }

        button.Node.EnableHoverTag = false;
        return button;
    }
}
