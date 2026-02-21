import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TextInput, SafeAreaView, TouchableOpacity, Alert, ActivityIndicator, Platform } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Button } from '../components/Button';
import { useAuth } from '../context/AuthContext';
import { ChevronLeft } from 'lucide-react-native';
import { LanguageSelector } from '../components/LanguageSelector';

export const LoginScreen = ({ navigation }: any) => {
    const { t, i18n } = useTranslation();
    const { login } = useAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    // Detect deactivated parameter
    useEffect(() => {
        // Simple check for web platform URL
        if (Platform.OS === 'web' && typeof window !== 'undefined' && window.location.search.includes('deactivated=true')) {
            const msg = t('login.deactivatedMessage');
            window.alert(msg);
        }
    }, []);

    const handleLogin = async () => {
        if (!username || !password) {
            const msg = t('login.errors.allFieldsRequired');
            if (Platform.OS === 'web' && typeof window !== 'undefined' && window.alert) {
                window.alert(msg);
            } else {
                Alert.alert(t('common.error'), msg);
            }
            return;
        }

        setLoading(true);
        const result = await login(username, password);
        setLoading(false);

        if (result.success) {
            navigation.reset({
                index: 0,
                routes: [{ name: 'Home' }],
            });
        } else {
            // Use translation key for credentials error if backend generic message is returned in Spanish
            const msg = result.error && !result.error.includes(' ') ? t(`login.errors.${result.error}`) : t('login.errors.invalidCredentials');

            if (Platform.OS === 'web' && typeof window !== 'undefined' && window.alert) {
                window.alert(msg);
            } else {
                Alert.alert(t('common.error'), msg);
            }
        }
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backButton}>
                    <ChevronLeft color={COLORS.primary} size={28} />
                </TouchableOpacity>
                <LanguageSelector compact />
            </View>

            <View style={styles.content}>
                <Text style={[TYPOGRAPHY.h1, styles.title]}>{t('login.title')}</Text>

                <View style={styles.form}>
                    <View style={styles.inputGroup}>
                        <Text style={styles.label}>{t('login.username')}</Text>
                        <TextInput
                            style={styles.input}
                            value={username}
                            onChangeText={setUsername}
                            placeholder={t('login.username')}
                            autoCapitalize="none"
                        />
                    </View>

                    <View style={styles.inputGroup}>
                        <Text style={styles.label}>{t('login.password')}</Text>
                        <TextInput
                            style={styles.input}
                            value={password}
                            onChangeText={setPassword}
                            placeholder={t('login.password')}
                            secureTextEntry
                        />
                    </View>

                    <Button
                        title={loading ? t('common.loading') : t('login.loginButton')}
                        onPress={handleLogin}
                        style={styles.button}
                        disabled={loading}
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
    header: {
        padding: SPACING.md,
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
    backButton: {
        padding: 4,
    },
    content: {
        flex: 1,
        padding: SPACING.xl,
        justifyContent: 'center',
    },
    title: {
        textAlign: 'center',
        marginBottom: SPACING.xl * 2,
        color: COLORS.primary,
    },
    form: {
        gap: SPACING.lg,
    },
    inputGroup: {
        gap: SPACING.xs,
    },
    label: {
        fontSize: 14,
        fontWeight: '600',
        color: COLORS.textSecondary,
    },
    input: {
        backgroundColor: COLORS.surface,
        borderRadius: 12,
        padding: SPACING.md,
        fontSize: 16,
        borderWidth: 1,
        borderColor: '#E0E0E0',
    },
    button: {
        marginTop: SPACING.md,
    }
});
