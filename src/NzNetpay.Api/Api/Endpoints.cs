using Microsoft.AspNetCore.Http.HttpResults;
using NzNetpay.Api.Engine;

namespace NzNetpay.Api.Api;

public static class Endpoints
{
    private const decimal MaxSalary = 10_000_000m;

    public static IEndpointRouteBuilder MapNetpayApi(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/v1").RequireRateLimiting("api");

        v1.MapGet("/take-home", TakeHome)
            .WithName("GetTakeHome")
            .WithSummary("Take-home pay breakdown for an annual salary")
            .WithDescription(
                "PAYE, ACC earners' levy, optional KiwiSaver and student loan "
                + "deductions under the selected tax year, with the result split "
                + "per pay period.");

        v1.MapGet("/gst", Gst)
            .WithName("GetGst")
            .WithSummary("Add or remove 15 percent GST");

        v1.MapGet("/rates", Rates)
            .WithName("GetRates")
            .WithSummary("Rate tables used by this API, with official source URLs");

        return app;
    }

    private static Results<Ok<TakeHomeResponse>, ProblemHttpResult> TakeHome(
        decimal salary,
        decimal? kiwiSaverRate,
        bool studentLoan = false,
        string? taxYear = null)
    {
        if (salary is < 0m or > MaxSalary)
        {
            return Problem($"salary must be between 0 and {MaxSalary}.");
        }

        var rates = taxYear is null ? RateTables.Latest : RateTables.Find(taxYear);
        if (rates is null)
        {
            var known = string.Join(", ", RateTables.All.Select(y => y.TaxYear));
            return Problem($"Unknown tax year '{taxYear}'. Available: {known}.");
        }

        if (kiwiSaverRate is decimal rate && !rates.KiwiSaverEmployeeRates.Contains(rate))
        {
            var valid = string.Join(", ", rates.KiwiSaverEmployeeRates);
            return Problem($"kiwiSaverRate must be one of: {valid}.");
        }

        var breakdown = TakeHomeCalculator.Calculate(salary, rates, kiwiSaverRate, studentLoan);

        return TypedResults.Ok(new TakeHomeResponse(
            TaxYear: breakdown.TaxYear,
            GrossSalary: breakdown.GrossSalary,
            KiwiSaverRate: kiwiSaverRate,
            StudentLoan: studentLoan,
            Deductions: new DeductionsDto(
                Paye: breakdown.Paye,
                AccLevy: breakdown.AccLevy,
                KiwiSaverEmployee: breakdown.KiwiSaverEmployee,
                StudentLoan: breakdown.StudentLoan,
                Total: breakdown.TotalDeductions),
            PayeByBracket: breakdown.PayeByBracket,
            TakeHome: PayPeriods.Split(breakdown.TakeHome),
            EffectiveTaxRate: breakdown.EffectiveTaxRate,
            HoursPerWeekForHourly: PayPeriods.HoursPerWeek));
    }

    private static Results<Ok<GstBreakdown>, ProblemHttpResult> Gst(
        decimal amount,
        string direction = "add")
    {
        if (amount is < 0m or > MaxSalary)
        {
            return Problem($"amount must be between 0 and {MaxSalary}.");
        }

        return direction switch
        {
            "add" => TypedResults.Ok(GstCalculator.AddGst(amount)),
            "remove" => TypedResults.Ok(GstCalculator.RemoveGst(amount)),
            _ => Problem("direction must be 'add' or 'remove'."),
        };
    }

    private static Ok<RatesResponse> Rates() =>
        TypedResults.Ok(new RatesResponse(RateTables.All, GstCalculator.Rate));

    private static ProblemHttpResult Problem(string detail) =>
        TypedResults.Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest);
}
