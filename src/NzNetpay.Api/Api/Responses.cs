using NzNetpay.Api.Engine;

namespace NzNetpay.Api.Api;

/// <summary>Response for GET /v1/take-home.</summary>
public sealed record TakeHomeResponse(
    string TaxYear,
    decimal GrossSalary,
    decimal? KiwiSaverRate,
    bool StudentLoan,
    DeductionsDto Deductions,
    IReadOnlyList<BracketLine> PayeByBracket,
    PeriodAmounts TakeHome,
    decimal EffectiveTaxRate,
    decimal HoursPerWeekForHourly);

public sealed record DeductionsDto(
    decimal Paye,
    decimal AccLevy,
    decimal KiwiSaverEmployee,
    decimal StudentLoan,
    decimal Total);

/// <summary>Response for GET /v1/rates.</summary>
public sealed record RatesResponse(IReadOnlyList<TaxYearRates> TaxYears, decimal GstRate);
