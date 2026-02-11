import React, { useState } from 'react';
import { View, Text, StyleSheet, SafeAreaView, Image } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Button } from '../components/Button';
import { RegisterModal } from '../components/RegisterModal';
import { LanguageSelector } from '../components/LanguageSelector';


export const OnboardingScreen = ({ navigation }: any) => {
    const { t } = useTranslation();
    const [showRegisterModal, setShowRegisterModal] = useState(false);

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <View style={{ width: 20 }} />
                <LanguageSelector compact />
            </View>
            <View style={styles.content}>
                <View style={styles.logoContainer}>
                    {/* Logo placeholder */}
                    <View style={styles.logoCircle}>
                        <Text style={styles.logoText}>IA</Text>
                    </View>
                </View>

                <Text style={[TYPOGRAPHY.h1, styles.title]}>{t('onboarding.title')}</Text>
                <Text style={[TYPOGRAPHY.body, styles.subtitle]}>
                    {t('onboarding.subtitle')}
                </Text>

                <View style={styles.buttonContainer}>
                    <Button
                        title={t('onboarding.getStarted')}
                        onPress={() => setShowRegisterModal(true)}
                        style={styles.button}
                    />
                    <Button
                        title={t('onboarding.alreadyHaveAccount')}
                        variant="outline"
                        onPress={() => navigation.navigate('Login')}
                        style={styles.button}
                    />
                </View>
            </View>

            <RegisterModal
                visible={showRegisterModal}
                onClose={() => setShowRegisterModal(false)}
            />
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
    },
    header: {
        padding: SPACING.md,
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
    content: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: SPACING.xl,
        paddingBottom: SPACING.xl,
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
