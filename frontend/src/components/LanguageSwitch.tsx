import { useTranslation } from 'react-i18next';
import { SUPPORTED_LANGUAGES } from '../i18n';

interface LanguageSwitchProps {
  current: string;
  onChange: (language: string) => void;
}

/**
 * Top-right language switch. Selecting a language is handled by the parent, which also
 * refreshes the current conversion in the chosen language.
 */
export function LanguageSwitch({ current, onChange }: LanguageSwitchProps) {
  const { t } = useTranslation();

  return (
    <div className="language-switch" role="group" aria-label={t('languageLabel')}>
      {SUPPORTED_LANGUAGES.map((lng) => (
        <button
          key={lng}
          type="button"
          className={lng === current ? 'lang-button active' : 'lang-button'}
          aria-pressed={lng === current}
          onClick={() => onChange(lng)}
        >
          {lng.toUpperCase()}
        </button>
      ))}
    </div>
  );
}
