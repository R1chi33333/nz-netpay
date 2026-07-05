# Roadmap

## v0.1.0 — Scaffold (LOOP 1)

- [x] Solution scaffold: API + test projects, nullable + warnings-as-errors
- [x] Health endpoint and dark placeholder page
- [x] Integration test harness (WebApplicationFactory)
- [x] CI: format check, build, test, coverage upload
- [x] Dockerfile (multi-stage, non-root)
- [x] CI green on GitHub

## v0.2.0 — Core tax engine

- [ ] Rate tables for 2025-26 as data records, each value cited to its IRD/ACC source URL
- [ ] PAYE income tax: progressive bracket calculation
- [ ] ACC earners' levy with earnings cap
- [ ] KiwiSaver employee contribution (3/4/6/8/10 percent)
- [ ] Student loan repayment (12 percent over annual threshold)
- [ ] Take-home breakdown per period: annual, monthly, fortnightly, weekly, hourly
- [ ] Unit tests against IRD worked examples, coverage at least 90 percent on the engine

## v0.3.0 — API surface

- [ ] GET /v1/take-home: salary + options to full deduction breakdown
- [ ] GET /v1/gst: add or remove 15 percent GST
- [ ] GET /v1/rates: the rate tables with source citations
- [ ] Problem Details error responses, input validation
- [ ] OpenAPI document + Swagger UI at /docs
- [ ] Rate limiting middleware

## v1.0.0 — Playground + deploy

- [ ] Playground page: salary input to instant breakdown table (calls the API)
- [ ] Azure deployment, live URL in README
- [ ] README: badges, architecture diagram, rate source table
- [ ] Portfolio sync: card on r1chi33333.github.io + repo topics
