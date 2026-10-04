using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Services;
using Tsukimichi.Umbra.Core;
using Umbra.Common;

namespace Tsukimichi.Umbra;

/// <summary>
/// The add-on's one connection to Tsukimichi (docs/ipc.md, "The summary"), shared by every widget. It reads the summary
/// gates only when something changed: when <c>Tsukimichi.SummaryChanged</c> arrives, when Tsukimichi is loaded or
/// unloaded (Dalamud's ActivePluginsChanged, Tsukimichi's own <c>Disposing</c>), and once when Umbra starts. Widgets
/// compare <see cref="Version"/> each frame, which costs an integer read and allocates nothing.
/// </summary>
[Service]
public sealed class TsukimichiLink : IDisposable
{
    private readonly IDalamudPluginInterface plugin;
    private readonly IFramework framework;
    private readonly string addonVersion;

    private readonly ICallGateSubscriber<int> apiVersion;
    private readonly ICallGateSubscriber<int> getSummaryVersion;
    private readonly ICallGateSubscriber<(string, string, int)> getCharacter;
    private readonly ICallGateSubscriber<(uint, string, string)> getUpNext;
    private readonly ICallGateSubscriber<(int, int, string)> getReadyCount;
    private readonly ICallGateSubscriber<(int, int)> getJournalRoom;
    private readonly ICallGateSubscriber<(string, int)[]> getEndingSoon;
    private readonly ICallGateSubscriber<(string, int, bool)> getStoryMeter;
    private readonly ICallGateSubscriber<string> getTheme;
    private readonly ICallGateSubscriber<string, bool> openAt;
    private readonly ICallGateSubscriber<string, string, int> addonHello;
    private readonly ICallGateSubscriber<object> summaryChanged;
    private readonly ICallGateSubscriber<object> disposing;

    private bool helloSent;
    private bool disposed;

    public TsukimichiLink()
    {
        plugin = Framework.DalamudPlugin;
        framework = Framework.DalamudFramework;
        addonVersion = Link.FormatVersion(typeof(TsukimichiLink).Assembly.GetName().Version);

        apiVersion = plugin.GetIpcSubscriber<int>("Tsukimichi.ApiVersion");
        getSummaryVersion = plugin.GetIpcSubscriber<int>("Tsukimichi.GetSummaryVersion");
        getCharacter = plugin.GetIpcSubscriber<(string, string, int)>("Tsukimichi.GetCharacter");
        getUpNext = plugin.GetIpcSubscriber<(uint, string, string)>("Tsukimichi.GetUpNext");
        getReadyCount = plugin.GetIpcSubscriber<(int, int, string)>("Tsukimichi.GetReadyCount");
        getJournalRoom = plugin.GetIpcSubscriber<(int, int)>("Tsukimichi.GetJournalRoom");
        getEndingSoon = plugin.GetIpcSubscriber<(string, int)[]>("Tsukimichi.GetEndingSoon");
        getStoryMeter = plugin.GetIpcSubscriber<(string, int, bool)>("Tsukimichi.GetStoryMeter");
        getTheme = plugin.GetIpcSubscriber<string>("Tsukimichi.GetTheme");
        openAt = plugin.GetIpcSubscriber<string, bool>("Tsukimichi.OpenAt");
        addonHello = plugin.GetIpcSubscriber<string, string, int>("Tsukimichi.AddonHello");
        summaryChanged = plugin.GetIpcSubscriber<object>("Tsukimichi.SummaryChanged");
        disposing = plugin.GetIpcSubscriber<object>("Tsukimichi.Disposing");

        // Subscribing works whether or not Tsukimichi is loaded; its messages arrive on the framework thread once it is.
        summaryChanged.Subscribe(OnSummaryChanged);
        disposing.Subscribe(OnTsukimichiDisposing);
        plugin.ActivePluginsChanged += OnActivePluginsChanged;

        Refresh();
    }

    /// <summary>The latest read of the summary. Replaced whole, never changed in place.</summary>
    public Summary Current { get; private set; } = Summary.Empty(LinkState.NotRunning);

    /// <summary>Goes up by one each time <see cref="Current"/> is replaced.</summary>
    public int Version { get; private set; }

    /// <summary>Opens Tsukimichi at a place (<see cref="Places"/>); never travels. Call it from a click.</summary>
    public bool OpenAt(string place) => Try(() => openAt.InvokeFunc(place), false);

