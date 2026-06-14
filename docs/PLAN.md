# Plan: Currency-to-Words Web Application

## Context

We are building the **web** variant of the coding task: an app that converts a dollar
amount (max `999 999 999`, max `99` cents, comma decimal separator) into words, in
**English** and **German**. The conversion must run **server-side** (ASP.NET Core on
.NET 10), exposed via a REST API, with a **React 19** client. Language is selected by the
user and reflected in the output.

The repository is currently empty. Toolchain confirmed: .NET 10 SDK and Node 20 / npm 10.

This plan covers the full app, but execution proceeds in the agreed order:

1. **Build the API first, then pause for review.**
2. React 19 UI.
3. E2E HTTP test fixture with one happy-path case.

### Locked decisions (from clarification)
- **Auth:** API-key header (`X-Api-Key`) via a custom `AuthenticationHandler`.
- **Enforcement:** Auth is scaffolded; `/convert` stays **anonymous** so the UI works
  without tokens. A separate sample endpoint demonstrates the protected path.
- **German currency nouns:** `Dollar` / `Cent` (German numerals + German noun forms).

---

## Solution layout

```
/  (repo root)
  CurrencyConverter.sln
  README.md                         # build/run, design decisions, assumptions, limits
  docs/PROMPTS.md                   # AI prompt-history transparency requirement
  .gitignore                        # VS / .NET / Node
  backend/
    src/
      CurrencyConverter.Domain/     # class lib (net10.0): pure conversion logic
      CurrencyConverter.Api/        # ASP.NET Core (net10.0): controller, auth, localization
    tests/
      CurrencyConverter.UnitTests/  # xUnit: spellers, converter, validation
      CurrencyConverter.E2E/        # xUnit + WebApplicationFactory: HTTP happy-path
  frontend/                         # Vite + React 19 + TypeScript
```

Rationale: a thin **Domain** library keeps the conversion logic free of ASP.NET and
trivially unit-testable; the Api stays "one controller + one service" as requested.

---

## Step 1 — API (implement first, then pause)

### 1a. Domain library — conversion logic (DRY core)

The hard part is sharing structure between English and German while honoring German word
order. Design:

- `ICurrencyConverter` → `string Convert(decimal amount, string languageCode)`.
- `CurrencyConverter` (the single injected service): validates, splits the decimal into
  `dollars` (long) and `cents` (int), selects an `INumberSpeller` by language, and
  **assembles the sentence once** for all languages:
  - always: `"{SpellInteger(dollars)} {DollarNoun(dollars)}"`
  - if `cents > 0`: append `" {And} {SpellInteger(cents)} {CentNoun(cents)}"`
  - cents clause omitted when `cents == 0` (matches `0 → zero dollars`, `45 100 → … dollars`).
- `INumberSpeller` exposes per-language pieces so the assembly above never repeats:
  `string SpellInteger(long value)`, `string DollarNoun(long)`, `string CentNoun(int)`,
  `string And { get; }`, `string Language { get; }`.
- `NumberSpellerBase` (abstract) holds the genuinely shared bits: the `0 → "zero"/"null"`
  special case and the base-1000 group decomposition `(millions, thousands, units)`; it
  defers `SpellHundreds(int 1..999)` and group-joining to the concrete spellers (English
  joins groups with spaces + invariant scale words; German glues sub-thousand into one
  word and separates `Million(en)` by spaces).

**English speller rules:** ones[0..19], tens[twenty..ninety]; hyphen between tens+units
(`twenty-five`, `ninety-nine`); `X hundred` with a space (`one hundred`); scale words
`thousand` / `million` invariant, space-joined; no "and" inside the number (American
style — matches the PDF). Nouns: `dollar`/`dollars` (1 → singular, incl. `0 → dollars`),
`cent`/`cents`; connector `and`.

**German speller rules (the tricky bits):**
- ones use **attributive** form everywhere (always `ein`, never `eins`) since every number
  is followed by a noun → `ein Dollar`, `ein Cent`, `einundzwanzig`, `einhundert`.
- Special spellings: `sechzehn`, `siebzehn`, `dreißig` (ß), `sechzig`, `siebzig`.
- 21–99: units **before** tens with `und`: `fünfundvierzig`, `einundzwanzig`.
- hundreds glued: `einhundert`, `einhundertfünfundzwanzig`.
- thousands glued: `{1..999}tausend{remainder}` → `eintausend`,
  `fünfundvierzigtausendeinhundert`.
- millions separated by spaces with gender/plural: `1 → "eine Million"`,
  `>1 → "{spelled} Millionen"`.
- Nouns invariant: `Dollar`, `Cent` (capitalized); connector `und`. Number words stay
  lowercase; German nouns capitalized.

