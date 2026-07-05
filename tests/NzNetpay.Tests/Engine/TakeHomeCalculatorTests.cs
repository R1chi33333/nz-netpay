using NzNetpay.Api.Engine;

namespace NzNetpay.Tests.Engine;

public sealed class TakeHomeCalculatorTests
{
    private static readonly TaxYearRates Y27 = RateTables.TaxYear2027;
    private static readonly TaxYearRates Y26 = RateTables.TaxYear2026;

    // Hand-computed from the IRD "From 1 April 2025" bracket table:
    // 15,600 * 10.5% = 1,638.00
    // (53,500 - 15,600) * 17.5% = 6,632.50
    // (60,000 - 53,500) * 30% = 1,950.00
    [Fact]
    public void Paye_on_60k_matches_hand_calculation()
    {
        var result = TakeHomeCalculator.Calculate(60_000m, Y27);

        Assert.Equal(1_638.00m + 6_632.50m + 1_950.00m, result.Paye);
    }

    [Theory]
    [InlineData(15_600, 1_638.00)] // top of the 10.5% bracket exactly
    [InlineData(53_500, 8_270.50)] // 1,638 + 37,900 * 17.5%
    [InlineData(78_100, 15_650.50)] // + 24,600 * 30%
    [InlineData(180_000, 49_277.50)] // + 101,900 * 33%
    [InlineData(200_000, 57_077.50)] // + 20,000 * 39%
    public void Paye_at_bracket_boundaries(decimal salary, decimal expectedPaye)
    {
        var result = TakeHomeCalculator.Calculate(salary, Y27);

        Assert.Equal(expectedPaye, result.Paye);
    }

    [Fact]
    public void Zero_salary_produces_all_zero_lines()
    {
        var result = TakeHomeCalculator.Calculate(0m, Y27, kiwiSaverRate: 0.04m, hasStudentLoan: true);

        Assert.Equal(0m, result.Paye);
        Assert.Equal(0m, result.AccLevy);
        Assert.Equal(0m, result.KiwiSaverEmployee);
        Assert.Equal(0m, result.StudentLoan);
        Assert.Equal(0m, result.TakeHome);
        Assert.Equal(0m, result.EffectiveTaxRate);
    }

    // IRD: 2026-27 earners' levy is 1.75% capped at $156,641 liable
    // earnings, maximum levy $2,741.22.
    [Fact]
    public void Acc_levy_caps_at_published_maximum()
    {
        var atCap = TakeHomeCalculator.Calculate(156_641m, Y27);
        var aboveCap = TakeHomeCalculator.Calculate(500_000m, Y27);

        Assert.Equal(2_741.22m, atCap.AccLevy);
        Assert.Equal(2_741.22m, aboveCap.AccLevy);
    }

    // IRD: 2025-26 maximum levy is $2,551.59 at $152,790.
    [Fact]
    public void Acc_levy_uses_the_selected_tax_year()
    {
        var result = TakeHomeCalculator.Calculate(200_000m, Y26);

        Assert.Equal(2_551.59m, result.AccLevy);
    }

    [Theory]
    [InlineData(50_000, 0.03, 1_500.00)]
    [InlineData(50_000, 0.035, 1_750.00)]
    [InlineData(50_000, 0.10, 5_000.00)]
    public void KiwiSaver_deducts_flat_percentage_of_gross(
        decimal salary, decimal rate, decimal expected)
    {
        var result = TakeHomeCalculator.Calculate(salary, Y27, kiwiSaverRate: rate);

        Assert.Equal(expected, result.KiwiSaverEmployee);
    }

    [Fact]
    public void KiwiSaver_rate_not_valid_for_year_is_rejected()
    {
        // 3.5% only exists from 1 April 2026; it is not a 2025-26 rate.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TakeHomeCalculator.Calculate(50_000m, Y26, kiwiSaverRate: 0.035m));
    }

    // IRD example: 12% of every dollar over the $24,128 annual threshold.
    [Fact]
    public void Student_loan_applies_only_above_threshold()
    {
        var below = TakeHomeCalculator.Calculate(24_128m, Y27, hasStudentLoan: true);
        var above = TakeHomeCalculator.Calculate(60_000m, Y27, hasStudentLoan: true);

        Assert.Equal(0m, below.StudentLoan);
        Assert.Equal((60_000m - 24_128m) * 0.12m, above.StudentLoan);
    }

    [Fact]
    public void Student_loan_is_zero_when_not_opted_in()
    {
        var result = TakeHomeCalculator.Calculate(60_000m, Y27, hasStudentLoan: false);

        Assert.Equal(0m, result.StudentLoan);
    }

    [Fact]
    public void Take_home_is_gross_minus_all_deductions()
    {
        var result = TakeHomeCalculator.Calculate(
            85_000m, Y27, kiwiSaverRate: 0.04m, hasStudentLoan: true);

        Assert.Equal(
            result.GrossSalary - result.Paye - result.AccLevy
                - result.KiwiSaverEmployee - result.StudentLoan,
            result.TakeHome);
        Assert.Equal(result.Paye + result.AccLevy + result.KiwiSaverEmployee + result.StudentLoan, result.TotalDeductions);
    }

    [Fact]
    public void Bracket_lines_sum_to_total_paye()
    {
        var result = TakeHomeCalculator.Calculate(123_456m, Y27);

        Assert.Equal(result.Paye, result.PayeByBracket.Sum(l => l.Tax));
        Assert.Equal(4, result.PayeByBracket.Count);
    }

    [Fact]
    public void Negative_salary_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TakeHomeCalculator.Calculate(-1m, Y27));
    }

    [Fact]
    public void Take_home_never_exceeds_gross_and_never_goes_negative()
    {
        foreach (var salary in new[] { 1m, 24_128m, 53_500m, 100_000m, 156_641m, 1_000_000m })
        {
            var result = TakeHomeCalculator.Calculate(
                salary, Y27, kiwiSaverRate: 0.10m, hasStudentLoan: true);

            Assert.InRange(result.TakeHome, 0m, salary);
        }
    }
}
