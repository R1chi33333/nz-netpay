using NzNetpay.Api.Engine;

namespace NzNetpay.Tests.Engine;

public sealed class GstCalculatorTests
{
    [Fact]
    public void Add_gst_to_100_gives_115()
    {
        var result = GstCalculator.AddGst(100m);

        Assert.Equal(100m, result.Exclusive);
        Assert.Equal(15m, result.Gst);
        Assert.Equal(115m, result.Inclusive);
    }

    [Fact]
    public void Remove_gst_from_115_gives_100()
    {
        var result = GstCalculator.RemoveGst(115m);

        Assert.Equal(100m, result.Exclusive);
        Assert.Equal(15m, result.Gst);
        Assert.Equal(115m, result.Inclusive);
    }

    [Fact]
    public void Remove_gst_rounds_to_the_cent()
    {
        var result = GstCalculator.RemoveGst(10m);

        // 10 / 1.15 = 8.6956... rounds to 8.70
        Assert.Equal(8.70m, result.Exclusive);
        Assert.Equal(1.30m, result.Gst);
    }

    [Fact]
    public void Parts_always_sum_to_inclusive()
    {
        foreach (var amount in new[] { 0.01m, 1m, 9.99m, 123.45m, 99_999.99m })
        {
            var added = GstCalculator.AddGst(amount);
            var removed = GstCalculator.RemoveGst(amount);

            Assert.Equal(added.Inclusive, added.Exclusive + added.Gst);
            Assert.Equal(removed.Inclusive, removed.Exclusive + removed.Gst);
        }
    }

    [Fact]
    public void Zero_is_valid_and_negative_is_rejected()
    {
        Assert.Equal(0m, GstCalculator.AddGst(0m).Inclusive);
        Assert.Throws<ArgumentOutOfRangeException>(() => GstCalculator.AddGst(-0.01m));
        Assert.Throws<ArgumentOutOfRangeException>(() => GstCalculator.RemoveGst(-5m));
    }
}
