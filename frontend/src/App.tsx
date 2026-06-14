import { useCallback, useState, type ChangeEvent, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { LanguageSwitch } from './components/LanguageSwitch';
import { convertCurrency } from './lib/currencyApi';
import { sanitizeAndFormat, toWireFormat, validateAmount } from './lib/amount';
import './App.css';

function App() {
  const { t, i18n } = useTranslation();
  const language = i18n.resolvedLanguage ?? 'en';

  const [amount, setAmount] = useState('');
  const [result, setResult] = useState('');
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  // The wire amount behind the current result; non-null means a conversion is shown.
  const [convertedWire, setConvertedWire] = useState<string | null>(null);

  const runConversion = useCallback(
    async (lang: string) => {
      const validationError = validateAmount(amount);
      if (validationError) {
        setErrorKey(validationError);
        setResult('');
        setConvertedWire(null);
        return;
      }

      setIsLoading(true);
      setErrorKey(null);
      try {
        const words = await convertCurrency(toWireFormat(amount), lang);
        setResult(words);
        setConvertedWire(toWireFormat(amount));
      } catch {
        setErrorKey('errors.requestFailed');
        setResult('');
        setConvertedWire(null);
      } finally {
        setIsLoading(false);
      }
    },
    [amount],
  );

  // Switch language and, if a conversion is already shown, refresh it in the new language.
  const handleLanguageChange = useCallback(
    (lng: string) => {
      void i18n.changeLanguage(lng);
      if (convertedWire !== null) {
        void runConversion(lng);
      }
    },
    [i18n, convertedWire, runConversion],
  );

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    setAmount(sanitizeAndFormat(event.target.value));
    setErrorKey(null);
  };

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    void runConversion(language);
  };

  const isInvalid = errorKey !== null && errorKey !== 'errors.requestFailed';

  return (
    <div className="page">
      <header className="topbar">
        <LanguageSwitch current={language} onChange={handleLanguageChange} />
      </header>

      <main className="content">
        <h1 className="title">{t('title')}</h1>
        <p className="subtitle">{t('subtitle')}</p>

        <form className="converter" onSubmit={handleSubmit} noValidate>
          <div className="input-row">
            <input
              className={isInvalid ? 'amount-input invalid' : 'amount-input'}
              type="text"
              inputMode="decimal"
              autoComplete="off"
              value={amount}
              onChange={handleChange}
              placeholder={t('inputPlaceholder')}
              aria-label={t('inputLabel')}
              aria-invalid={isInvalid}
            />
            <button type="submit" className="convert-button" disabled={isLoading}>
              {isLoading ? t('converting') : t('convert')}
            </button>
          </div>
          {errorKey && (
            <p className="error" role="alert">
              {t(errorKey)}
            </p>
          )}
        </form>

        <section className="output" aria-live="polite">
          <h2 className="output-label">{t('resultLabel')}</h2>
          <p className={result ? 'output-value' : 'output-value empty'}>
            {result || t('resultEmpty')}
          </p>
        </section>
      </main>
    </div>
  );
}

export default App;
