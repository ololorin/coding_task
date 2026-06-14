# AI Prompt History

This project was developed with the assistance of an AI coding agent (Claude Code). This
file documents the prompts used, for transparency.

## 1. Initial brief (planning + API implementation)

> Read the attached PDF file with a description of the coding task. Create a plan for
> building such an app. We will be implementing the Web version.
>
> First step will be building an API, use .NET 10. The app is simple enough so that one
> controller and one service will suffice. The chosen languages will be English and German
> for the conversion service (consider the different order of words in the German
> numerals). The "currency" controller should contain a single GET method, called
> "convert", taking a decimal number as input and returning a string representation of the
> number, as described in the PDF, considering the Accept-Language header to determine the
> language. The result should also correctly set Content-Language. Try to minimize
> repeating logic in the service and cover it with unit-tests. Add simple authentication
> and authorization in case we need it later.
>
> Second step will be creating a simple React 19 UI. There should be a single input field
> in the center of the page, a "Convert" button to the right, an output field under them,
> and the language switch in the top right with EN and DE options. Use i18next for
> translating the contents of the Convert button and all other possible instances of text.
> Enforce the validation of the input field (one or zero commas as decimal separator,
> spaces as thousands separators (may be added automatically), non-negative numbers,
> maximum amount specified in PDF, recheck all this also on server side). Do not use
> external design libraries at this point, stick to the native React toolkit.
>
> In the end scaffold an e2e test fixture which should test the application by invoking
> REST endpoints through HTTP calls: add a single happy-path case test.
>
> After we agree on the plan, start with implementing API, then pause for review.

### Clarifying questions answered during planning

- **Authentication scheme:** API-key header (`X-Api-Key`) via a custom
  `AuthenticationHandler`.
- **Enforcement:** Auth is scaffolded only; the `/convert` endpoint stays anonymous so the
  UI works without tokens. A separate `/api/secure/ping` endpoint demonstrates the
  protected path.
- **German currency wording:** `Dollar` / `Cent` (German numerals + German noun forms,
  keeping the dollar currency).

## 2. API fixes
> Good start. A few changes before we proceed further:
> 1. Currently API returns a raw string. This is undesirable. Wrap the string into a simple DTO object, eg:
> {
> "conversionResult": "..."
> }
> Change e2e test accordingly.
>
> 2. Change tests naming to be closer to standard Microsoft Test Naming convention: 
> Name of the method being tested
> Scenario under which the method is being tested
> Expected behavior when the scenario is invoked
>
> Eg. Converts_english_pdf_examples -> Convert_EnglishPdfExamples_ReturnsExpectedResults
>
> 3. Move the .slnx file from the repository root to the backend folder, change inner folder structure accordingly.
