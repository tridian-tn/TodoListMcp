using TodoListMcp.Core.Model;

namespace TodoListMcp.Core.Tests;

public class TimeUnitsTests
{
    [Theory]
    [InlineData("h", TimeUnit.Hours)]
    [InlineData("Hours", TimeUnit.Hours)]
    [InlineData("HRS", TimeUnit.Hours)]
    [InlineData("i", TimeUnit.Minutes)]
    [InlineData("min", TimeUnit.Minutes)]
    [InlineData("minutes", TimeUnit.Minutes)]
    [InlineData("m", TimeUnit.Months)]      // M is months, not minutes
    [InlineData("month", TimeUnit.Months)]
    [InlineData("d", TimeUnit.Days)]
    [InlineData("k", TimeUnit.Weekdays)]
    [InlineData("weekdays", TimeUnit.Weekdays)]
    [InlineData("w", TimeUnit.Weeks)]
    [InlineData("y", TimeUnit.Years)]
    [InlineData("  Days  ", TimeUnit.Days)]
    public void TryParse_accepts_letters_and_words(string input, TimeUnit expected)
    {
        Assert.True(TimeUnits.TryParse(input, out var unit));
        Assert.Equal(expected, unit);
    }

    [Theory]
    [InlineData("s")]         // seconds aren't valid for estimate/spent
    [InlineData("seconds")]
    [InlineData("fortnight")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_rejects_unknown_units(string? input)
    {
        Assert.False(TimeUnits.TryParse(input, out _));
    }

    [Fact]
    public void File_codes_round_trip()
    {
        foreach (TimeUnit u in Enum.GetValues<TimeUnit>())
            Assert.Equal(u, TimeUnits.FromFileCode(TimeUnits.ToFileCode(u)));
    }

    [Theory]
    [InlineData(120, TimeUnit.Minutes, 2)]
    [InlineData(1, TimeUnit.Hours, 1)]
    [InlineData(1, TimeUnit.Days, 8)]        // 8h working day
    [InlineData(1, TimeUnit.Weekdays, 8)]
    [InlineData(1, TimeUnit.Weeks, 40)]      // 5 working days
    public void ToHours_uses_work_time_convention(double value, TimeUnit unit, double expected)
    {
        Assert.Equal(expected, TimeUnits.ToHours(value, unit), precision: 6);
    }
}
