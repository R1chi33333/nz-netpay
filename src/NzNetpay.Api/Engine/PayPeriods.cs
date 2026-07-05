namespace NzNetpay.Api.Engine;

/// <summary>An annual amount expressed per pay period.</summary>
public sealed record PeriodAmounts(
    decimal Annual,
    decimal Monthly,
    decimal Fortnightly,
    decimal Weekly,
    decimal Hourly);

public static class PayPeriods
{
    /// <summary>
    /// Standard full-time week used for the hourly figure; stated next
    /// to the number wherever it is shown.
    /// </summary>
    public const decimal HoursPerWeek = 40m;

    private const decimal WeeksPerYear = 52m;

    public static PeriodAmounts Split(decimal annual)
    {
        if (annual < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(annual), annual, "Amount must not be negative.");
        }

        return new PeriodAmounts(
            Annual: RoundCents(annual),
            Monthly: RoundCents(annual / 12m),
            Fortnightly: RoundCents(annual / (WeeksPerYear / 2m)),
            Weekly: RoundCents(annual / WeeksPerYear),
            Hourly: RoundCents(annual / (WeeksPerYear * HoursPerWeek)));
    }

    private static decimal RoundCents(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
