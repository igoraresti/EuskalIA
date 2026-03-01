import React from 'react';
import { TouchableOpacity, Text, StyleSheet, ViewStyle, TextStyle } from 'react-native';
import { COLORS, SPACING } from '../theme';

interface ButtonProps {
    title: string;
    onPress: () => void;
    variant?: 'primary' | 'secondary' | 'outline';
    style?: ViewStyle;
    textStyle?: TextStyle;
    disabled?: boolean;
}

export const Button: React.FC<ButtonProps> = ({
    title,
    onPress,
    variant = 'primary',
    style,
    textStyle,
    disabled
}) => {
    const getButtonStyle = () => {
        switch (variant) {
            case 'secondary': return [styles.secondary, { borderBottomColor: COLORS.secondaryDark }];
            case 'outline': return styles.outline;
            default: return [styles.primary, { borderBottomColor: COLORS.primaryDark }];
        }
    };

    const getTextStyle = () => {
        switch (variant) {
            case 'outline': return styles.outlineText;
            default: return styles.primaryText;
        }
    };

    return (
        <TouchableOpacity
            style={[styles.base, getButtonStyle(), style, disabled && styles.disabled]}
            onPress={onPress}
            activeOpacity={0.8}
            disabled={disabled}
        >
            <Text style={[styles.baseText, getTextStyle(), textStyle, disabled && styles.disabledText]}>{title}</Text>
        </TouchableOpacity>
    );
};

const styles = StyleSheet.create({
    base: {
        paddingVertical: 14,
        paddingHorizontal: SPACING.xl,
        borderRadius: 16,
        alignItems: 'center',
        justifyContent: 'center',
        borderBottomWidth: 4, // The 3D effect
    },
    baseText: {
        fontSize: 18,
        fontWeight: '800', // Extra bold
        textTransform: 'uppercase', // Playful and bold
        letterSpacing: 0.8,
    },
    primary: {
        backgroundColor: COLORS.primary,
        borderColor: COLORS.primary,
    },
    primaryText: {
        color: COLORS.white,
    },
    secondary: {
        backgroundColor: COLORS.secondary,
        borderColor: COLORS.secondary,
    },
    outline: {
        backgroundColor: COLORS.white,
        borderWidth: 2,
        borderColor: COLORS.border,
        borderBottomWidth: 4,
        borderBottomColor: COLORS.border,
    },
    outlineText: {
        color: COLORS.secondary,
    },
    disabled: {
        opacity: 0.5,
        backgroundColor: COLORS.border,
        borderBottomColor: COLORS.border,
    },
    disabledText: {
        color: '#AFAFAF',
    }
});
