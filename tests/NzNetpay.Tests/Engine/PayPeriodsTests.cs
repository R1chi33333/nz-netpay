using NzNetpay.Api.Engine;

namespace NzNetpay.Tests.Engine;

public sealed class PayPeriodsTests
{
    [Fact]
    public void Splits_52_000_into_round_period_amounts()
    {
        var periods = PayPeriods.Split(52_000m);

        Assert.Equal(52_000m, periods.Annual);
        Assert.Equal(4_333.33m, periods.Monthly);
        Assert.Equal(2_000m, periods.Fortnightly);
        Assert.Equal(1_000m, periods.Weekly);
        Assert.Equal(25m, periods.Hourly); // 40-hour week
    }

    [Fact]
    public void Zero_stays_zero_everywhere()
    {
        var periods = PayPeriods.Split(0m);

        Assert.Equal(0m, periods.Monthly);
        Assert.Equal(0m, periods.Hourly);
    }

    [Fact]
    public void Negative_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PayPeriods.Split(-1m));
    }
}