**Validation (`CurrencyConverter` + a guard type):** reject `amount < 0`,
`amount > 999999999.99`, and scale > 2 decimal places (`amount != Math.Round(amount, 2)`).
On failure throw a domain exception mapped to HTTP 400 in the Api.
Cents extraction: `cents = (int)Math.Round((amount - dollars) * 100)` (so `25.1 → 10`,
`0.01 → 1`).

### 1b. Api project

- `CurrencyController` with one action:
  `GET /api/currency/convert?amount=25.1` → `[FromQuery] decimal amount` (invariant `.`
  wire format; the React client converts the user's comma input to `.`).
  Returns the string body; maps domain validation failures to `400 ProblemDetails`.
  Marked `[AllowAnonymous]`.
- **Localization / headers:** configure `RequestLocalizationMiddleware` with supported
  cultures `en` (default) + `de`. The default `AcceptLanguageHeaderRequestCultureProvider`
  resolves the **Accept-Language** header; `ApplyCurrentCultureToResponseHeaders` writes
  **Content-Language** automatically. The converter picks the speller from
  `CultureInfo.CurrentUICulture`.
- **Auth (scaffolded):** custom `ApiKeyAuthenticationHandler` (scheme `ApiKey`) reading
  `X-Api-Key` against a configured key in `appsettings`; `AddAuthentication` +
  `AddAuthorization` registered. No global fallback policy (so anonymous works). A sample
  `GET /api/secure/ping` carries `[Authorize]` to prove the protected path works.
- **CORS:** allow the Vite dev origin (also a dev proxy in the frontend) so the browser
  client can call the API.
- DI: register `ICurrencyConverter` and both spellers (resolved by language).

### 1c. Unit tests (xUnit)

`[Theory]/[InlineData]` driven by the PDF table for both languages, e.g.:

| amount | en | de |
|---|---|---|
| 0 | zero dollars | null Dollar |
| 1 | one dollar | ein Dollar |
| 25.1 | twenty-five dollars and ten cents | fünfundzwanzig Dollar und zehn Cent |
| 0.01 | zero dollars and one cent | null Dollar und ein Cent |
| 45100 | forty-five thousand one hundred dollars | fünfundvierzigtausendeinhundert Dollar |
| 999999999.99 | nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-nine dollars and ninety-nine cents | neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig Dollar und neunundneunzig Cent |

Plus edge cases: boundary `999999999.99`, hyphen/`und` ordering, validation rejects
(negative, over-max, 3 decimals).

**→ PAUSE for review after Step 1.**

---

## Step 2 — React 19 client

- Scaffold with Vite (`react-ts`), React 19, TypeScript. No external design libraries —
  plain CSS only.
- Layout: centered **input** field with **Convert** button to its right, **output** field
  below, **language switch (EN / DE)** top-right.
- **i18next** (`i18next` + `react-i18next`) for all UI text (title, placeholder, Convert
  button, output label, validation messages, switch labels); `en` + `de` resources.
- **Input validation (client, mirrored on server):** digits only, spaces as thousands
  separators (auto-inserted on input), at most **one** comma as decimal separator, max 2
  decimals, non-negative (no minus), max `999 999 999,99`. Inline translated error.
- On Convert: normalize (strip spaces, comma→`.`) → `GET /api/currency/convert?amount=…`
  with `Accept-Language: <selected>`; show the returned words. Changing the language
  switch updates UI strings and re-converts the current valid value.
- Dev: Vite proxy `/api` → backend to avoid CORS friction.

---

## Step 3 — E2E fixture

- `CurrencyConverter.E2E` using `WebApplicationFactory<Program>` (real HTTP pipeline,
  in-memory `HttpClient` → genuine REST call).
- One happy-path test: `GET /api/currency/convert?amount=25.1` with
  `Accept-Language: en` → asserts `200`, body `twenty-five dollars and ten cents`, and
  `Content-Language: en`.

---

## Assumptions & limitations

- API wire format for `amount` uses `.` decimal separator (invariant); the comma UX lives
  in the client. Documented in README.
- English uses American style (no "and" inside the number), matching the PDF.
- API key is a static configured value (demo-grade), suitable as a "wire it up for later"
  placeholder, not production secret management.
- Max value `999 999 999,99`; values outside the range / >2 decimals are rejected 400.

---

## Verification

- `dotnet test` (backend) — unit + E2E green; the PDF examples are the unit-test oracle.
- `dotnet run` the Api; manual check:
  `curl "http://localhost:PORT/api/currency/convert?amount=25.1" -H "Accept-Language: de"`
  → `fünfundzwanzig Dollar und zehn Cent`, response header `Content-Language: de`.
  `curl … -H "Accept-Language: en"` → English; verify `Content-Language: en`.
  Hit `/api/secure/ping` without/with `X-Api-Key` → 401 then 200.
- `npm run dev` (frontend): type `25,1`, Convert → output; toggle EN/DE re-converts;
  invalid input shows translated error.
```
