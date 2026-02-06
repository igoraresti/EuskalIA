import React from 'react';
import { View, Text, StyleSheet, SafeAreaView, Image } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Button } from '../components/Button';

export const OnboardingScreen = ({ navigation }: any) => {
    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.content}>
                <View style={styles.logoContainer}>
                    {/* Logo placeholder */}
                    <View style={styles.logoCircle}>
                        <Text style={styles.logoText}>IA</Text>
                    </View>
                </View>

                <Text style={[TYPOGRAPHY.h1, styles.title]}>Euskal IA</Text>
                <Text style={[TYPOGRAPHY.body, styles.subtitle]}>
                    Aprende euskera de forma inteligente y divertida. El poder de la IA en tus manos.
                </Text>

                <View style={styles.buttonContainer}>
                    <Button
                        title="Empezar ahora"
                        onPress={() => navigation.navigate('Home')}
                        style={styles.button}
                    />
                    <Button
                        title="Ya tengo cuenta"
                        variant="outline"
                        onPress={() => navigation.navigate('Login')}
                        style={styles.button}
                    />
                </View>
            </View>
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
    },
    content: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        padding: SPACING.xl,
    },
    logoContainer: {
        marginBottom: SPACING.xl,
    },
    logoCircle: {
        width: 120,
        height: 120,
        borderRadius: 60,
        backgroundColor: COLORS.primary,
        alignItems: 'center',
        justifyContent: 'center',
    },
    logoText: {
        color: COLORS.white,
        fontSize: 40,
        fontWeight: 'bold',
    },
    title: {
        textAlign: 'center',
        marginBottom: SPACING.md,
        color: COLORS.primary,
    },
    subtitle: {
        textAlign: 'center',
        marginBottom: SPACING.xl * 2,
        lineHeight: 24,
    },
    buttonContainer: {
        width: '100%',
        gap: SPACING.md,
    },
    button: {
        width: '100%',
    }
});
