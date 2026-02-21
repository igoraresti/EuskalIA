import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import translation files
import es from './locales/es.json';
import en from './locales/en.json';
import pl from './locales/pl.json';
import eu from './locales/eu.json';
import fr from './locales/fr.json';

const resources = {
    es: { translation: es },
    en: { translation: en },
    pl: { translation: pl },
    eu: { translation: eu },
    fr: { translation: fr },
};

i18n
    .use(LanguageDetector) // Detect user language
    .use(initReactI18next) // Pass i18n instance to react-i18next
    .init({
        resources,
        fallbackLng: 'es', // Default language
        lng: 'es', // Initial language (Spanish for login/register)

        detection: {
            // Order of language detection
            order: ['querystring', 'localStorage', 'navigator'],
            caches: ['localStorage'], // Cache user language preference
            lookupQuerystring: 'lng',
            lookupLocalStorage: 'i18nextLng',
        },

        interpolation: {
            escapeValue: false, // React already escapes values
        },

        react: {
            useSuspense: false, // Disable suspense for React Native compatibility
        },
    });

export default i18n;
