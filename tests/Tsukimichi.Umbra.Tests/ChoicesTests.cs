using Tsukimichi.Umbra.Core;

namespace Tsukimichi.Umbra.Tests;

public class ChoicesTests
{
    [Theory]
    [InlineData(ClickAction.None)]
    [InlineData(ClickAction.OpenTsukimichi)]
    [InlineData(ClickAction.OpenTonight)]
    [InlineData(ClickAction.Popup)]
    public void Click_actions_round_trip(ClickAction action) =>
        Assert.Equal(action, ClickActions.Parse(ClickActions.ValueOf(action), ClickAction.None));

    [Fact]
    public void Unknown_click_action_falls_back() =>
        Assert.Equal(ClickAction.OpenTonight, ClickActions.Parse("Teleport", ClickAction.OpenTonight));

    [Fact]
    public void Every_choice_is_offered_once() =>
        Assert.Equal(4, ClickActions.Choices.Select(c => ClickActions.Parse(c.Value, ClickAction.None)).Distinct().Count());

    [Fact]
    public void Click_actions_only_open_places_never_travel()
    {
        Assert.Equal(Places.Main, ClickActions.PlaceOf(ClickAction.OpenTsukimichi));
        Assert.Equal(Places.Tonight, ClickActions.PlaceOf(ClickAction.OpenTonight));
        Assert.Null(ClickActions.PlaceOf(ClickAction.Popup));
        Assert.Null(ClickActions.PlaceOf(ClickAction.None));
    }

    [Theory]
    [InlineData("medallion", "medallion")]
    [InlineData("Sumi-To-Kinpaku", "sumi-to-kinpaku")]
    [InlineData("a-future-theme", "medallion")]
    [InlineData("", "medallion")]
    [InlineData(null, "medallion")]
    public void Unknown_themes_get_medallions_moon(string? theme, string expected) =>
        Assert.Equal(expected, Moons.KeyFor(theme));

    [Fact]
    public void Every_theme_key_has_a_moon_image()
    {
        foreach (var key in Moons.Keys)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "moons", key + ".png");
            Assert.True(File.Exists(path), $"assets/moons/{key}.png is missing");
            Assert.True(new FileInfo(path).Length > 0);
        }
    }
}
