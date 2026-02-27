import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, SafeAreaView, ActivityIndicator, Modal } from 'react-native';
import { LessonProgressService } from '../services/LessonProgressService';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { BookOpen, Trophy, MessageCircle, User as UserIcon, Shield } from 'lucide-react-native';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';

export const HomeScreen = ({ navigation }: any) => {
    const { t } = useTranslation();
    const { user, isAdmin } = useAuth();
    const [lessons, setLessons] = useState<any[]>([]);
    const [progress, setProgress] = useState<any>(null);
    const [lessonScores, setLessonScores] = useState<any[]>([]);
    const [globalRank, setGlobalRank] = useState<{ rank: number; total: number } | null>(null);
    const [loading, setLoading] = useState(true);

    // Resume-conflict dialog state
    const [resumeDialog, setResumeDialog] = useState<{
        visible: boolean;
        savedLevelId: string;
        targetLevelId: string;
        exerciseInfo: string;
    } | null>(null);

    /** Navigate to a level, checking for an in-progress lesson first */
    const handleLevelPress = async (targetLevelId: string) => {
        const saved = await LessonProgressService.load();

        if (saved && saved.levelId !== targetLevelId && saved.exercises.length > 0) {
            // There is an in-progress lesson for a DIFFERENT level — show custom modal
            setResumeDialog({
                visible: true,
                savedLevelId: saved.levelId,
                targetLevelId,
                exerciseInfo: `ejercicio ${saved.currentExerciseIndex + 1} de ${saved.exercises.length}`,
            });
        } else {
            // No conflict: navigate directly
            navigation.navigate('Lesson', { levelId: targetLevelId });
        }
    };

    const handleResumeOld = () => {
        if (!resumeDialog) return;
        setResumeDialog(null);
        navigation.navigate('Lesson', { levelId: resumeDialog.savedLevelId });
    };

    const handleDiscardAndStart = async () => {
        if (!resumeDialog) return;
        const target = resumeDialog.targetLevelId;
        setResumeDialog(null);
        await LessonProgressService.clear();
        navigation.navigate('Lesson', { levelId: target });
    };

    useEffect(() => {
        const fetchData = async () => {
            if (!user) return;

            setLoading(true);
            try {
                const [lessonsData, progressData, rankData] = await Promise.all([
                    apiService.getLessons(),
                    apiService.getUserProgress(user.id),
                    apiService.getGlobalRank(user.id)
                ]);

                setLessons(lessonsData);
                setGlobalRank(rankData);
                if (progressData && progressData.progress) {
                    setProgress(progressData.progress);
                    setLessonScores(progressData.lessonScores || []);
                } else {
                    setProgress(progressData);
                }
            } catch (err) {
                console.error("Error fetching home data", err);
            }
            setLoading(false);
        };
        fetchData();
    }, [user]);

    if (loading || !user) {
        return (
            <View style={styles.loadingContainer}>
                <ActivityIndicator size="large" color={COLORS.primary} />
            </View>
        );
    }

    const getLessonScore = (lessonId: number) => {
        const score = lessonScores.find(s => s.lessonId === lessonId);
        return score ? `${score.correctAnswers}/${score.totalQuestions}` : null;
    };

    return (
        <>
            <SafeAreaView style={styles.container}>
                {/* Header */}
                <View style={styles.header}>
                    <View style={styles.headerLeft}>
                        <Text style={styles.userName}>{user.username || t('common.user')}</Text>
                        <View style={styles.statsRow}>
                            <View style={styles.stat}>
                                <Text style={styles.statIcon}>⭐</Text>
                                <Text style={styles.statText}>{progress?.xp || 0} XP</Text>
                            </View>
                            {globalRank && (
                                <View style={styles.stat}>
                                    <Text style={styles.statIcon}>🏆</Text>
                                    <Text style={styles.statText}>#{globalRank.rank} / {globalRank.total}</Text>
                                </View>
                            )}
                        </View>
                    </View>
                    {isAdmin && (
                        <TouchableOpacity
                            style={styles.adminIcon}
                            onPress={() => navigation.navigate('Admin')}
                        >
                            <Shield color={COLORS.primary} size={28} />
                        </TouchableOpacity>
                    )}
                    <TouchableOpacity
                        style={styles.profileIcon}
                        onPress={() => navigation.navigate('Profile')}
                    >
                        <UserIcon color={COLORS.primary} size={28} />
                    </TouchableOpacity>
                </View>

                <ScrollView contentContainerStyle={styles.scrollContent}>
                    <View style={styles.levelsContainer}>
                        <TouchableOpacity
                            style={[styles.levelNode, { backgroundColor: '#4CAF50' }]}
                            onPress={() => handleLevelPress('A1')}
                        >
                            <Text style={styles.levelTitle}>A1</Text>
                        </TouchableOpacity>

                        <TouchableOpacity
                            style={[styles.levelNode, { backgroundColor: '#FF9800', marginLeft: 80 }]}
                            onPress={() => handleLevelPress('A2')}
                        >
                            <Text style={styles.levelTitle}>A2</Text>
                        </TouchableOpacity>

                        <TouchableOpacity
                            style={[styles.levelNode, { backgroundColor: '#2196F3', marginLeft: -80 }]}
                            onPress={() => handleLevelPress('B1')}
                        >
                            <Text style={styles.levelTitle}>B1</Text>
                        </TouchableOpacity>
                    </View>
                </ScrollView>

                {/* Bottom Tabs Placeholder */}
                <View style={styles.tabBar}>
                    <TouchableOpacity style={styles.tabItem}>
                        <BookOpen color={COLORS.primary} />
                    </TouchableOpacity>
                    <TouchableOpacity style={styles.tabItem} onPress={() => navigation.navigate('AIChat')}>
                        <MessageCircle color={COLORS.textSecondary} />
                    </TouchableOpacity>
                    <TouchableOpacity style={styles.tabItem} onPress={() => navigation.navigate('Leaderboard')}>
                        <Trophy color={COLORS.textSecondary} />
                    </TouchableOpacity>
                </View>
            </SafeAreaView>

            {/* Resume-conflict dialog — Modal works on web; Alert.alert does not */}
            {resumeDialog != null && (
                <Modal
                    transparent
                    animationType="fade"
                    visible={resumeDialog.visible}
                    onRequestClose={() => setResumeDialog(null)}
                >
                    <View style={styles.modalOverlay}>
                        <View style={styles.dialogBox}>
                            <Text style={styles.dialogTitle}>⚠️ Lección en curso</Text>
                            <Text style={styles.dialogMessage}>
                                Tienes una lección de{' '}
                                <Text style={{ fontWeight: 'bold' }}>{resumeDialog.savedLevelId}</Text>
                                {' '}sin terminar ({resumeDialog.exerciseInfo}).{' '}
                                Si empiezas{' '}
                                <Text style={{ fontWeight: 'bold' }}>{resumeDialog.targetLevelId}</Text>
                                {' '}perderás ese avance.
                            </Text>
                            <TouchableOpacity style={styles.dialogBtnPrimary} onPress={handleResumeOld}>
                                <Text style={styles.dialogBtnPrimaryText}>
                                    Volver a {resumeDialog.savedLevelId}
                                </Text>
                            </TouchableOpacity>
                            <TouchableOpacity style={styles.dialogBtnDestructive} onPress={handleDiscardAndStart}>
                                <Text style={styles.dialogBtnDestructiveText}>
                                    Empezar {resumeDialog.targetLevelId}
                                </Text>
                            </TouchableOpacity>
                        </View>
                    </View>
                </Modal>
            )}
        </>
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
        backgroundColor: COLORS.background,
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: SPACING.md,
        borderBottomWidth: 1,
        borderBottomColor: '#EEE',
    },
    headerLeft: {
        flex: 1,
    },
    userName: {
        fontSize: 18,
        fontWeight: 'bold',
        color: COLORS.primary,
        marginBottom: 4,
    },
    statsRow: {
        flexDirection: 'row',
        gap: SPACING.sm,
    },
    stat: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: COLORS.surface,
        paddingHorizontal: SPACING.sm,
        paddingVertical: 4,
        borderRadius: 12,
    },
    statIcon: {
        fontSize: 16,
        marginRight: 4,
    },
    statText: {
        fontWeight: 'bold',
        color: COLORS.text,
        fontSize: 12,
    },
    profileIcon: {
        padding: 8,
    },
    adminIcon: {
        padding: 8,
        marginRight: 4,
    },
    scrollContent: {
        padding: SPACING.lg,
        alignItems: 'center',
    },
    sectionTitle: {
        marginBottom: SPACING.xl,
        textAlign: 'center',
    },
    levelsContainer: {
        width: '100%',
        alignItems: 'center',
        marginTop: SPACING.xl,
        gap: SPACING.xl,
    },
    levelNode: {
        width: 120,
        height: 120,
        borderRadius: 60,
        alignItems: 'center',
        justifyContent: 'center',
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 4 },
        shadowOpacity: 0.3,
        shadowRadius: 5,
        elevation: 8,
    },
    levelTitle: {
        fontSize: 36,
        fontWeight: 'bold',
        color: COLORS.white,
    },
    tabBar: {
        flexDirection: 'row',
        height: 60,
        borderTopWidth: 1,
        borderTopColor: '#EEE',
        justifyContent: 'space-around',
        alignItems: 'center',
    },
    tabItem: {
        padding: 10,
    },
    scoreText: {
        fontSize: 10,
        color: COLORS.primary,
        fontWeight: 'bold',
        marginTop: 2,
    },
    // Resume-conflict dialog styles
    modalOverlay: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.55)',
        justifyContent: 'center',
        alignItems: 'center',
        padding: 24,
    },
    dialogBox: {
        backgroundColor: '#fff',
        borderRadius: 20,
        padding: 24,
        width: '100%',
        maxWidth: 420,
        gap: 12,
    },
    dialogTitle: {
        fontSize: 20,
        fontWeight: 'bold',
        color: COLORS.text,
        marginBottom: 4,
    },
    dialogMessage: {
        fontSize: 15,
        color: COLORS.textSecondary,
        lineHeight: 22,
        marginBottom: 8,
    },
    dialogBtnPrimary: {
        backgroundColor: COLORS.primary,
        borderRadius: 12,
        paddingVertical: 14,
        alignItems: 'center',
    },
    dialogBtnPrimaryText: {
        color: '#fff',
        fontSize: 16,
        fontWeight: '700',
    },
    dialogBtnDestructive: {
        borderRadius: 12,
        paddingVertical: 14,
        alignItems: 'center',
        borderWidth: 1.5,
        borderColor: COLORS.secondary,
    },
    dialogBtnDestructiveText: {
        color: COLORS.secondary,
        fontSize: 16,
        fontWeight: '600',
    },
});

