export const COLORS = {
    primary: '#009B3A', // Basque Green
    secondary: '#DA121A', // Basque Red
    background: '#FFFFFF',
    surface: '#F7F7F7',
    text: '#333333',
    textSecondary: '#666666',
    accent: '#F9A825', // Golden for XP/Coins
    white: '#FFFFFF',
    error: '#FF5252',
    success: '#4CAF50',
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
        fontWeight: '700',
        color: COLORS.text,
    },
    h2: {
        fontSize: 24,
        fontWeight: '700',
        color: COLORS.text,
    },
    body: {
        fontSize: 16,
        color: COLORS.text,
    },
    caption: {
        fontSize: 14,
        color: COLORS.textSecondary,
    },
} as const;
