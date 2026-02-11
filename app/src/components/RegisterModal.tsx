import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, Modal, TouchableOpacity, ActivityIndicator, Alert } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { X, User, Mail, Lock } from 'lucide-react-native';
import { apiService } from '../services/apiService';

interface RegisterModalProps {
    visible: boolean;
    onClose: () => void;
}

export const RegisterModal: React.FC<RegisterModalProps> = ({ visible, onClose }) => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);

    const handleRegister = async () => {
        // Client-side validation
        if (!username || !email || !password) {
            const msg = 'Por favor completa todos los campos';
            if (typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
            return;
        }

        if (!email.includes('@') || !email.includes('.')) {
            const msg = 'Por favor introduce un email válido';
            if (typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
            return;
        }

        if (password.length < 6) {
            const msg = 'La contraseña debe tener al menos 6 caracteres';
            if (typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
            return;
        }

        setLoading(true);
        const result = await apiService.register({ username, email, password });
        setLoading(false);

        if (result && !result.error) {
            setSuccess(true);
            // Reset form
            setUsername('');
            setEmail('');
            setPassword('');

            // Show success message for 3 seconds then close
            setTimeout(() => {
                setSuccess(false);
                onClose();
            }, 3000);
        } else {
            const msg = result?.error || result?.message || 'Error al registrar usuario';
            if (typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert('Error', msg);
            }
        }
    };

    return (
        <Modal
            visible={visible}
            transparent
            animationType="fade"
            onRequestClose={onClose}
        >
            <View style={styles.overlay}>
                <View style={styles.modalContainer}>
                    {/* Header */}
                    <View style={styles.header}>
                        <Text style={styles.title}>Crear Cuenta</Text>
                        <TouchableOpacity onPress={onClose} style={styles.closeButton}>
                            <X color={COLORS.textSecondary} size={24} />
                        </TouchableOpacity>
                    </View>

                    {success ? (
                        // Success State
                        <View style={styles.successContainer}>
                            <View style={styles.successIcon}>
                                <Mail color={COLORS.white} size={40} />
                            </View>
                            <Text style={styles.successTitle}>¡Correo enviado!</Text>
                            <Text style={styles.successMessage}>
                                Hemos enviado un enlace de verificación a tu correo electrónico.
                                Por favor revisa tu bandeja de entrada.
                            </Text>
                        </View>
                    ) : (
                        // Form State
                        <View style={styles.form}>
                            <View style={styles.inputGroup}>
                                <Text style={styles.label}>Nombre</Text>
                                <View style={styles.inputWrapper}>
                                    <User color={COLORS.textSecondary} size={20} />
                                    <TextInput
                                        style={styles.input}
                                        value={username}
                                        onChangeText={setUsername}
                                        placeholder="Tu nombre completo"
                                        autoCapitalize="words"
                                        editable={!loading}
                                    />
                                </View>
                            </View>

                            <View style={styles.inputGroup}>
                                <Text style={styles.label}>Email</Text>
                                <View style={styles.inputWrapper}>
                                    <Mail color={COLORS.textSecondary} size={20} />
                                    <TextInput
                                        style={styles.input}
                                        value={email}
                                        onChangeText={setEmail}
                                        placeholder="correo@ejemplo.com"
                                        keyboardType="email-address"
                                        autoCapitalize="none"
                                        editable={!loading}
                                    />
                                </View>
                            </View>

                            <View style={styles.inputGroup}>
                                <Text style={styles.label}>Contraseña</Text>
                                <View style={styles.inputWrapper}>
                                    <Lock color={COLORS.textSecondary} size={20} />
                                    <TextInput
                                        style={styles.input}
                                        value={password}
                                        onChangeText={setPassword}
                                        placeholder="Mínimo 6 caracteres"
                                        secureTextEntry
                                        editable={!loading}
                                    />
                                </View>
                            </View>

                            <TouchableOpacity
                                style={[styles.registerButton, loading && styles.registerButtonDisabled]}
                                onPress={handleRegister}
                                disabled={loading}
                            >
                                {loading ? (
                                    <ActivityIndicator color={COLORS.white} />
                                ) : (
                                    <Text style={styles.registerButtonText}>Registrarse</Text>
                                )}
                            </TouchableOpacity>
                        </View>
                    )}
                </View>
            </View>
        </Modal>
    );
};

const styles = StyleSheet.create({
    overlay: {
        flex: 1,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        justifyContent: 'center',
        alignItems: 'center',
        padding: SPACING.lg,
    },
    modalContainer: {
        backgroundColor: COLORS.white,
        borderRadius: 24,
        padding: SPACING.xl,
        width: '100%',
        maxWidth: 500,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 10 },
        shadowOpacity: 0.3,
        shadowRadius: 20,
        elevation: 10,
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: SPACING.lg,
    },
    title: {
        fontSize: 24,
        fontWeight: 'bold',
        color: COLORS.text,
    },
    closeButton: {
        padding: 4,
    },
    form: {
        gap: SPACING.md,
    },
    inputGroup: {
        gap: SPACING.xs,
    },
    label: {
        fontSize: 14,
        fontWeight: '600',
        color: COLORS.textSecondary,
    },
    inputWrapper: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: '#F5F7F9',
        borderRadius: 12,
        paddingHorizontal: SPACING.md,
        height: 50,
    },
    input: {
        flex: 1,
        marginLeft: 10,
        fontSize: 16,
        color: COLORS.text,
    },
    registerButton: {
        backgroundColor: COLORS.primary,
        padding: 16,
        borderRadius: 16,
        alignItems: 'center',
        marginTop: SPACING.md,
    },
    registerButtonDisabled: {
        opacity: 0.6,
    },
    registerButtonText: {
        color: COLORS.white,
        fontWeight: 'bold',
        fontSize: 16,
    },
    successContainer: {
        alignItems: 'center',
        paddingVertical: SPACING.xl,
    },
    successIcon: {
        width: 80,
        height: 80,
        borderRadius: 40,
        backgroundColor: COLORS.primary,
        justifyContent: 'center',
        alignItems: 'center',
        marginBottom: SPACING.lg,
    },
    successTitle: {
        fontSize: 22,
        fontWeight: 'bold',
        color: COLORS.text,
        marginBottom: SPACING.md,
    },
    successMessage: {
        fontSize: 16,
        color: COLORS.textSecondary,
        textAlign: 'center',
        lineHeight: 24,
    },
});
