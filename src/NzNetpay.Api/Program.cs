using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using NzNetpay.Api.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "nz-netpay",
        Version = "v1",
        Description =
            "New Zealand take-home pay API. PAYE, ACC earners' levy, KiwiSaver "
            + "and student loan deductions from official IRD and ACC rates. "
            + "General information, not financial advice.",
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "docs";
    options.DocumentTitle = "nz-netpay API docs";
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapNetpayApi();

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
