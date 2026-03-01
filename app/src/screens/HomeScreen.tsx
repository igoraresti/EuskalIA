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
                            style={[styles.levelNode, { backgroundColor: COLORS.primary, borderBottomColor: COLORS.primaryDark }]}
                            onPress={() => handleLevelPress('A1')}
                        >
                            <Text style={styles.levelTitle}>A1</Text>
                        </TouchableOpacity>

                        <TouchableOpacity
                            style={[styles.levelNode, { backgroundColor: COLORS.accent, borderBottomColor: COLORS.accentDark, marginLeft: 80 }]}
                            onPress={() => handleLevelPress('A2')}
                        >
                            <Text style={styles.levelTitle}>A2</Text>
                        </TouchableOpacity>

                        <TouchableOpacity
                            style={[styles.levelNode, { backgroundColor: COLORS.secondary, borderBottomColor: COLORS.secondaryDark, marginLeft: -80 }]}
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
        borderBottomWidth: 2,
        borderBottomColor: COLORS.border,
    },
    headerLeft: {
        flex: 1,
    },
    userName: {
        fontSize: 20,
        fontWeight: '800',
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
        paddingHorizontal: 12,
        paddingVertical: 6,
        borderRadius: 16,
        borderBottomWidth: 2,
        borderBottomColor: '#E5E5E5',
    },
    statIcon: {
        fontSize: 16,
        marginRight: 4,
    },
    statText: {
        fontWeight: '800',
        color: COLORS.text,
        fontSize: 14,
    },
    profileIcon: {
        padding: 8,
    },
    adminIcon: {
        padding: 8,
        marginRight: 4,
    },
    scrollContent: {
        paddingTop: SPACING.xl,
        paddingBottom: 100,
        alignItems: 'center',
    },
    levelsContainer: {
        width: '100%',
        alignItems: 'center',
        gap: 40,
    },
    levelNode: {
        width: 100,
        height: 100,
        borderRadius: 50,
        alignItems: 'center',
        justifyContent: 'center',
        borderBottomWidth: 8, // Intense 3D effect for levels
    },
    levelTitle: {
        fontSize: 32,
        fontWeight: '900',
        color: COLORS.white,
    },
    tabBar: {
        flexDirection: 'row',
        height: 70,
        borderTopWidth: 2,
        borderTopColor: COLORS.border,
        justifyContent: 'space-around',
        alignItems: 'center',
        backgroundColor: COLORS.white,
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
        backgroundColor: 'rgba(0,0,0,0.5)',
        justifyContent: 'center',
        alignItems: 'center',
        padding: 24,
    },
    dialogBox: {
        backgroundColor: '#fff',
        borderRadius: 24,
        padding: 24,
        width: '100%',
        maxWidth: 420,
        borderWidth: 2,
        borderColor: COLORS.border,
        borderBottomWidth: 8,
        borderBottomColor: COLORS.border,
    },
    dialogTitle: {
        fontSize: 22,
        fontWeight: '900',
        color: COLORS.text,
        marginBottom: 8,
        textAlign: 'center',
    },
    dialogMessage: {
        fontSize: 17,
        color: COLORS.textSecondary,
        lineHeight: 24,
        marginBottom: 20,
        textAlign: 'center',
    },
    dialogBtnPrimary: {
        backgroundColor: COLORS.primary,
        borderRadius: 16,
        paddingVertical: 14,
        alignItems: 'center',
        borderBottomWidth: 4,
        borderBottomColor: COLORS.primaryDark,
        marginBottom: 12,
    },
    dialogBtnPrimaryText: {
        color: '#fff',
        fontSize: 18,
        fontWeight: '800',
        textTransform: 'uppercase',
    },
    dialogBtnDestructive: {
        backgroundColor: COLORS.white,
        borderRadius: 16,
        paddingVertical: 14,
        alignItems: 'center',
        borderWidth: 2,
        borderColor: COLORS.border,
        borderBottomWidth: 4,
        borderBottomColor: COLORS.border,
    },
    dialogBtnDestructiveText: {
        color: COLORS.secondary,
        fontSize: 18,
        fontWeight: '800',
        textTransform: 'uppercase',
    },
});

