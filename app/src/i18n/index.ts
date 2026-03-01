import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import AsyncStorage from '@react-native-async-storage/async-storage';

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

const languageDetectorPlugin: any = {
    type: 'languageDetector',
    async: true,
    detect: async (callback: (lang: string) => void) => {
        try {
            const language = await AsyncStorage.getItem('i18nextLng');
            if (language) {
                callback(language);
            } else {
                callback('es');
            }
        } catch (error) {
            console.log('Error reading language', error);
            callback('es');
        }
    },
    init: () => { },
    cacheUserLanguage: async (language: string) => {
        try {
            await AsyncStorage.setItem('i18nextLng', language);
        } catch (error) {
            console.log('Error saving language', error);
        }
    }
};

i18n
    .use(languageDetectorPlugin) // custom async storage detector
    .use(initReactI18next) // Pass i18n instance to react-i18next
    .init({
        resources,
        fallbackLng: 'es', // Default language

        interpolation: {
            escapeValue: false, // React already escapes values
        },

        react: {
            useSuspense: false, // Disable suspense for React Native compatibility
        },
    });

export default i18n;
