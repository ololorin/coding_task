const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5282';

interface ConversionResponse {
  conversionResult: string;
}

/**
 * Calls the conversion API. The language is sent via the Accept-Language header so the
 * server picks the matching language (and echoes it in Content-Language).
 *
 * @param wireAmount The amount in invariant format ("25100.99").
 * @param language A supported two-letter language code.
 */
export async function convertCurrency(wireAmount: string, language: string): Promise<string> {
  const url = `${API_BASE_URL}/api/currency/convert?amount=${encodeURIComponent(wireAmount)}`;

  const response = await fetch(url, {
    headers: { 'Accept-Language': language },
  });

  if (!response.ok) {
    throw new Error(`Conversion request failed with status ${response.status}`);
  }

  const data: ConversionResponse = await response.json();
  return data.conversionResult;
}
