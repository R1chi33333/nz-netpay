namespace NzNetpay.Api.Engine;

public sealed record TakeHomeBreakdown
{
    public required decimal GrossSalary { get; init; }
    public required decimal Paye { get; init; }
    public required decimal AccLevy { get; init; }
    public required decimal KiwiSaverEmployee { get; init; }
    public required decimal StudentLoan { get; init; }
    public required decimal TakeHome { get; init; }
    public required IReadOnlyList<BracketLine> PayeByBracket { get; init; }
    public required string TaxYear { get; init; }

    public decimal TotalDeductions => Paye + AccLevy + KiwiSaverEmployee + StudentLoan;

    /// <summary>Effective overall tax rate (PAYE + ACC only), 0 for zero income.</summary>
    public decimal EffectiveTaxRate =>
        GrossSalary == 0m ? 0m : Math.Round((Paye + AccLevy) / GrossSalary, 4);
}

/// <summary>PAYE charged within a single bracket, for the breakdown table.</summary>
public sealed record BracketLine(decimal From, decimal To, decimal Rate, decimal Tax);

public static class TakeHomeCalculator
{
    /// <summary>
    /// Annual take-home pay from a gross salary. Pure: rates in, numbers
    /// out. All amounts are rounded to the cent, away from zero, the way
    /// payroll rounds.
    /// </summary>
    /// <param name="grossSalary">Annual gross salary in dollars. Must not be negative.</param>
    /// <param name="rates">The tax year to calculate under.</param>
    /// <param name="kiwiSaverRate">Employee contribution rate, or null when not enrolled. Must be one of the year's valid rates.</param>
    /// <param name="hasStudentLoan">Whether 12 percent student loan repayments apply over the threshold.</param>
    public static TakeHomeBreakdown Calculate(
        decimal grossSalary,
        TaxYearRates rates,
        decimal? kiwiSaverRate = null,
        bool hasStudentLoan = false)
    {
        ArgumentNullException.ThrowIfNull(rates);
        if (grossSalary < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(grossSalary), grossSalary, "Salary must not be negative.");
        }

        if (kiwiSaverRate is decimal rate && !rates.KiwiSaverEmployeeRates.Contains(rate))
        {
            var valid = string.Join(", ", rates.KiwiSaverEmployeeRates);
            throw new ArgumentOutOfRangeException(
                nameof(kiwiSaverRate),
                rate,
                $"KiwiSaver rate must be one of: {valid}.");
        }

        var payeLines = PayeByBracket(grossSalary, rates.PayeBrackets);
        var paye = payeLines.Sum(l => l.Tax);
        var accLevy = RoundCents(Math.Min(grossSalary, rates.AccMaxLiableEarnings) * rates.AccLevyRate);
        var kiwiSaver = RoundCents(grossSalary * (kiwiSaverRate ?? 0m));
        var studentLoan = hasStudentLoan
            ? RoundCents(Math.Max(0m, grossSalary - rates.StudentLoanAnnualThreshold) * rates.StudentLoanRate)
            : 0m;

        return new TakeHomeBreakdown
        {
            GrossSalary = grossSalary,
            Paye = paye,
            AccLevy = accLevy,
            KiwiSaverEmployee = kiwiSaver,
            StudentLoan = studentLoan,
            TakeHome = grossSalary - paye - accLevy - kiwiSaver - studentLoan,
            PayeByBracket = payeLines,
            TaxYear = rates.TaxYear,
        };
    }

    private static List<BracketLine> PayeByBracket(decimal salary, IReadOnlyList<TaxBracket> brackets)
    {
        var lines = new List<BracketLine>();
        var lower = 0m;
        foreach (var bracket in brackets)
        {
            if (salary <= lower)
            {
                break;
            }

            var taxableHere = Math.Min(salary, bracket.UpTo) - lower;
            lines.Add(new BracketLine(lower, bracket.UpTo, bracket.Rate, RoundCents(taxableHere * bracket.Rate)));
            lower = bracket.UpTo;
        }

        return lines;
    }

    private static decimal RoundCents(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
