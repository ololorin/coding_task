# CLAUDE.md

Guidance for working in this repository. Keep this file in sync when decisions or
constraints change.

## What this is

A currency-to-words converter: turns a dollar amount (max `999,999,999.99`) into written
words in **English** and **German**. Client–server: an ASP.NET Core API (.NET 10) does all
conversion server-side, and a React 19 + Vite SPA consumes it. See [README.md](README.md)
for build/run instructions and [docs/PROMPTS.md](docs/PROMPTS.md) for the AI prompt history
(transparency requirement — keep appending prompts there).

## Layout

```
backend/
  CurrencyConverter.slnx                 # solution (lives in backend/, NOT repo root)
  src/CurrencyConverter.Domain/          # pure conversion logic, no ASP.NET dependency
  src/CurrencyConverter.Api/             # controllers, localization, auth, CORS
  tests/CurrencyConverter.UnitTests/     # xUnit: spellers, converter, validation
  tests/CurrencyConverter.E2E/           # xUnit + WebApplicationFactory: HTTP happy-path
  Dockerfile                             # API image
frontend/                                # React 19 + Vite + TS SPA (Dockerfile, nginx.conf)
docker-compose.yml                       # bootstraps API + UI for local deployment
```

`dotnet` commands run from `backend/` (the solution is there, not the repo root).

## Functional constraints (from the task spec — do not break)

- Range: `0` to `999,999,999.99`. Negatives, over-max, or >2 decimals are invalid.
- The user-facing decimal separator is a **comma** (`25,1`); thousands are spaces.
- Output examples must match exactly, e.g. `0 -> zero dollars`, `1 -> one dollar`,
  `25,1 -> twenty-five dollars and ten cents`, `0,01 -> zero dollars and one cent`. The unit
  tests encode these; treat them as the spec.

## Key design decisions

### Conversion logic ([CurrencyToWordsConverter.cs](backend/src/CurrencyConverter.Domain/CurrencyToWordsConverter.cs))
- **One converter, no duplicated sentence logic.** The converter validates, splits into
  dollars/cents, and builds the sentence *once*. Only the per-language words come from an
  [`INumberSpeller`](backend/src/CurrencyConverter.Domain/Spelling/INumberSpeller.cs)
  (`EnglishNumberSpeller`, `GermanNumberSpeller`).
  [`NumberSpellerBase`](backend/src/CurrencyConverter.Domain/Spelling/NumberSpellerBase.cs)
  holds shared zero-handling and base-1000 group decomposition. **Add a language by adding a
  speller + registering it; do not branch in the converter.**
- **German word order** lives entirely in `GermanNumberSpeller`: units-before-tens
  (`fünfundzwanzig`), sub-million words glued together (`fünfundvierzigtausendeinhundert`),
  `Million`/`Millionen` inflection, and attributive `ein` (never standalone `eins`, since a
  currency noun always follows).
- Spellers are registered as `IEnumerable<INumberSpeller>` and keyed by `Language`
  (case-insensitive) in the converter. Unknown languages fall back to
  `SupportedLanguages.Default` (English).
- Validation failures throw
  [`CurrencyConversionException`](backend/src/CurrencyConverter.Domain/CurrencyConversionException.cs),
  which the controller maps to `400 ProblemDetails`.

### API ([CurrencyController.cs](backend/src/CurrencyConverter.Api/Controllers/CurrencyController.cs), [Program.cs](backend/src/CurrencyConverter.Api/Program.cs))
- `GET /api/currency/convert?amount={decimal}` returns
  `{ "conversionResult": "..." }` ([`ConversionResponse`](backend/src/CurrencyConverter.Api/Contracts/ConversionResponse.cs)).
  **Responses are wrapped in a DTO — do not return raw strings.**
- `amount` uses the **invariant** (`.`) decimal separator on the wire; the comma UX is the
  client's job.
- Language comes from the **`Accept-Language`** header; the response echoes it in
  **`Content-Language`** via `RequestLocalizationMiddleware`
  (`ApplyCurrentCultureToResponseHeaders = true`). CORS exposes `Content-Language`.
- **Auth is scaffolding only.** API-key scheme (`X-Api-Key`,
  [`ApiKeyAuthenticationHandler`](backend/src/CurrencyConverter.Api/Authentication/ApiKeyAuthenticationHandler.cs))
  with a demo `/api/secure/ping`. The convert endpoint is `[AllowAnonymous]` so the UI works
  without tokens. The key is a static config value (`ApiKey` in appsettings) — not real
  secret management.
- `Program` is `public partial` so the E2E project can use `WebApplicationFactory<Program>`.

### Frontend (`frontend/src/`)
- Plain **React 19 + Vite + TypeScript, no design library** (native toolkit only).
- All UI text via **i18next** ([i18n.ts](frontend/src/i18n.ts), `en`/`de`). Add strings to
  both language resources.
- [lib/amount.ts](frontend/src/lib/amount.ts) does live input formatting/validation:
  single comma, auto thousands spaces, max two decimals, non-negative, and **clamps** to
  `999 999 999,99` (the field can never hold an over-limit value). This mirrors server
  validation for UX — **the server is still the source of truth; keep both in sync.**
- API base URL is `VITE_API_BASE_URL` ([frontend/.env](frontend/.env), default
  `http://localhost:5282`). The browser calls the API **directly** (no dev proxy), in both
  dev and production — intentional per project requirement.
- Language switching re-converts the current amount in the **change handler**, not a
  `useEffect` (avoids the React 19 `set-state-in-effect` cascading-render lint error).

### CORS
- Allowed origins are config-driven (`Cors:AllowedOrigins` in
  [appsettings.json](backend/src/CurrencyConverter.Api/appsettings.json)): `5173` (Vite dev),
  `4173` (Vite preview), `8080` (Docker UI). If Vite picks a different port, add it there.

### Docker
- `docker compose up --build` → UI on host `8080`, API on host `5282`. The UI image is built
  with `VITE_API_BASE_URL=http://localhost:5282` (build arg) because the **browser** (on the
  host) calls the API, not the web container. API container listens on `8080`, published to
  host `5282`.

## Conventions

- **Test naming:** `Method_Scenario_ExpectedBehavior`
  (e.g. `Convert_EnglishPdfExamples_ReturnsExpectedResults`). The PDF examples and edge
  cases are data-driven `[Theory]` tests — add new cases there.
- C#: nullable enabled, file-scoped namespaces, `sealed` where practical.
- Keep domain logic free of ASP.NET dependencies (the `Domain` project must stay
  framework-agnostic and unit-testable).

## Build, test, run

```bash
# backend (from backend/)
cd backend && dotnet build && dotnet test

# frontend (from frontend/)
cd frontend && npm install && npm run dev      # lint: npm run lint, build: npm run build
```

When changing conversion behavior, run `dotnet test` — the unit tests are the executable
spec. When changing the API contract, also update the E2E test and the frontend client
([lib/currencyApi.ts](frontend/src/lib/currencyApi.ts)).
