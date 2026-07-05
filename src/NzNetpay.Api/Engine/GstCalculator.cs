namespace NzNetpay.Api.Engine;

public sealed record GstBreakdown(decimal Exclusive, decimal Gst, decimal Inclusive);

public static class GstCalculator
{
    /// <summary>
    /// GST rate of 15 percent, in force since 1 October 2010.
    /// https://www.ird.govt.nz/gst
    /// </summary>
    public const decimal Rate = 0.15m;

    public static GstBreakdown AddGst(decimal exclusiveAmount)
    {
        Validate(exclusiveAmount);
        var gst = RoundCents(exclusiveAmount * Rate);
        return new GstBreakdown(exclusiveAmount, gst, exclusiveAmount + gst);
    }

    public static GstBreakdown RemoveGst(decimal inclusiveAmount)
    {
        Validate(inclusiveAmount);
        var exclusive = RoundCents(inclusiveAmount / (1m + Rate));
        return new GstBreakdown(exclusive, inclusiveAmount - exclusive, inclusiveAmount);
    }

    private static void Validate(decimal amount)
    {
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must not be negative.");
        }
    }

    private static decimal RoundCents(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
