# nz-netpay — New Zealand take-home pay API

[![CI](https://github.com/R1chi33333/nz-netpay/actions/workflows/ci.yml/badge.svg)](https://github.com/R1chi33333/nz-netpay/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-white.svg)](LICENSE)

ASP.NET Core 8 API that turns a salary into a full deduction breakdown — PAYE income tax, ACC earners' levy, KiwiSaver and student loan — using only official IRD and ACC published rates, each cited to its source.

Live demo: coming with v1.0.0.

## Why this exists

Every pay calculator for New Zealand is a closed web page. There is no small, tested, open API you can call from your own budgeting tool, spreadsheet or app. nz-netpay is that API: pure calculation, documented rate sources, no accounts, no tracking.

## Status

Under construction — see [ROADMAP.md](ROADMAP.md). Currently at LOOP 1: scaffold, CI and deploy chain.

## Getting Started

```bash
dotnet run --project src/NzNetpay.Api
# http://localhost:5000/healthz
```

Or with Docker:

```bash
docker build -t nz-netpay . && docker run -p 8080:8080 nz-netpay
```

## Testing

```bash
dotnet test
```

## License

[MIT](LICENSE)
