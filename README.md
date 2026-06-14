# Currency-to-Words

Converts a dollar amount (max `999,999,999.99`) into written-out words in **English** and
**German**. The conversion runs entirely server-side (ASP.NET Core on .NET 10) and is
exposed through a small REST API; a React 19 + Vite client consumes it.

```
0              -> zero dollars
1              -> one dollar
25,1           -> twenty-five dollars and ten cents
0,01           -> zero dollars and one cent
45 100         -> forty-five thousand one hundred dollars
999 999 999,99 -> nine hundred ninety-nine million nine hundred ninety-nine thousand
                  nine hundred ninety-nine dollars and ninety-nine cents
```

## Project structure

```
backend/
  CurrencyConverter.slnx        # solution file
  src/
    CurrencyConverter.Domain/   # pure conversion logic (no ASP.NET dependency)
    CurrencyConverter.Api/      # ASP.NET Core: controller, localization, API-key auth
  tests/
    CurrencyConverter.UnitTests/# xUnit: spellers, converter, validation
    CurrencyConverter.E2E/      # xUnit + WebApplicationFactory: HTTP happy-path
frontend/                       # React 19 + Vite + TypeScript client
  src/lib/        # API client + amount validation/formatting
  src/components/ # LanguageSwitch
  src/i18n.ts     # i18next setup with en/de resources
docs/PROMPTS.md                 # AI prompt history (transparency requirement)
```

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for the frontend)

## Run locally (API + UI)

The app is client–server: start the **API** first, then the **UI** in a second terminal.
The UI targets the local API at `http://localhost:5282` in both development and production
builds (configurable via `frontend/.env`).

```bash
# Terminal 1 — API (listens on http://localhost:5282)
cd backend
dotnet run --project src/CurrencyConverter.Api

# Terminal 2 — UI (dev server on http://localhost:5173)
cd frontend
npm install        # first time only
npm run dev        # open the printed URL, e.g. http://localhost:5173
```

To run the UI as a production build instead of the dev server:

```bash
cd frontend
npm run build      # outputs to frontend/dist
npm run preview    # serves the build on http://localhost:4173
```

Both `http://localhost:5173` (dev) and `http://localhost:4173` (preview) are allowed by the
API's CORS policy. If Vite picks a different port (because one is already in use), add that
origin to `Cors:AllowedOrigins` in `backend/src/CurrencyConverter.Api/appsettings.json`.

## Build & test the backend

```bash
cd backend
dotnet build       # build the whole solution
dotnet test        # run unit + e2e tests
```

## API

### `GET /api/currency/convert?amount={decimal}`

Converts an amount to words.

- `amount` uses an **invariant** (`.`) decimal separator on the wire, max two decimals
  (e.g. `?amount=25.1`). The client is responsible for the comma UX shown to users.
- The language is taken from the **`Accept-Language`** request header (`en` or `de`,
  defaulting to `en`). The response echoes the language used in the **`Content-Language`**
  header.
- Returns `200` with a JSON body `{ "conversionResult": "..." }`, or
  `400 application/problem+json` for invalid amounts (negative, above the maximum, or more
  than two decimals).
- Anonymous — no credentials required.

```bash
curl "http://localhost:5282/api/currency/convert?amount=25.1" -H "Accept-Language: de"
# => {"conversionResult":"fünfundzwanzig Dollar und zehn Cent"}   (Content-Language: de)
```

### `GET /api/secure/ping`

Sample **protected** endpoint demonstrating the API-key scheme. Send the key in the
`X-Api-Key` header; returns `401` without a valid key.

```bash
curl "http://localhost:5282/api/secure/ping" -H "X-Api-Key: dev-secret-key-change-me"
```

## Key design decisions

- **Shared assembly, language-specific words.** The single `CurrencyToWordsConverter`
  validates the amount, splits it into dollars/cents and builds the sentence *once*. Only
  the per-language words come from an `INumberSpeller` (`EnglishNumberSpeller`,
  `GermanNumberSpeller`), so the high-level logic is never duplicated. A `NumberSpellerBase`
  holds the shared zero-handling and base-1000 group decomposition.
- **German word order** is handled in `GermanNumberSpeller`: units before tens
  (`fünfundzwanzig`), sub-million numbers glued into one word
  (`fünfundvierzigtausendeinhundert`), and gender/number-inflected `Million`/`Millionen`.
  The attributive `ein` is always used (never standalone `eins`) because every number is
  followed by a currency noun.
- **Localization** uses the built-in `RequestLocalizationMiddleware`, which resolves the
  language from `Accept-Language` and writes the `Content-Language` response header.
- **Auth** is intentionally simple API-key scaffolding (`X-Api-Key`) so endpoints can be
  protected later; the `convert` endpoint stays anonymous so the UI works without tokens.
- **Frontend** is plain React 19 + Vite + TypeScript with no design library. All text is
  translated via **i18next** (`en`/`de`). The amount input formats live (thousands spaces,
  a single comma, max two decimals, non-negative) and is validated client-side; the server
  re-validates everything. Switching language re-converts the current amount.

## Assumptions & limitations

- The API wire format for `amount` uses `.` as the decimal separator; the comma-based UX
  from the task lives in the client.
- English uses American style (no "and" inside the number), matching the task examples.
- German localizes the numerals and the noun forms but keeps the **Dollar/Cent** currency.
- The API key is a static configured value (`appsettings.json`) — demo-grade scaffolding,
  not production secret management.
- Supported range is `0` to `999,999,999.99`; out-of-range or >2-decimal values return 400.

## AI usage

This solution was built with AI assistance. See [docs/PROMPTS.md](docs/PROMPTS.md) for the
prompt history (transparency requirement).
