import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, SafeAreaView, TextInput, Alert, ActivityIndicator, Platform, Modal } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { User, Mail, Lock, Calendar, BookOpen, Trash2, LogOut, Save, ChevronLeft, Globe, Check } from 'lucide-react-native';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';
import { LanguageSelector } from '../components/LanguageSelector';

export const ProfileScreen = ({ navigation }: any) => {
    const { t } = useTranslation();
    const { user: authUser, logout, updateUser, updateLanguage } = useAuth();
    const [user, setUser] = useState<any>(null); // Local user state for form
    const [progress, setProgress] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);

    // Deletion states
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [verificationCode, setVerificationCode] = useState('');
    const [isVerifying, setIsVerifying] = useState(false);

    // Confirmation modals (replacing window.confirm for Chrome compatibility)
    const [showLogoutModal, setShowLogoutModal] = useState(false);
    const [showDeactivationModal, setShowDeactivationModal] = useState(false);

    // Form states
    const [username, setUsername] = useState('');
    const [nickname, setNickname] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    useEffect(() => {
        if (!authUser?.id) return;

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
                }

                if (progressData) {
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
    }, [authUser?.id]);

    // Track if form has any unsaved modifications
    const hasChanges = username !== (user?.username || '') ||
        nickname !== (user?.nickname || '') ||
        password.length > 0;

    const handleUpdate = async () => {
        if (!user || !hasChanges) return;
        setSaving(true);
        const result = await apiService.updateProfile(user.id, {
            username,
            nickname,
            password: password || undefined
        });

        if (result && !result.error) {
            setPassword('');
            // Update local state and context
            const updatedUser = { ...user, username, nickname };
            setUser(updatedUser);
            updateUser(updatedUser);

            // Trigger the visual success effect
            setIsSuccess(true);
            setTimeout(() => {
                setIsSuccess(false);
            }, 2000);
        } else {
            const msg = t('profile.errors.updateFailed');
            if (Platform.OS === 'web' && typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert(t('common.error'), msg);
            }
        }
        setSaving(false);
    };

    const handleLanguageChange = async (languageCode: string) => {
        const result = await updateLanguage(languageCode);
        if (result.success) {
            const msg = t('profile.success.languageUpdated');
            if (Platform.OS === 'web' && typeof window !== 'undefined') {
                // Not showing alert for language change to be smoother
            } else {
                Alert.alert(t('common.success'), msg);
            }
        } else {
            const msg = t('profile.errors.updateFailed');
            if (Platform.OS === 'web' && typeof window !== 'undefined') {
                window.alert(msg);
            } else {
                Alert.alert(t('common.error'), msg);
            }
        }
    };

    const handleLogout = () => {
        if (Platform.OS === 'web') {
            setShowLogoutModal(true);
        } else {
            Alert.alert(
                t('profile.logout'),
                t('profile.confirmLogout'),
                [
                    { text: t('common.cancel'), style: 'cancel' },
                    {
                        text: t('profile.logout'),
                        style: 'destructive',
                        onPress: () => {
                            logout();
                            navigation.reset({ index: 0, routes: [{ name: 'Onboarding' }] });
                        }
                    }
                ]
            );
        }
    };

    const confirmLogout = () => {
        setShowLogoutModal(false);
        logout();
        navigation.reset({ index: 0, routes: [{ name: 'Onboarding' }] });
    };

    const handleRequestDeactivation = () => {
        if (Platform.OS === 'web') {
            setShowDeactivationModal(true);
        } else {
            Alert.alert(
                t('profile.deleteAccount'),
                t('profile.deactivateConfirm'),
                [
                    { text: t('common.cancel'), style: 'cancel' },
                    { text: t('common.confirm'), onPress: performDeactivationRequest }
                ]
            );
        }
    };

    const performDeactivationRequest = async () => {
        setSaving(true);
        const result = await apiService.requestAccountDeactivation(user?.id || 0);
        setSaving(false);

        if (result && !result.error) {
            const feedbackMsg = t('profile.deactivateFeedback');
            if (Platform.OS === 'web' && typeof window !== 'undefined') {
                window.alert(feedbackMsg);
            } else {
                Alert.alert(t('common.success'), feedbackMsg);
            }
        } else {
            const errMsg = result?.error || t('profile.errors.deleteFailed');
            if (Platform.OS === 'web' && typeof window !== 'undefined') {
                window.alert(errMsg);
            } else {
                Alert.alert(t('common.error'), errMsg);
            }
        }
    };

    const handleVerifyDelete = async () => {
        if (verificationCode.length !== 6) {
            Alert.alert(t('common.error'), t('profile.errors.invalidCode'));
            return;
        }

        setIsVerifying(true);
        const success = await apiService.deleteAccount(user?.id || 0, verificationCode);
        setIsVerifying(false);

        if (success) {
            setShowDeleteModal(false);
            Alert.alert(t('profile.success.accountDeleted'), t('profile.success.accountDeleted'), [
                {
                    text: 'OK',
                    onPress: () => navigation.reset({
                        index: 0,
                        routes: [{ name: 'Onboarding' }],
                    })
                }
            ]);
        } else {
            Alert.alert(t('common.error'), t('profile.errors.invalidCode'));
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
                <Text style={TYPOGRAPHY.h2}>{t('profile.title')}</Text>
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
                            <Text style={styles.statsLabel}>{t('profile.joinedAt')}</Text>
                            <Text style={styles.statsValue}>{user?.joinedAt ? new Date(user.joinedAt).toLocaleDateString() : 'N/A'}</Text>
                        </View>
                        <View style={styles.statsItem}>
                            <BookOpen color={COLORS.textSecondary} size={20} />
                            <Text style={styles.statsLabel}>{t('home.continueLesson')}</Text>
                            <Text style={styles.statsValue}>{progress?.lastLessonTitle || 'N/A'}</Text>
                        </View>
                        <View style={styles.statsItem}>
                            <Text style={styles.statsValueLarge}>{progress?.xp || 0}</Text>
                            <Text style={styles.statsLabel}>{t('home.totalXP')}</Text>
                        </View>
                    </View>
                </View>

                {/* Verification View for Deletion */}
                {showDeleteModal && (
                    <View style={styles.formSection}>
                        <Text style={[styles.formTitle, { color: COLORS.error }]}>{t('common.confirm')} {t('profile.deleteAccount')}</Text>
                        <Text style={styles.label}>{t('profile.deleteInstructions')}</Text>
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
                                <Text style={styles.saveButtonText}>{t('common.confirm')} & {t('common.delete')}</Text>
                            )}
                        </TouchableOpacity>
                        <TouchableOpacity
                            style={{ alignSelf: 'center', marginTop: 15 }}
                            onPress={() => setShowDeleteModal(false)}
                        >
                            <Text style={{ color: COLORS.textSecondary }}>{t('common.cancel')}</Text>
                        </TouchableOpacity>
                    </View>
                )}

                {/* App Preferences Section */}
                {!showDeleteModal && (
                    <View style={styles.formSection}>
                        <Text style={styles.formTitle}>{t('profile.appPreferences')}</Text>
                        <View style={styles.inputGroup}>
                            <LanguageSelector onLanguageChange={handleLanguageChange} />
                        </View>
                    </View>
                )}

                {/* Edit Form */}
                {!showDeleteModal && (
                    <View style={styles.formSection}>
                        <Text style={styles.formTitle}>{t('profile.userSettings')}</Text>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>{t('register.name')}</Text>
                            <View style={styles.inputWrapper}>
                                <User color={COLORS.textSecondary} size={20} />
                                <TextInput
                                    style={styles.input}
                                    value={username}
                                    onChangeText={setUsername}
                                    placeholder={t('register.name')}
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>{t('profile.nickname')}</Text>
                            <View style={styles.inputWrapper}>
                                <Text style={styles.atSymbol}>@</Text>
                                <TextInput
                                    style={styles.input}
                                    value={nickname}
                                    onChangeText={setNickname}
                                    placeholder={t('profile.nickname')}
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>{t('profile.email')}</Text>
                            <View style={styles.inputWrapper}>
                                <Mail color={COLORS.textSecondary} size={20} />
                                <TextInput
                                    placeholder={t('profile.email')}
                                    keyboardType="email-address"
                                    editable={false}
                                    value={email}
                                    style={[styles.input, { color: COLORS.textSecondary }]}
                                />
                            </View>
                        </View>

                        <View style={styles.inputGroup}>
                            <Text style={styles.label}>{t('register.password')}</Text>
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
                            style={[
                                styles.saveButton,
                                (!hasChanges || saving) && styles.saveButtonDisabled,
                                isSuccess && styles.saveButtonSuccess
                            ]}
                            onPress={handleUpdate}
                            disabled={!hasChanges || saving}
                        >
                            {saving ? <ActivityIndicator color={COLORS.white} /> : (
                                isSuccess ? (
                                    <>
                                        <Check color={COLORS.white} size={20} />
                                        <Text style={styles.saveButtonText}>{t('profile.success.profileUpdated', '¡Guardado!')}</Text>
                                    </>
                                ) : (
                                    <>
                                        <Save color={COLORS.white} size={20} />
                                        <Text style={styles.saveButtonText}>{t('profile.saveChanges')}</Text>
                                    </>
                                )
                            )}
                        </TouchableOpacity>
                    </View>
                )}

                {/* Danger Zone */}
                <View style={styles.dangerZone}>
                    <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
                        <LogOut color={COLORS.textSecondary} size={20} />
                        <Text style={styles.logoutText}>{t('profile.logout')}</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.deleteButton} onPress={handleRequestDeactivation}>
                        <Trash2 color={COLORS.error} size={20} />
                        <Text style={styles.deleteText}>{t('profile.deleteAccount')}</Text>
                    </TouchableOpacity>
                </View>
            </ScrollView>

            {/* Logout Confirmation Modal */}
            <Modal visible={showLogoutModal} transparent animationType="fade" onRequestClose={() => setShowLogoutModal(false)}>
                <View style={styles.modalOverlay}>
                    <View style={styles.modalBox}>
                        <Text style={styles.modalTitle}>{t('profile.logout')}</Text>
                        <Text style={styles.modalMessage}>{t('profile.confirmLogout')}</Text>
                        <View style={styles.modalButtons}>
                            <TouchableOpacity style={styles.modalCancel} onPress={() => setShowLogoutModal(false)}>
                                <Text style={styles.modalCancelText}>{t('common.cancel')}</Text>
                            </TouchableOpacity>
                            <TouchableOpacity style={styles.modalConfirm} onPress={confirmLogout}>
                                <Text style={styles.modalConfirmText}>{t('profile.logout')}</Text>
                            </TouchableOpacity>
                        </View>
                    </View>
                </View>
            </Modal>

            {/* Deactivation Confirmation Modal */}
            <Modal visible={showDeactivationModal} transparent animationType="fade" onRequestClose={() => setShowDeactivationModal(false)}>
                <View style={styles.modalOverlay}>
                    <View style={styles.modalBox}>
                        <Text style={styles.modalTitle}>{t('profile.deleteAccount')}</Text>
                        <Text style={styles.modalMessage}>{t('profile.deactivateConfirm')}</Text>
                        <View style={styles.modalButtons}>
                            <TouchableOpacity style={styles.modalCancel} onPress={() => setShowDeactivationModal(false)}>
                                <Text style={styles.modalCancelText}>{t('common.cancel')}</Text>
                            </TouchableOpacity>
                            <TouchableOpacity style={[styles.modalConfirm, { backgroundColor: COLORS.error }]} onPress={() => { setShowDeactivationModal(false); performDeactivationRequest(); }}>
                                <Text style={styles.modalConfirmText}>{t('common.confirm')}</Text>
                            </TouchableOpacity>
                        </View>
                    </View>
                </View>
            </Modal>
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
        textAlign: 'center',
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
    saveButtonDisabled: {
        backgroundColor: COLORS.textSecondary,
        opacity: 0.5,
    },
    saveButtonSuccess: {
        backgroundColor: COLORS.success || '#4CAF50',
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
    },
    modalOverlay: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.5)',
        justifyContent: 'center',
        alignItems: 'center',
        padding: 24,
    },
    modalBox: {
        backgroundColor: COLORS.white,
        borderRadius: 20,
        padding: 24,
        width: '100%',
        maxWidth: 360,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 8 },
        shadowOpacity: 0.2,
        shadowRadius: 16,
        elevation: 10,
    },
    modalTitle: {
        fontSize: 18,
        fontWeight: 'bold',
        color: COLORS.text,
        marginBottom: 10,
    },
    modalMessage: {
        fontSize: 15,
        color: COLORS.textSecondary,
        marginBottom: 24,
        lineHeight: 22,
    },
    modalButtons: {
        flexDirection: 'row',
        gap: 12,
    },
    modalCancel: {
        flex: 1,
        padding: 14,
        borderRadius: 12,
        borderWidth: 1,
        borderColor: '#E0E0E0',
        alignItems: 'center',
    },
    modalCancelText: {
        color: COLORS.textSecondary,
        fontWeight: '600',
    },
    modalConfirm: {
        flex: 1,
        padding: 14,
        borderRadius: 12,
        backgroundColor: COLORS.primary,
        alignItems: 'center',
    },
    modalConfirmText: {
        color: COLORS.white,
        fontWeight: '700',
    },
});
