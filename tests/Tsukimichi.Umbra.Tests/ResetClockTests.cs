using Tsukimichi.Umbra.Core;

namespace Tsukimichi.Umbra.Tests;

public class ResetClockTests
{
    private static DateTime Utc(int year, int month, int day, int hour, int minute, int second = 0) =>
        new(year, month, day, hour, minute, second, DateTimeKind.Utc);

    [Fact]
    public void Daily_reset_is_15_utc_today_or_tomorrow()
    {
        Assert.Equal(Utc(2026, 10, 4, 15, 0), ResetClock.NextDaily(Utc(2026, 10, 4, 11, 48)));
        Assert.Equal(Utc(2026, 10, 5, 15, 0), ResetClock.NextDaily(Utc(2026, 10, 4, 15, 0)));
        Assert.Equal(Utc(2026, 10, 5, 15, 0), ResetClock.NextDaily(Utc(2026, 10, 4, 22, 0)));
    }

    [Fact]
    public void Weekly_reset_is_tuesday_08_utc()
    {
        // 4 October 2026 is a Sunday.
        Assert.Equal(DayOfWeek.Sunday, Utc(2026, 10, 4, 0, 0).DayOfWeek);
        Assert.Equal(Utc(2026, 10, 6, 8, 0), ResetClock.NextWeekly(Utc(2026, 10, 4, 12, 0)));
        Assert.Equal(Utc(2026, 10, 13, 8, 0), ResetClock.NextWeekly(Utc(2026, 10, 6, 8, 0)));
        Assert.Equal(Utc(2026, 10, 6, 8, 0), ResetClock.NextWeekly(Utc(2026, 10, 6, 7, 59)));
    }

    [Fact]
    public void Next_is_whichever_comes_first()
    {
        // Tuesday 06:00: weekly at 08:00 comes before daily at 15:00.
        var (kind, left) = ResetClock.Next(Utc(2026, 10, 6, 6, 0));
        Assert.Equal(ResetKind.Weekly, kind);
        Assert.Equal(TimeSpan.FromHours(2), left);

        // Sunday 11:48: daily in 3 h 12 m, the render's line.
        (kind, left) = ResetClock.Next(Utc(2026, 10, 4, 11, 48));
        Assert.Equal(ResetKind.Daily, kind);
        Assert.Equal("daily in 3 h 12 m", SummaryText.ResetLine(kind, ResetClock.MinutesLeft(left)));
    }

    [Fact]
    public void Minutes_round_up_and_never_reach_zero()
    {
        Assert.Equal(1, ResetClock.MinutesLeft(TimeSpan.FromSeconds(1)));
        Assert.Equal(1, ResetClock.MinutesLeft(TimeSpan.Zero));
        Assert.Equal(2, ResetClock.MinutesLeft(TimeSpan.FromSeconds(61)));
        Assert.Equal(192, ResetClock.MinutesLeft(new TimeSpan(3, 11, 20)));
    }
}
