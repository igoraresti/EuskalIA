import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, SafeAreaView, TouchableOpacity, Alert, ActivityIndicator } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Button } from '../components/Button';
import { useAuth } from '../context/AuthContext';
import { ChevronLeft } from 'lucide-react-native';

export const LoginScreen = ({ navigation }: any) => {
    const { login } = useAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    const handleLogin = async () => {
        if (!username || !password) {
            const msg = 'Por favor introduce usuario y contraseña';
            if (typeof window !== 'undefined' && window.alert) {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
            return;
        }

        setLoading(true);
        console.log('Attempting login with:', username);
        const result = await login(username, password);
        console.log('Login result:', result);
        setLoading(false);

        if (result.success) {
            navigation.reset({
                index: 0,
                routes: [{ name: 'Home' }],
            });
        } else {
            const msg = result.error || 'Usuario o contraseña incorrectos';
            if (typeof window !== 'undefined' && window.alert) {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
        }
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backButton}>
                    <ChevronLeft color={COLORS.primary} size={28} />
                </TouchableOpacity>
            </View>

            <View style={styles.content}>
                <Text style={[TYPOGRAPHY.h1, styles.title]}>Bienvenido</Text>

                <View style={styles.form}>
                    <View style={styles.inputGroup}>
                        <Text style={styles.label}>Usuario</Text>
                        <TextInput
                            style={styles.input}
                            value={username}
                            onChangeText={setUsername}
                            placeholder="Introduce tu usuario"
                            autoCapitalize="none"
                        />
                    </View>

                    <View style={styles.inputGroup}>
                        <Text style={styles.label}>Contraseña</Text>
                        <TextInput
                            style={styles.input}
                            value={password}
                            onChangeText={setPassword}
                            placeholder="Introduce tu contraseña"
                            secureTextEntry
                        />
                    </View>

                    <Button
                        title={loading ? "Entrando..." : "Entrar"}
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
