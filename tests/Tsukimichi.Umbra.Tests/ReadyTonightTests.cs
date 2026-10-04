using Tsukimichi.Umbra.Core;

namespace Tsukimichi.Umbra.Tests;

public class ReadyTonightTests
{
    private static readonly ReadyQuest[] RenderQuests =
    [
        new(70_101, "Knowing the Pelupelu", "Urqopacha"),
        new(70_102, "A Leaking Workpot", "Kozama'uka"),
        new(70_103, "Caught in the Act", "The Pillars"),
    ];

    private static Summary Evening(int ready, IReadOnlyList<ReadyQuest>? list) => new(
        LinkState.Ready, "Kiri", "WHM", 100, 0, "", "", ready, 3, 27, 30, [], "Dawntrail", 91, false, "medallion", list);

    [Fact]
    public void Names_the_first_three_then_the_rest_as_more()
    {
        // The render: 12 Ready, three named with their place, then "9 more".
        var plan = SummaryText.PlanReady(Evening(12, RenderQuests));

        Assert.Equal(RenderQuests, plan.Quests);
        Assert.Equal("9 more", plan.More);
        Assert.Equal(string.Empty, plan.CountRow);
    }

    [Fact]
    public void Never_names_more_than_three()
    {
        var five = RenderQuests.Concat([new ReadyQuest(70_104, "Four", "Here"), new ReadyQuest(70_105, "Five", "There")]).ToArray();
        var plan = SummaryText.PlanReady(Evening(5, five));

        Assert.Equal(3, plan.Quests.Count);
        Assert.Equal("Knowing the Pelupelu", plan.Quests[0].Name);
        Assert.Equal("2 more", plan.More);
    }

    [Fact]
    public void No_more_row_when_every_ready_quest_is_named()
    {
        var plan = SummaryText.PlanReady(Evening(2, RenderQuests[..2]));

        Assert.Equal(2, plan.Quests.Count);
        Assert.Equal(string.Empty, plan.More);
        Assert.Equal(string.Empty, plan.CountRow);
    }

    [Fact]
    public void Older_tsukimichi_without_the_gate_falls_back_to_the_count_row()
    {
        var plan = SummaryText.PlanReady(Evening(12, null));

        Assert.Empty(plan.Quests);
        Assert.Equal(string.Empty, plan.More);
        Assert.Equal("See them in Tonight", plan.CountRow);
    }

    [Fact]
    public void An_empty_answer_while_some_are_ready_falls_back_to_the_count_row()
    {
        var plan = SummaryText.PlanReady(Evening(12, []));

        Assert.Empty(plan.Quests);
        Assert.Equal("See them in Tonight", plan.CountRow);
    }

    [Fact]
    public void Nothing_ready_says_so_with_or_without_the_gate()
    {
        Assert.Equal("Nothing Ready on WHM tonight", SummaryText.PlanReady(Evening(0, [])).CountRow);
        Assert.Equal("Nothing Ready on WHM tonight", SummaryText.PlanReady(Evening(0, null)).CountRow);
    }

    [Fact]
    public void Without_a_summary_the_group_is_empty()
    {
        var plan = SummaryText.PlanReady(Summary.Empty(LinkState.NotRunning));

        Assert.Empty(plan.Quests);
        Assert.Equal(string.Empty, plan.More);
        Assert.Equal(string.Empty, plan.CountRow);
    }

    [Fact]
    public void A_missing_gate_stays_missing() => Assert.Null(ReadyQuest.From(null));

    [Fact]
    public void Gate_answer_is_kept_in_order_and_cleaned()
    {
        (uint, string, string)[] answer =
        [
            (70_101, "Knowing the Pelupelu", "Urqopacha"),
            (0, "No id", "Nowhere"),
            (70_102, "", "No name"),
            (70_103, "Caught in the Act", null!),
        ];

        var list = ReadyQuest.From(answer);

        Assert.NotNull(list);
        Assert.Equal(
            [new ReadyQuest(70_101, "Knowing the Pelupelu", "Urqopacha"), new ReadyQuest(70_103, "Caught in the Act", "")],
            list);
    }

    [Fact]
    public void Summary_without_the_list_defaults_to_missing() =>
        Assert.Null(Summary.Empty(LinkState.Ready).ReadyTonight);

    [Fact]
    public void Make_room_is_its_own_place() => Assert.Equal("makeroom", Places.MakeRoom);
}
