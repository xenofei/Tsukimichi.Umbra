namespace Tsukimichi.Umbra.Core;

/// <summary>
/// One read of Tsukimichi's summary gates (docs/ipc.md, "The summary"), taken when <c>Tsukimichi.SummaryChanged</c>
/// arrives or Tsukimichi appears or leaves. Immutable, so a widget can hold it across frames. Every quest name in it
/// already went through the character's spoiler shield on Tsukimichi's side.
/// </summary>
public sealed record Summary(
    LinkState State,
    string Character,
    string Job,
    int Level,
    uint UpNextRowId,
    string UpNextName,
    string UpNextStep,
    int ReadyCount,
    int HereCount,
    int JournalUsed,
    int JournalCap,
    IReadOnlyList<EndingEvent> EndingSoon,
    string StoryPart,
    int StoryLeft,
    bool CaughtUp,
    string Theme,
    IReadOnlyList<ReadyQuest>? ReadyTonight = null)
{
    /// <summary>The summary of a state with no data (Tsukimichi absent, too old, too new or not ready).</summary>
    public static Summary Empty(LinkState state) =>
        new(state, string.Empty, string.Empty, 0, 0, string.Empty, string.Empty, 0, 0, -1, 0, [], string.Empty, 0, false, string.Empty);

    /// <summary>Whether Tsukimichi answers and the summary describes a character.</summary>
    public bool IsReady => State == LinkState.Ready;

    /// <summary>Whether the journal was read (<c>GetJournalRoom</c> answers (-1, 0) when not).</summary>
    public bool HasJournal => JournalUsed >= 0 && JournalCap > 0;
}

/// <summary>
/// A Ready quest from <c>Tsukimichi.GetReadyTonight</c>: its Quest row id, its name and its giver's zone, both through
/// the character's spoiler shield on Tsukimichi's side. <see cref="Summary.ReadyTonight"/> is null when the installed
/// Tsukimichi lacks that gate.
/// </summary>
public readonly record struct ReadyQuest(uint RowId, string Name, string Place)
{
    /// <summary>
    /// The gate's answer as the popup keeps it: null stays null (the gate is missing, so the popup shows the count row),
    /// null strings become empty, and rows with no id or no name are dropped.
    /// </summary>
    public static IReadOnlyList<ReadyQuest>? From((uint RowId, string Name, string Place)[]? answer)
    {
        if (answer is null)
        {
            return null;
        }

        var list = new List<ReadyQuest>(answer.Length);
        foreach (var (rowId, name, place) in answer)
        {
            if (rowId != 0 && !string.IsNullOrEmpty(name))
            {
                list.Add(new ReadyQuest(rowId, name, place ?? string.Empty));
            }
        }

        return list;
    }
}

/// <summary>A seasonal event ending soon: its name and whole days left (0 ends today).</summary>
public readonly record struct EndingEvent(string Name, int DaysLeft);
