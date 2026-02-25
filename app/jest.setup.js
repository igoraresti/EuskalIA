import mockAsyncStorage from '@react-native-async-storage/async-storage/jest/async-storage-mock';

jest.mock('@react-native-async-storage/async-storage', () => mockAsyncStorage);

// Mock i18next
jest.mock('i18next', () => ({
    use: jest.fn().mockReturnThis(),
    init: jest.fn().mockResolvedValue({}),
    t: (key) => key,
    changeLanguage: jest.fn().mockResolvedValue({}),
    language: 'es',
}));

jest.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key) => key,
        i18n: {
            changeLanguage: jest.fn().mockResolvedValue({}),
            language: 'es',
        },
    }),
    initReactI18next: {
        type: '3rdParty',
        init: jest.fn(),
    },
}));

// Mock the internal i18n instance completely to avoid loading real config
jest.mock('./src/i18n', () => ({
    __esModule: true,
    default: {
        t: (key) => key,
        changeLanguage: jest.fn().mockResolvedValue({}),
        use: jest.fn().mockReturnThis(),
        init: jest.fn().mockResolvedValue({}),
        language: 'es',
    },
}));

// Mock i18next-browser-languagedetector
jest.mock('i18next-browser-languagedetector', () => ({
    __esModule: true,
    default: class {
        constructor() { this.type = 'languageDetector'; }
        init() { }
        detect() { return 'es'; }
        cacheUserLanguage() { }
    }
}));

// Mock lucide-react-native
jest.mock('lucide-react-native', () => {
    const React = require('react');
    const { View } = require('react-native');
    const mockIcon = (name) => (props) => React.createElement(View, { ...props, testID: `icon-${name}` });
    return {
        User: mockIcon('User'),
        Mail: mockIcon('Mail'),
        Lock: mockIcon('Lock'),
        Calendar: mockIcon('Calendar'),
        BookOpen: mockIcon('BookOpen'),
        Trash2: mockIcon('Trash2'),
        LogOut: mockIcon('LogOut'),
        Save: mockIcon('Save'),
        ChevronLeft: mockIcon('ChevronLeft'),
        ChevronRight: mockIcon('ChevronRight'),
        Globe: mockIcon('Globe'),
        Users: mockIcon('Users'),
        UserCheck: mockIcon('UserCheck'),
        UserX: mockIcon('UserX'),
        Search: mockIcon('Search'),
        Filter: mockIcon('Filter'),
        Shield: mockIcon('Shield'),
        RefreshCw: mockIcon('RefreshCw'),
    };
});

// Mock LoginButtons explicitly so it never tries to load its tricky expo-auth native dependencies globally during test renders
jest.mock('./src/components/LoginButtons', () => {
    const React = require('react');
    const { View } = require('react-native');
    return {
        LoginButtons: () => React.createElement(View, { testID: 'mocked-login-buttons' })
    };
});

// Avoid console error noise in tests
console.error = jest.fn();
console.warn = jest.fn();
console.log = jest.fn();