    /// <summary>
    /// Opens Dalamud's plugin installer where Tsukimichi is: its installed list when Tsukimichi is installed (to turn it
    /// on or update it), else the full list, searched for Tsukimichi. Call it from a click.
    /// </summary>
    public void OpenPluginInstaller()
    {
        var installed = plugin.InstalledPlugins.Any(p => p.InternalName == Link.TsukimichiInternalName);
        plugin.OpenPluginInstallerTo(
            installed ? PluginInstallerOpenKind.InstalledPlugins : PluginInstallerOpenKind.AllPlugins,
            Link.TsukimichiInternalName);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        plugin.ActivePluginsChanged -= OnActivePluginsChanged;
        summaryChanged.Unsubscribe(OnSummaryChanged);
        disposing.Unsubscribe(OnTsukimichiDisposing);
    }

    /// <summary>Reads every summary gate once. Runs on the framework thread.</summary>
    private void Refresh()
    {
        if (disposed)
        {
            return;
        }

        int? api = TryNullable(() => apiVersion.InvokeFunc());
        int? summaryVersion = api is null ? null : TryNullable(() => getSummaryVersion.InvokeFunc());
        var character = summaryVersion == Link.SupportedSummaryVersion
            ? Try(() => getCharacter.InvokeFunc(), (string.Empty, string.Empty, 0))
            : (string.Empty, string.Empty, 0);

        var state = Link.Classify(api, summaryVersion, character.Item1);

        if (state is LinkState.NotRunning)
        {
            helloSent = false;
        }
        else if (!helloSent && state is LinkState.NotReady or LinkState.Ready)
        {
            // Once per Tsukimichi session: Settings › About then shows "Tsukimichi for Umbra · Added · 1.0.0".
            helloSent = Try(() => addonHello.InvokeFunc(Link.AddonName, addonVersion), 0) != 0;
        }

        var theme = state is LinkState.NotReady or LinkState.Ready ? Try(() => getTheme.InvokeFunc(), string.Empty) : string.Empty;

        Publish(state == LinkState.Ready ? Read(state, character, theme) : Summary.Empty(state) with { Theme = theme });
    }

    private Summary Read(LinkState state, (string Name, string Job, int Level) character, string theme)
    {
        var upNext = Try(() => getUpNext.InvokeFunc(), (0u, string.Empty, string.Empty));
        var ready = Try(() => getReadyCount.InvokeFunc(), (0, 0, string.Empty));
        var journal = Try(() => getJournalRoom.InvokeFunc(), (-1, 0));
        var ending = Try(() => getEndingSoon.InvokeFunc(), []);
        var story = Try(() => getStoryMeter.InvokeFunc(), (string.Empty, 0, false));

        var events = new EndingEvent[ending.Length];
        for (var i = 0; i < ending.Length; i++)
        {
            events[i] = new EndingEvent(ending[i].Item1 ?? string.Empty, ending[i].Item2);
        }

        return new Summary(
            state,
            character.Name ?? string.Empty,
            character.Job ?? string.Empty,
            character.Level,
            upNext.Item1,
            upNext.Item2 ?? string.Empty,
            upNext.Item3 ?? string.Empty,
            ready.Item1,
            ready.Item2,
            journal.Item1,
            journal.Item2,
            events,
            story.Item1 ?? string.Empty,
            story.Item2,
            story.Item3,
            theme ?? string.Empty);
    }

    private void Publish(Summary summary)
    {
        Current = summary;
        Version++;
    }

    private void OnSummaryChanged() => Refresh();

    private void OnTsukimichiDisposing()
    {
        // Tsukimichi is unloading: its gates go away right after this message.
        helloSent = false;
        Publish(Summary.Empty(LinkState.NotRunning));
    }

    private void OnActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        if (!args.AffectedInternalNames.Contains(Link.TsukimichiInternalName))
        {
            return;
        }

        // The event may come from any thread; the gates are read on the framework thread, like the messages.
        _ = framework.RunOnFrameworkThread(Refresh);
    }

    private static T Try<T>(Func<T> call, T absent)
    {
        try
        {
            return call();
        }
        catch (IpcError)
        {
            // Tsukimichi is not installed, not loaded, reloading, or older than the gate.
            return absent;
        }
        catch (Exception e)
        {
            // A gate threw inside Tsukimichi. The call runs in Umbra's startup and Tsukimichi's message, so it must not
            // escape: log it and show the gate's absent answer.
            Logger.Warning($"Tsukimichi for Umbra: a Tsukimichi gate failed: {e.GetType().Name}: {e.Message}");
            return absent;
        }
    }

    private static int? TryNullable(Func<int> call)
    {
        try
        {
            return call();
        }
        catch (IpcError)
        {
            return null;
        }
        catch (Exception e)
        {
            Logger.Warning($"Tsukimichi for Umbra: a Tsukimichi gate failed: {e.GetType().Name}: {e.Message}");
            return null;
        }
    }
}
