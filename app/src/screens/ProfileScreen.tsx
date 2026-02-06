import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, SafeAreaView, TextInput, Alert, ActivityIndicator } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { User, Mail, Lock, Calendar, BookOpen, Trash2, LogOut, Save, ChevronLeft } from 'lucide-react-native';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';

export const ProfileScreen = ({ navigation }: any) => {
    const { user: authUser, logout, updateUser } = useAuth();
    const [user, setUser] = useState<any>(null); // Local user state for form
    const [progress, setProgress] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // Deletion states
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [verificationCode, setVerificationCode] = useState('');
    const [isVerifying, setIsVerifying] = useState(false);

    // Form states
    const [username, setUsername] = useState('');
    const [nickname, setNickname] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    useEffect(() => {
        if (!authUser) return;

        const loadProfile = async () => {
            setLoading(true);
            try {
                // Fetch fresh data
                const [userData, progressData] = await Promise.all([
                    apiService.getUser(authUser.id),
                    apiService.getUserProgress(authUser.id)
                ]);

                if (userData) {
                    setUser(userData);
                    setUsername(userData.username || '');
                    setNickname(userData.nickname || '');
                    setEmail(userData.email || '');
                    // updateUser(userData); // Removed to prevent infinite loop
                }

                if (progressData) {
                    // Handle new structure
                    if (progressData.progress) {
                        setProgress(progressData.progress);
                    } else {
                        setProgress(progressData);
                    }
                }
            } catch (err) {
                console.error("Error loading profile", err);
            }
            setLoading(false);
        };
        loadProfile();
    }, [authUser]);

    const handleUpdate = async () => {
        if (!user) return;
        setSaving(true);
        const result = await apiService.updateProfile(user.id, {
            username,
            nickname,
            // email, // Email not updatable via this form as per requirements
            password: password || undefined
        });

        if (result && !result.error) {
            Alert.alert('Éxito', 'Perfil actualizado correctamente');
            setPassword('');
            // Update local state and context
            const updatedUser = { ...user, username, nickname };
            setUser(updatedUser);
            updateUser(updatedUser);
        } else {
            Alert.alert('Error', 'No se pudo actualizar el perfil');
        }
        setSaving(false);
    };

    const handleLogout = () => {
        if (typeof window !== 'undefined' && window.alert && window.confirm) {
            if (window.confirm('¿Realmente quieres cerrar sesión?')) {
                logout();
                navigation.reset({
                    index: 0,
                    routes: [{ name: 'Onboarding' }],
                });
            }
        } else {
            Alert.alert(
                'Cerrar Sesión',
                '¿Realmente quieres cerrar sesión?',
                [
                    { text: 'Cancelar', style: 'cancel' },
                    {
                        text: 'Cerrar Sesión',
                        style: 'destructive',
                        onPress: () => {
                            logout();
                            navigation.reset({
                                index: 0,
                                routes: [{ name: 'Onboarding' }],
                            });
                        }
                    }
                ]
            );
        }
    };

    const handleRequestDelete = () => {
        Alert.alert(
            'Eliminar Cuenta',
            '¿Estás seguro? Esta acción enviará un código de confirmación a tu correo.',
            [
                { text: 'Cancelar', style: 'cancel' },
                {
                    text: 'Continuar',
                    onPress: async () => {
                        const success = await apiService.requestDeletion(1);
                        if (success) {
                            setShowDeleteModal(true);
                        } else {
                            Alert.alert('Error', 'No se pudo procesar la solicitud.');
                        }
                    }
                }
            ]
        );
    };

    const handleVerifyDelete = async () => {
        if (verificationCode.length !== 6) {
            Alert.alert('Error', 'El código debe tener 6 dígitos.');
            return;
        }

        setIsVerifying(true);
        const success = await apiService.deleteAccount(1, verificationCode);
        setIsVerifying(false);

        if (success) {
            setShowDeleteModal(false);
            Alert.alert('Cuenta Eliminada', 'Tu cuenta ha sido eliminada correctamente.', [
                {
                    text: 'OK',
                    onPress: () => navigation.reset({
                        index: 0,
                        routes: [{ name: 'Onboarding' }],
                    })
                }
            ]);
        } else {
            Alert.alert('Error', 'Código incorrecto o expirado.');
        }
    };

    if (loading) {
        return (
            <View style={styles.loadingContainer}>
                <ActivityIndicator size="large" color={COLORS.primary} />
            </View>
        );
    }

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity
                    onPress={() => {
                        if (navigation.canGoBack()) {
                            navigation.goBack();
                        } else {
                            navigation.navigate('Home');
                        }
                    }}
                    style={styles.backButton}
                >
                    <ChevronLeft color={COLORS.primary} size={28} />
                </TouchableOpacity>
                <Text style={TYPOGRAPHY.h2}>Mi Perfil</Text>
                <View style={{ width: 28 }} />
            </View>

            <ScrollView contentContainerStyle={styles.scrollContent}>
                {/* Profile Stats Card */}
                <View style={styles.statsCard}>
                    <View style={styles.avatarContainer}>
                        <User color={COLORS.white} size={40} />
                    </View>
                    <Text style={styles.statsName}>{user?.username}</Text>
                    <Text style={styles.statsNick}>@{user?.nickname}</Text>

                    <View style={styles.statsDivider} />

                    <View style={styles.statsGrid}>
                        <View style={styles.statsItem}>
                            <Calendar color={COLORS.textSecondary} size={20} />
                            <Text style={styles.statsLabel}>Unido</Text>
                            <Text style={styles.statsValue}>{user?.joinedAt ? new Date(user.joinedAt).toLocaleDateString() : 'N/A'}</Text>
                        </View>
                        <View style={styles.statsItem}>
                            <BookOpen color={COLORS.textSecondary} size={20} />
                            <Text style={styles.statsLabel}>Última Lección</Text>
                            <Text style={styles.statsValue}>{progress?.lastLessonTitle || 'N/A'}</Text>
                        </View>
                        <View style={styles.statsItem}>
                            <Text style={styles.statsValueLarge}>{progress?.xp || 0}</Text>
                            <Text style={styles.statsLabel}>XP Total</Text>
                        </View>
                    </View>
                </View>

                {/* Verification View for Deletion */}
                {showDeleteModal && (
                    <View style={styles.formSection}>
                        <Text style={[styles.formTitle, { color: COLORS.error }]}>Confirmar Eliminación</Text>
                        <Text style={styles.label}>Introduce el código de 6 dígitos enviado a tu correo:</Text>
                        <View style={[styles.inputWrapper, { marginTop: 10 }]}>
                            <TextInput
                                style={[styles.input, { textAlign: 'center', fontSize: 24, letterSpacing: 8 }]}
                                value={verificationCode}
                                onChangeText={setVerificationCode}
                                placeholder="000000"
                                keyboardType="number-pad"
                                maxLength={6}
                            />
                        </View>
                        <TouchableOpacity
                            style={[styles.saveButton, { backgroundColor: COLORS.error, marginTop: 20 }]}
                            onPress={handleVerifyDelete}
                            disabled={isVerifying}
                        >
                            {isVerifying ? <ActivityIndicator color={COLORS.white} /> : (
                                <Text style={styles.saveButtonText}>Confirmar y Eliminar</Text>
                            )}
                        </TouchableOpacity>
                        <TouchableOpacity
                            style={{ alignSelf: 'center', marginTop: 15 }}
                            onPress={() => setShowDeleteModal(false)}
                        >
                            <Text style={{ color: COLORS.textSecondary }}>Cancelar</Text>
                        </TouchableOpacity>
                    </View>
                )}

                {/* Edit Form */}
                {!showDeleteModal && (
                    <View style={styles.formSection}>
                        <Text style={styles.formTitle}>Editar Información</Text>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Nombre Completo</Text>
                            <View style={styles.inputWrapper}>
                                <User color={COLORS.textSecondary} size={20} />
                                <TextInput
                                    style={styles.input}
                                    value={username}
                                    onChangeText={setUsername}
                                    placeholder="Tu nombre"
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Nombre de Usuario (Nick)</Text>
                            <View style={styles.inputWrapper}>
                                <Text style={styles.atSymbol}>@</Text>
                                <TextInput
                                    style={styles.input}
                                    value={nickname}
                                    onChangeText={setNickname}
                                    placeholder="tu_nick"
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Correo Electrónico</Text>
                            <View style={styles.inputWrapper}>
                                <Mail color={COLORS.textSecondary} size={20} />
                                <TextInput
                                    placeholder="correo@ejemplo.com"
                                    keyboardType="email-address"
                                    editable={false}
                                    style={[styles.input, { color: COLORS.textSecondary }]}
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>Nueva Contraseña</Text>
                            <View style={styles.inputWrapper}>
                                <Lock color={COLORS.textSecondary} size={20} />
                                <TextInput
                                    style={styles.input}
                                    value={password}
                                    onChangeText={setPassword}
                                    placeholder="••••••••"
                                    secureTextEntry
                                />
                            </View>
                        </View>

                        <TouchableOpacity
                            style={styles.saveButton}
                            onPress={handleUpdate}
                            disabled={saving}
                        >
                            {saving ? <ActivityIndicator color={COLORS.white} /> : (
                                <>
                                    <Save color={COLORS.white} size={20} />
                                    <Text style={styles.saveButtonText}>Guardar Cambios</Text>
                                </>
                            )}
                        </TouchableOpacity>
                    </View>
                )}

                {/* Danger Zone */}
                <View style={styles.dangerZone}>
                    <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
                        <LogOut color={COLORS.textSecondary} size={20} />
                        <Text style={styles.logoutText}>Cerrar Sesión</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.deleteButton} onPress={handleRequestDelete}>
                        <Trash2 color={COLORS.error} size={20} />
                        <Text style={styles.deleteText}>Eliminar Cuenta</Text>
                    </TouchableOpacity>
                </View>
            </ScrollView>
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
    },
    loadingContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: SPACING.md,
        backgroundColor: COLORS.white,
        borderBottomWidth: 1,
        borderBottomColor: '#F0F0F0',
    },
    backButton: {
        padding: 4,
    },
    scrollContent: {
        padding: SPACING.lg,
    },
    statsCard: {
        backgroundColor: COLORS.white,
        borderRadius: 24,
        padding: SPACING.xl,
        alignItems: 'center',
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 4 },
        shadowOpacity: 0.1,
        shadowRadius: 10,
        elevation: 5,
        marginBottom: SPACING.xl,
    },
    avatarContainer: {
        width: 80,
        height: 80,
        borderRadius: 40,
        backgroundColor: COLORS.primary,
        justifyContent: 'center',
        alignItems: 'center',
        marginBottom: SPACING.md,
    },
    statsName: {
        fontSize: 22,
        fontWeight: 'bold',
        color: COLORS.text,
    },
    statsNick: {
        fontSize: 16,
        color: COLORS.textSecondary,
        marginBottom: SPACING.lg,
    },
    statsDivider: {
        width: '100%',
        height: 1,
        backgroundColor: '#F0F0F0',
        marginBottom: SPACING.lg,
    },
    statsGrid: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        width: '100%',
    },
    statsItem: {
        alignItems: 'center',
        flex: 1,
    },
    statsLabel: {
        fontSize: 12,
        color: COLORS.textSecondary,
        marginTop: 4,
    },
    statsValue: {
        fontSize: 14,
        fontWeight: '600',
        color: COLORS.text,
        textAlign: 'center',
    },
    statsValueLarge: {
        fontSize: 24,
        fontWeight: 'bold',
        color: COLORS.primary,
    },
    formSection: {
        backgroundColor: COLORS.white,
        borderRadius: 24,
        padding: SPACING.xl,
        marginBottom: SPACING.xl,
    },
    formTitle: {
        fontSize: 18,
        fontWeight: 'bold',
        marginBottom: SPACING.lg,
        color: COLORS.text,
    },
    inputGroup: {
        marginBottom: SPACING.md,
    },
    label: {
        fontSize: 14,
        fontWeight: '600',
        color: COLORS.textSecondary,
        marginBottom: 8,
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
    atSymbol: {
        fontSize: 18,
        color: COLORS.textSecondary,
        fontWeight: 'bold',
    },
    saveButton: {
        backgroundColor: COLORS.primary,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 16,
        borderRadius: 16,
        marginTop: SPACING.md,
    },
    saveButtonText: {
        color: COLORS.white,
        fontWeight: 'bold',
        fontSize: 16,
        marginLeft: 10,
    },
    dangerZone: {
        paddingBottom: 40,
    },
    logoutButton: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 16,
        marginBottom: SPACING.md,
    },
    logoutText: {
        color: COLORS.textSecondary,
        fontWeight: '600',
        marginLeft: 10,
    },
    deleteButton: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 16,
        borderWidth: 1,
        borderColor: '#FFEBEB',
        borderRadius: 16,
    },
    deleteText: {
        color: COLORS.error,
        fontWeight: '600',
        marginLeft: 10,
    }
});
