import React from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { CheckCircle } from 'lucide-react-native';
import { Button } from '../components/Button';

export const RegistrationSuccessScreen = ({ navigation }: any) => {
    const handleGoToLogin = () => {
        navigation.reset({
            index: 0,
            routes: [{ name: 'Login' }],
        });
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.content}>
                <View style={styles.iconContainer}>
                    <CheckCircle color={COLORS.primary} size={80} strokeWidth={2} />
                </View>

                <Text style={[TYPOGRAPHY.h1, styles.title]}>¡Cuenta Verificada!</Text>

                <Text style={[TYPOGRAPHY.body, styles.message]}>
                    Tu cuenta ha sido verificada exitosamente. Ya puedes iniciar sesión y comenzar a aprender euskera.
                </Text>

                <Button
                    title="Ir a Iniciar Sesión"
                    onPress={handleGoToLogin}
                    style={styles.button}
                />
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
    iconContainer: {
        marginBottom: SPACING.xl,
    },
    title: {
        textAlign: 'center',
        marginBottom: SPACING.lg,
        color: COLORS.primary,
    },
    message: {
        textAlign: 'center',
        marginBottom: SPACING.xl * 2,
        lineHeight: 24,
        color: COLORS.textSecondary,
    },
    button: {
        width: '100%',
        maxWidth: 300,
    },
});
