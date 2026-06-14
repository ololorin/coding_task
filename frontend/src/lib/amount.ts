/**
 * Helpers for the amount input: live formatting (thousands spaces, single comma,
 * max two decimals, non-negative) and validation. The server re-validates everything,
 * so this is purely for UX.
 */

export const MAX_AMOUNT = 999_999_999.99;

/** The maximum amount in the input's formatted representation. */
export const MAX_AMOUNT_FORMATTED = '999 999 999,99';

export type AmountErrorKey = 'errors.empty' | 'errors.invalid' | 'errors.exceedsMax';

/**
 * Sanitizes raw user input and reformats it: keeps digits and a single comma, drops
 * everything else (including any minus sign), groups the integer part with spaces and
 * caps the decimal part at two digits. Values above the maximum are clamped to the cap so
 * the field can never hold an over-limit amount.
 */
export function sanitizeAndFormat(raw: string): string {
  const cleaned = raw.replace(/[^\d,]/g, '');
  const firstComma = cleaned.indexOf(',');

  let formatted: string;
  if (firstComma === -1) {
    formatted = groupThousands(cleaned);
  } else {
    const intDigits = cleaned.slice(0, firstComma).replace(/,/g, '');
    const decDigits = cleaned.slice(firstComma + 1).replace(/,/g, '').slice(0, 2);
    const intPart = intDigits === '' ? '0' : groupThousands(intDigits);
    formatted = `${intPart},${decDigits}`;
  }

  const value = parseAmount(formatted);
  return value !== null && value > MAX_AMOUNT ? MAX_AMOUNT_FORMATTED : formatted;
}

function groupThousands(digits: string): string {
  const normalized = digits.replace(/^0+(?=\d)/, '');
  return normalized.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
}

/** Converts a formatted amount ("25 100,99") to the invariant wire format ("25100.99"). */
export function toWireFormat(formatted: string): string {
  return formatted.replace(/\s/g, '').replace(',', '.');
}

/** Parses a formatted amount to a number, or null if it is not a finite value. */
export function parseAmount(formatted: string): number | null {
  const normalized = toWireFormat(formatted);
  if (normalized === '' || normalized === '.') return null;
  const value = Number(normalized);
  return Number.isFinite(value) ? value : null;
}

/** Returns a translation key for the first validation problem, or null if valid. */
export function validateAmount(formatted: string): AmountErrorKey | null {
  if (formatted.trim() === '') return 'errors.empty';

  const value = parseAmount(formatted);
  if (value === null) return 'errors.invalid';
  if (value > MAX_AMOUNT) return 'errors.exceedsMax';

  return null;
}
