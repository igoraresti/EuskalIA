import React from 'react';
import { TouchableOpacity, Text, StyleSheet, ViewStyle, TextStyle } from 'react-native';
import { COLORS, SPACING } from '../theme';

interface ButtonProps {
    title: string;
    onPress: () => void;
    variant?: 'primary' | 'secondary' | 'outline';
    style?: ViewStyle;
    textStyle?: TextStyle;
}

export const Button: React.FC<ButtonProps> = ({
    title,
    onPress,
    variant = 'primary',
    style,
    textStyle
}) => {
    const getButtonStyle = () => {
        switch (variant) {
            case 'secondary': return styles.secondary;
            case 'outline': return styles.outline;
            default: return styles.primary;
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
            style={[styles.base, getButtonStyle(), style]}
            onPress={onPress}
            activeOpacity={0.8}
        >
            <Text style={[styles.baseText, getTextStyle(), textStyle]}>{title}</Text>
        </TouchableOpacity>
    );
};

const styles = StyleSheet.create({
    base: {
        paddingVertical: SPACING.md,
        paddingHorizontal: SPACING.xl,
        borderRadius: 16,
        alignItems: 'center',
        justifyContent: 'center',
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 4 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
        elevation: 4,
    },
    baseText: {
        fontSize: 18,
        fontWeight: 'bold',
    },
    primary: {
        backgroundColor: COLORS.primary,
    },
    primaryText: {
        color: COLORS.white,
    },
    secondary: {
        backgroundColor: COLORS.secondary,
    },
    outline: {
        backgroundColor: COLORS.white,
        borderWidth: 2,
        borderColor: COLORS.primary,
    },
    outlineText: {
        color: COLORS.primary,
    },
});
