# nz-netpay — New Zealand take-home pay API

[![CI](https://github.com/R1chi33333/nz-netpay/actions/workflows/ci.yml/badge.svg)](https://github.com/R1chi33333/nz-netpay/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/R1chi33333/nz-netpay)](https://github.com/R1chi33333/nz-netpay/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-white.svg)](LICENSE)

ASP.NET Core 8 API that turns a salary into a full deduction breakdown — PAYE income tax, ACC earners' levy, KiwiSaver and student loan — using only official IRD and ACC published rates, each cited to its source.

Live demo: Azure deployment in progress — run locally with one command below.

## Why this exists

Every pay calculator for New Zealand is a closed web page. There is no small, tested, open API you can call from your own budgeting tool, spreadsheet or app. nz-netpay is that API: pure calculation, documented rate sources, no accounts, no tracking. General information, not financial advice.

## Features

- Take-home breakdown: PAYE by bracket, ACC levy with the annual cap, KiwiSaver, student loan
- Results split per pay period: annual, monthly, fortnightly, weekly, hourly
- Tax years as data: 2025-26 and 2026-27 ship today, a new year is a record, not a code change
- Every rate cited: `/v1/rates` returns the tables with their IRD and ACC source URLs
- GST helper: add or remove 15 percent
- Playground UI, Swagger docs at `/docs`, RFC 7807 errors, rate limiting

## Architecture

```
wwwroot/index.html          Playground (vanilla JS, calls the API)
        |
Api/Endpoints.cs            /v1/take-home  /v1/gst  /v1/rates
        |                   validation, Problem Details, rate limit
Engine/                     pure calculators, no I/O
  TaxYearRates.cs           rate records + source URLs per tax year
  TakeHomeCalculator.cs     PAYE brackets, ACC cap, KiwiSaver, student loan
  GstCalculator.cs          15 percent add / remove
  PayPeriods.cs             annual -> monthly / fortnightly / weekly / hourly
```

## Rate sources (2026-27)

| Figure | Value | Source |
|---|---|---|
| PAYE brackets | 10.5% to $15,600 · 17.5% to $53,500 · 30% to $78,100 · 33% to $180,000 · 39% above | [IRD tax rates](https://www.ird.govt.nz/income-tax/income-tax-for-individuals/tax-codes-and-tax-rates-for-individuals/tax-rates-for-individuals) |
| ACC earners' levy | 1.75%, max liable earnings $156,641 | [IRD ACC levy rates](https://www.ird.govt.nz/income-tax/income-tax-for-individuals/acc-clients-and-carers/acc-earners-levy-rates) |
| Student loan | 12% over $24,128 | [IRD student loans](https://www.ird.govt.nz/student-loans/living-in-new-zealand-with-a-student-loan/repaying-my-student-loan-when-i-earn-salary-or-wages) |
| KiwiSaver employee rates | 3.5 (default), 4, 6, 8, 10 percent; 3 percent temporary reduction | [IRD KiwiSaver rates](https://www.ird.govt.nz/kiwisaver/kiwisaver-individuals/making-changes-to-my-kiwisaver/changing-my-kiwisaver-contribution-rate) |
| GST | 15 percent | [IRD GST](https://www.ird.govt.nz/gst) |

## Tech Stack

ASP.NET Core 8 (minimal APIs) · xUnit + WebApplicationFactory · Swashbuckle · GitHub Actions · semantic-release · Docker

## Getting Started

```bash
dotnet run --project src/NzNetpay.Api
# playground on the printed localhost URL, docs at /docs
```

Or with Docker:

```bash
docker build -t nz-netpay . && docker run -p 8080:8080 nz-netpay
```

Example:

```bash
curl "localhost:8080/v1/take-home?salary=70000&kiwiSaverRate=0.035&studentLoan=true"
```

## Testing

```bash
dotnet test
```

38 tests. Engine coverage: 97 percent line, 94 percent branch. PAYE numbers are asserted against hand calculations from the IRD bracket table, ACC against the published maximum levy, student loan against the IRD worked example.

## Roadmap

See [ROADMAP.md](ROADMAP.md).

## License

[MIT](LICENSE)
