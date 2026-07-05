namespace NzNetpay.Api.Engine;

/// <summary>
/// One progressive PAYE bracket: income up to <paramref name="UpTo"/>
/// (inclusive) is taxed at <paramref name="Rate"/> for the dollars that
/// fall inside the bracket.
/// </summary>
public sealed record TaxBracket(decimal UpTo, decimal Rate);

/// <summary>
/// Every number the engine uses for one tax year, with the official
/// source URL for each figure. Values are data, not behaviour, so a new
/// tax year is a new record, never a code change.
/// </summary>
public sealed record TaxYearRates
{
    public required string TaxYear { get; init; }
    public required DateOnly From { get; init; }
    public required DateOnly To { get; init; }
    public required IReadOnlyList<TaxBracket> PayeBrackets { get; init; }
    public required decimal AccLevyRate { get; init; }
    public required decimal AccMaxLiableEarnings { get; init; }
    public required IReadOnlyList<decimal> KiwiSaverEmployeeRates { get; init; }
    public required decimal StudentLoanRate { get; init; }
    public required decimal StudentLoanAnnualThreshold { get; init; }
    public required IReadOnlyDictionary<string, string> Sources { get; init; }
}

public static class RateTables
{
    private const string PayeSource =
        "https://www.ird.govt.nz/income-tax/income-tax-for-individuals/tax-codes-and-tax-rates-for-individuals/tax-rates-for-individuals";

    private const string AccSource =
        "https://www.ird.govt.nz/income-tax/income-tax-for-individuals/acc-clients-and-carers/acc-earners-levy-rates";

    private const string StudentLoanSource =
        "https://www.ird.govt.nz/student-loans/living-in-new-zealand-with-a-student-loan/repaying-my-student-loan-when-i-earn-salary-or-wages";

    private const string KiwiSaverSource =
        "https://www.ird.govt.nz/kiwisaver/kiwisaver-individuals/making-changes-to-my-kiwisaver/changing-my-kiwisaver-contribution-rate";

    // IRD "From 1 April 2025" brackets, unchanged for 2026-27.
    private static readonly TaxBracket[] PayeFrom1April2025 =
    [
        new(15_600m, 0.105m),
        new(53_500m, 0.175m),
        new(78_100m, 0.30m),
        new(180_000m, 0.33m),
        new(decimal.MaxValue, 0.39m),
    ];

    public static readonly TaxYearRates TaxYear2027 = new()
    {
        TaxYear = "2026-27",
        From = new DateOnly(2026, 4, 1),
        To = new DateOnly(2027, 3, 31),
        PayeBrackets = PayeFrom1April2025,
        // "1 April 2026 to 31 March 2027: $1.75 per $100 (1.75%)"
        AccLevyRate = 0.0175m,
        // "1 April 2026 to 31 March 2027: $156,641 (max levy $2,741.22)"
        AccMaxLiableEarnings = 156_641m,
        // Standard rates from 1 April 2026; 3% remains as a temporary
        // rate reduction employees can apply for.
        KiwiSaverEmployeeRates = [0.03m, 0.035m, 0.04m, 0.06m, 0.08m, 0.10m],
        StudentLoanRate = 0.12m,
        StudentLoanAnnualThreshold = 24_128m,
        Sources = new Dictionary<string, string>
        {
            ["paye"] = PayeSource,
            ["acc"] = AccSource,
            ["studentLoan"] = StudentLoanSource,
            ["kiwiSaver"] = KiwiSaverSource,
        },
    };

    public static readonly TaxYearRates TaxYear2026 = new()
    {
        TaxYear = "2025-26",
        From = new DateOnly(2025, 4, 1),
        To = new DateOnly(2026, 3, 31),
        PayeBrackets = PayeFrom1April2025,
        // "1 April 2025 to 31 March 2026: $1.67 per $100 (1.67%)"
        AccLevyRate = 0.0167m,
        // "1 April 2025 to 31 March 2026: $152,790 (max levy $2,551.59)"
        AccMaxLiableEarnings = 152_790m,
        KiwiSaverEmployeeRates = [0.03m, 0.04m, 0.06m, 0.08m, 0.10m],
        StudentLoanRate = 0.12m,
        StudentLoanAnnualThreshold = 24_128m,
        Sources = new Dictionary<string, string>
        {
            ["paye"] = PayeSource,
            ["acc"] = AccSource,
            ["studentLoan"] = StudentLoanSource,
            ["kiwiSaver"] = KiwiSaverSource,
        },
    };

    public static readonly IReadOnlyList<TaxYearRates> All = [TaxYear2027, TaxYear2026];

    public static TaxYearRates Latest => TaxYear2027;

    public static TaxYearRates? Find(string taxYear) =>
        All.FirstOrDefault(y => string.Equals(y.TaxYear, taxYear, StringComparison.Ordinal));
}
