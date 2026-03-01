export const COLORS = {
    primary: '#58CC02', // Duolingo Green
    primaryDark: '#46A302',
    secondary: '#1CB0F6', // Duolingo Blue
    secondaryDark: '#1899D6',
    background: '#FFFFFF',
    surface: '#F7F7F7',
    text: '#3C3C3C',
    textSecondary: '#777777',
    accent: '#FFC800', // Duolingo Yellow
    accentDark: '#E5A400',
    white: '#FFFFFF',
    error: '#EA2B2B',
    success: '#58CC02',
    border: '#E5E5E5',
};

export const SPACING = {
    xs: 4,
    sm: 8,
    md: 16,
    lg: 24,
    xl: 32,
};

export const TYPOGRAPHY = {
    h1: {
        fontSize: 32,
        fontWeight: '800',
        color: '#58CC02',
    },
    h2: {
        fontSize: 24,
        fontWeight: '800',
        color: '#58CC02',
    },
    body: {
        fontSize: 18,
        color: '#3C3C3C',
        fontWeight: '600',
    },
    caption: {
        fontSize: 14,
        color: '#777777',
    },
} as const;
