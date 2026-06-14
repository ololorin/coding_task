import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

export const SUPPORTED_LANGUAGES = ['en', 'de'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

const resources = {
  en: {
    translation: {
      title: 'Currency to Words',
      subtitle: 'Convert a dollar amount into words.',
      inputPlaceholder: 'e.g. 25,1',
      inputLabel: 'Amount in dollars',
      convert: 'Convert',
      converting: 'Converting…',
      resultLabel: 'Result',
      resultEmpty: 'The converted amount will appear here.',
      languageLabel: 'Language',
      errors: {
        empty: 'Please enter an amount.',
        invalid: 'Please enter a valid amount.',
        exceedsMax: 'The amount must not exceed 999 999 999,99.',
        requestFailed: 'Could not convert the amount. Is the API running?',
      },
    },
  },
  de: {
    translation: {
      title: 'Währung in Worte',
      subtitle: 'Wandle einen Dollarbetrag in Worte um.',
      inputPlaceholder: 'z. B. 25,1',
      inputLabel: 'Betrag in Dollar',
      convert: 'Umwandeln',
      converting: 'Wird umgewandelt…',
      resultLabel: 'Ergebnis',
      resultEmpty: 'Der umgewandelte Betrag erscheint hier.',
      languageLabel: 'Sprache',
      errors: {
        empty: 'Bitte einen Betrag eingeben.',
        invalid: 'Bitte einen gültigen Betrag eingeben.',
        exceedsMax: 'Der Betrag darf 999 999 999,99 nicht überschreiten.',
        requestFailed: 'Betrag konnte nicht umgewandelt werden. Läuft die API?',
      },
    },
  },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    supportedLngs: SUPPORTED_LANGUAGES,
    interpolation: { escapeValue: false },
    react: { useSuspense: false },
  });

export default i18n;
