using Tsukimichi.Umbra.Core;
using Umbra.Common;
using Umbra.Widgets;
using Una.Drawing;

namespace Tsukimichi.Umbra.Widgets;

/// <summary>
/// What every Tsukimichi widget shares: the link to Tsukimichi, the Tsukimichi popup, the click action (A2) and the
/// rule that a frame only compares a version number. Labels change in <see cref="OnSummary"/>, called when the summary
/// changes; a frame with nothing new allocates nothing.
/// </summary>
public abstract class TsukimichiWidgetBase(
    WidgetInfo info,
    string? guid = null,
    Dictionary<string, object>? configValues = null
) : StandardToolbarWidget(info, guid, configValues)
{
    private const string ClickActionOption = "ClickAction";

    private TsukimichiMenu? menu;
    private int seenVersion = -1;
    private string? tooltip;
    private string? label;
    private string? subLabel;

    /// <summary>The Tsukimichi popup; it opens only when the click action is the popup.</summary>
    public override MenuPopup Popup { get; } = new();

    protected TsukimichiLink Link { get; } = Framework.Service<TsukimichiLink>();

    /// <summary>Whether this widget offers the click-action option (the Tsukimichi widget always opens its popup).</summary>
    protected virtual bool HasClickActionOption => true;

    /// <summary>What a click does until the player chooses.</summary>
    protected virtual ClickAction DefaultClickAction => ClickAction.OpenTsukimichi;

    /// <summary>Tsukimichi's summary changed (or the widget just loaded): update the labels.</summary>
    protected abstract void OnSummary(Summary summary);

    /// <summary>Per-frame work beyond the summary; must not allocate.</summary>
    protected virtual void OnFrame() { }

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        if (!HasClickActionOption)
        {
            return base.GetConfigVariables();
        }

        var choices = new Dictionary<string, string>();
        foreach (var (value, label) in ClickActions.Choices)
        {
            choices[value] = label;
        }

        return
        [
            ..base.GetConfigVariables(),
            new SelectWidgetConfigVariable(
                ClickActionOption,
                "Click action",
                "What a click on this widget does. Nothing here starts travel: Tsukimichi only opens.",
                ClickActions.ValueOf(DefaultClickAction),
                choices
            ) { Category = I18N.Translate("Widgets.Standard.Config.Category.General") },
        ];
    }

    protected override void OnLoad()
    {
        menu = new TsukimichiMenu(Popup, Link);
        Node.OnClick += OnWidgetClicked;
    }

    protected sealed override void OnUnload()
    {
        Node.OnClick -= OnWidgetClicked;
    }

    protected sealed override void OnDraw()
    {
        Popup.IsDisabled = CurrentClickAction() != ClickAction.Popup;

        var version = Link.Version;
        if (version != seenVersion)
        {
            seenVersion = version;
            var summary = Link.Current;
            menu?.Apply(summary);
            OnSummary(summary);
        }

        OnFrame();
    }

    /// <summary>Sets the text only when it changed.</summary>
    protected void SetLabel(string text)
    {
        if (!string.Equals(text, label, StringComparison.Ordinal))
        {
            label = text;
            SetText(text);
        }
    }

    /// <summary>Sets the second line only when it changed.</summary>
    protected void SetSubLabel(string text)
    {
        if (!string.Equals(text, subLabel, StringComparison.Ordinal))
        {
            subLabel = text;
            SetSubText(text);
        }
    }

    /// <summary>Sets the tooltip only when its text changed.</summary>
    protected void SetTooltipText(string? text)
    {
        var value = string.IsNullOrEmpty(text) ? null : text;
        if (!string.Equals(value, tooltip, StringComparison.Ordinal))
        {
            tooltip = value;
            SetTooltip(value);
        }
    }

    private ClickAction CurrentClickAction() =>
        HasClickActionOption
            ? ClickActions.Parse(GetConfigValue<string>(ClickActionOption), DefaultClickAction)
            : DefaultClickAction;

    private void OnWidgetClicked(Node _)
    {
        var place = ClickActions.PlaceOf(CurrentClickAction());
        if (place is not null)
        {
            Link.OpenAt(place);
        }
    }
}
