using Tsukimichi.Umbra.Core;

namespace Tsukimichi.Umbra.Tests;

public class LinkTests
{
    [Fact]
    public void No_api_answer_means_not_running() =>
        Assert.Equal(LinkState.NotRunning, Link.Classify(null, null, null));

    [Fact]
    public void Api_without_summary_means_too_old() =>
        Assert.Equal(LinkState.TooOld, Link.Classify(1, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Older_summary_version_means_too_old(int summaryVersion) =>
        Assert.Equal(LinkState.TooOld, Link.Classify(1, summaryVersion, "Kiri"));

    [Fact]
    public void Newer_summary_version_means_too_new() =>
        Assert.Equal(LinkState.TooNew, Link.Classify(1, 2, "Kiri"));

    [Fact]
    public void Other_api_versions_are_not_read()
    {
        Assert.Equal(LinkState.TooOld, Link.Classify(0, 1, "Kiri"));
        Assert.Equal(LinkState.TooNew, Link.Classify(2, 1, "Kiri"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Summary_without_a_character_means_not_ready(string? name) =>
        Assert.Equal(LinkState.NotReady, Link.Classify(1, 1, name));

    [Fact]
    public void Summary_with_a_character_means_ready() =>
        Assert.Equal(LinkState.Ready, Link.Classify(1, 1, "Kiri"));

    [Fact]
    public void Version_is_three_parts()
    {
        Assert.Equal("1.0.0", Link.FormatVersion(new Version(1, 0, 0, 0)));
        Assert.Equal("1.2.0", Link.FormatVersion(new Version(1, 2)));
        Assert.Equal("0.0.0", Link.FormatVersion(null));
    }

    [Fact]
    public void Hello_names_the_add_on_as_docs_ipc_md_does() =>
        Assert.Equal("Tsukimichi.Umbra", Link.AddonName);
}
