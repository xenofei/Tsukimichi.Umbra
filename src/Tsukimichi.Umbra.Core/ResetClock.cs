namespace Tsukimichi.Umbra.Core;

/// <summary>Which reset comes next.</summary>
public enum ResetKind
{
    Daily,
    Weekly,
}

/// <summary>
/// The game's reset times (spec-1.22 A2, "Next reset"): the daily reset at 15:00 UTC and the weekly reset on Tuesday at
/// 08:00 UTC. Pure, so it needs no game and no Tsukimichi.
/// </summary>
public static class ResetClock
{
    public static readonly TimeSpan DailyResetUtc = new(15, 0, 0);
    public static readonly TimeSpan WeeklyResetUtc = new(8, 0, 0);
    public const DayOfWeek WeeklyResetDay = DayOfWeek.Tuesday;

    /// <summary>The next daily reset strictly after <paramref name="nowUtc"/>.</summary>
    public static DateTime NextDaily(DateTime nowUtc)
    {
        var today = nowUtc.Date + DailyResetUtc;
        return today > nowUtc ? today : today.AddDays(1);
    }

    /// <summary>The next weekly reset strictly after <paramref name="nowUtc"/>.</summary>
    public static DateTime NextWeekly(DateTime nowUtc)
    {
        var days = ((int)WeeklyResetDay - (int)nowUtc.DayOfWeek + 7) % 7;
        var candidate = nowUtc.Date.AddDays(days) + WeeklyResetUtc;
        return candidate > nowUtc ? candidate : candidate.AddDays(7);
    }

    /// <summary>The reset that comes first after <paramref name="nowUtc"/>, and how long until it (the daily on a tie).</summary>
    public static (ResetKind Kind, TimeSpan Left) Next(DateTime nowUtc)
    {
        var daily = NextDaily(nowUtc);
        var weekly = NextWeekly(nowUtc);
        return weekly < daily ? (ResetKind.Weekly, weekly - nowUtc) : (ResetKind.Daily, daily - nowUtc);
    }

    /// <summary>Whole minutes left, rounded up, so the last half minute still reads "1 m" and never "0 m".</summary>
    public static int MinutesLeft(TimeSpan left) => Math.Max(1, (int)Math.Ceiling(left.TotalMinutes));
}
