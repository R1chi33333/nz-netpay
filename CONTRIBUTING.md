# Contributing

Thanks for your interest. This project is small and contributions are welcome.

## Setup

```bash
dotnet restore
dotnet run --project src/NzNetpay.Api   # API + playground on http://localhost:5000
dotnet test                             # unit + integration tests
dotnet format                           # apply code style before committing
```

## Rules

- Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`, `ci:`). Releases are cut automatically from commit messages.
- `dotnet format --verify-no-changes`, `dotnet build` and `dotnet test` must pass before a PR.
- The tax engine must stay pure: no I/O, no clock reads, no configuration lookups inside calculation code. Every rate constant carries a citation to its IRD or ACC source.
- No emoji anywhere: code, comments, docs, commit messages.
