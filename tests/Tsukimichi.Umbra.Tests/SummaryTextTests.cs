using Tsukimichi.Umbra.Core;

namespace Tsukimichi.Umbra.Tests;

public class SummaryTextTests
{
    // The render's evening (umbra-widgets-1.22.png): Kiri, WHM 100, 12 Ready, journal 27/30, The Rising in 2 days.
    private static readonly Summary Evening = new(
        LinkState.Ready,
        "Kiri",
        "WHM",
        100,
        70_000,
        "The Long Road to Xak Tural",
        "Step 3: Speak with Erenville. · Shaaloani",
        12,
        3,
        27,
        30,
        [new EndingEvent("The Rising", 2)],
        "Dawntrail",
        91,
        false,
        "medallion");

    [Fact]
    public void Bar_shows_the_ready_count() => Assert.Equal("12 Ready", SummaryText.BarText(Evening));

    [Fact]
    public void Bar_at_zero_says_nothing_ready() =>
        Assert.Equal("Nothing Ready", SummaryText.BarText(Evening with { ReadyCount = 0 }));

    [Theory]
    [InlineData(LinkState.NotRunning)]
    [InlineData(LinkState.TooOld)]
    [InlineData(LinkState.TooNew)]
    [InlineData(LinkState.NotReady)]
    public void Bar_without_a_summary_reads_tsukimichi_with_no_count(LinkState state) =>
        Assert.Equal("Tsukimichi", SummaryText.BarText(Summary.Empty(state)));

    [Fact]
    public void Header_names_character_job_and_level()
    {
        Assert.Equal("Tsukimichi · Kiri · WHM 100", SummaryText.Header(Evening));
        Assert.Equal("Tsukimichi · Kiri", SummaryText.Header(Evening with { Job = "" }));
        Assert.Equal("Tsukimichi", SummaryText.Header(Summary.Empty(LinkState.NotRunning)));
    }

    [Fact]
    public void Up_next_names_the_quest_or_says_there_is_none()
    {
        Assert.True(SummaryText.HasUpNext(Evening));
        Assert.Equal("The Long Road to Xak Tural", SummaryText.UpNextName(Evening));
        Assert.Equal("Nothing up next", SummaryText.UpNextName(Evening with { UpNextRowId = 0, UpNextName = "" }));
        Assert.Equal("Up next", SummaryText.UpNextName(Summary.Empty(LinkState.NotRunning)));
    }

    [Fact]
    public void Ready_group_and_row()
    {
        Assert.Equal("Ready tonight · 12", SummaryText.ReadyGroup(12));
        Assert.Equal("See them in Tonight", SummaryText.ReadyRow(12, "WHM"));
        Assert.Equal("Nothing Ready on WHM tonight", SummaryText.ReadyRow(0, "WHM"));
        Assert.Equal("Nothing Ready tonight", SummaryText.ReadyRow(0, ""));
        Assert.Equal("3 can start here", SummaryText.HereCount(3));
        Assert.Equal(string.Empty, SummaryText.HereCount(0));
    }

    [Theory]
    [InlineData(27, 30, "27/30 · 3 slots left")]
    [InlineData(29, 30, "29/30 · 1 slot left")]
    [InlineData(30, 30, "30/30 · full")]
    [InlineData(-1, 0, "Journal not read yet")]
    public void Journal_line_says_what_is_left(int used, int cap, string expected) =>
        Assert.Equal(expected, SummaryText.JournalLine(used, cap));

    [Fact]
    public void Journal_bar()
    {
        Assert.Equal("Journal 27/30", SummaryText.JournalBar(Evening));
        Assert.Equal("Journal", SummaryText.JournalBar(Evening with { JournalUsed = -1, JournalCap = 0 }));
        Assert.Equal("Journal", SummaryText.JournalBar(Summary.Empty(LinkState.NotRunning)));
    }

    [Theory]
    [InlineData(0, "today")]
    [InlineData(1, "1 day")]
    [InlineData(2, "2 days")]
    public void Days_left(int days, string expected) => Assert.Equal(expected, SummaryText.DaysLeft(days));

    [Fact]
    public void Story_meter_says_what_is_left_never_a_percentage()
    {
        Assert.Equal("Dawntrail", SummaryText.StoryTop(Evening));
        Assert.Equal("91 to the latest story", SummaryText.StoryBottom(Evening));
        Assert.Equal("Caught up", SummaryText.StoryBottom(Evening with { StoryLeft = 0, CaughtUp = true }));
        Assert.Equal("Story", SummaryText.StoryTop(Summary.Empty(LinkState.NotReady)));
        Assert.Equal(string.Empty, SummaryText.StoryBottom(Summary.Empty(LinkState.NotReady)));
        Assert.DoesNotContain("%", SummaryText.StoryBottom(Evening));
    }

    [Theory]
    [InlineData(ResetKind.Daily, 192, "daily in 3 h 12 m")]
    [InlineData(ResetKind.Daily, 180, "daily in 3 h")]
    [InlineData(ResetKind.Weekly, 45, "weekly in 45 m")]
    [InlineData(ResetKind.Daily, 0, "daily in 1 m")]
    public void Reset_line(ResetKind kind, int minutes, string expected) =>
        Assert.Equal(expected, SummaryText.ResetLine(kind, minutes));

    [Fact]
    public void Missing_gives_one_reason_per_case()
    {
        var notRunning = SummaryText.Missing(LinkState.NotRunning);
        Assert.NotNull(notRunning);
        Assert.Equal("Tsukimichi isn't running", notRunning.Value.Title);
        Assert.Equal("Turn it on in Dalamud's plugin installer, or install it there.", notRunning.Value.Line);
        Assert.True(notRunning.Value.OffersInstaller);

        var tooOld = SummaryText.Missing(LinkState.TooOld);
        Assert.NotNull(tooOld);
        Assert.Equal("This widget needs Tsukimichi 1.22 or later", tooOld.Value.Title);
        Assert.True(tooOld.Value.OffersInstaller);

        Assert.False(SummaryText.Missing(LinkState.TooNew)!.Value.OffersInstaller);
        Assert.False(SummaryText.Missing(LinkState.NotReady)!.Value.OffersInstaller);
        Assert.Null(SummaryText.Missing(LinkState.Ready));
    }
}
