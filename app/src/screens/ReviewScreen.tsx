import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, ActivityIndicator } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { X, CheckCircle2, AlertCircle } from 'lucide-react-native';
import { Button } from '../components/Button';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';

export const ReviewScreen = ({ navigation }: any) => {
    const { t, i18n } = useTranslation();
    const { user } = useAuth();

    const [exercises, setExercises] = useState<any[]>([]);
    const [currentExerciseIndex, setCurrentExerciseIndex] = useState(0);
    const [selectedOption, setSelectedOption] = useState<string | null>(null);
    const [isChecked, setIsChecked] = useState(false);
    const [isCorrect, setIsCorrect] = useState(false);
    const [loading, setLoading] = useState(true);
    const [correctCount, setCorrectCount] = useState(0);

    useEffect(() => {
        const fetchReviewSession = async () => {
            if (!user) return;
            setLoading(true);
            try {
                const data = await apiService.getSrsSession(user.id);
                setExercises(data || []);
            } catch (e) {
                console.error("Failed loading review session", e);
            } finally {
                setLoading(false);
            }
        };
        fetchReviewSession();
    }, [user]);

    const handleCheck = async () => {
        if (selectedOption !== null && exercises.length > 0) {
            const currentExercise = exercises[currentExerciseIndex];
            const correct = selectedOption === currentExercise.correctAnswer;
            setIsCorrect(correct);
            setIsChecked(true);

            if (correct) {
                setCorrectCount(prev => prev + 1);
            }

            // Record result in SRS
            if (user && currentExercise.lesson && currentExercise.lesson.topic) {
                await apiService.recordSrsReview(user.id, currentExercise.lesson.topic, correct);
            }
        }
    };

    const handleContinue = () => {
        if (currentExerciseIndex < exercises.length - 1) {
            setCurrentExerciseIndex(prev => prev + 1);
            setSelectedOption(null);
            setIsChecked(false);
        } else {
            // Review session finished
            navigation.navigate('Home');
        }
    };

    if (loading) {
        return (
            <View style={styles.loadingContainer}>
                <ActivityIndicator size="large" color={COLORS.primary} />
            </View>
        );
    }

    if (exercises.length === 0) {
        return (
            <View style={styles.loadingContainer}>
                <Text style={TYPOGRAPHY.h2}>{t('review.nothingToReview') || '¡Todo al día!'}</Text>
                <Text style={styles.subtitle}>Has completado todos tus temas pendientes por ahora.</Text>
                <Button title={t('common.back')} onPress={() => navigation.goBack()} style={{ marginTop: 20 }} />
            </View>
        );
    }

    const currentExercise = exercises[currentExerciseIndex];
    const options = currentExercise.optionsJson ? JSON.parse(currentExercise.optionsJson) : [];

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <X color={COLORS.textSecondary} size={28} />
                </TouchableOpacity>
                <View style={styles.progressBack}>
                    <View style={[styles.progressBar, { width: `${((currentExerciseIndex + 1) / exercises.length) * 100}%` }]} />
                </View>
                <Text style={styles.statText}>{currentExerciseIndex + 1}/{exercises.length}</Text>
            </View>

            <View style={styles.content}>
                <Text style={styles.topicLabel}>REPASO: {currentExercise.lesson?.topic || 'General'}</Text>
                <Text style={[TYPOGRAPHY.h2, styles.question]}>{currentExercise.question}</Text>
                
                <View style={styles.optionsContainer}>
                    {options.map((option: string, index: number) => (
                        <TouchableOpacity
                            key={index}
                            style={[
                                styles.optionCard,
                                selectedOption === option && styles.selectedCard,
                                isChecked && option === currentExercise.correctAnswer && styles.correctCard,
                                isChecked && selectedOption === option && option !== currentExercise.correctAnswer && styles.wrongCard
                            ]}
                            onPress={() => !isChecked && setSelectedOption(option)}
                        >
                            <Text style={[
                                styles.optionText,
                                isChecked && (option === currentExercise.correctAnswer || (selectedOption === option && option !== currentExercise.correctAnswer)) && styles.whiteText
                            ]}>{option}</Text>
                        </TouchableOpacity>
                    ))}
                </View>
            </View>

            <View style={[
                styles.footer,
                isChecked && (isCorrect ? styles.footerCorrect : styles.footerWrong)
            ]}>
                {isChecked && (
                    <View style={styles.feedbackRow}>
                        {isCorrect ? <CheckCircle2 color={COLORS.success} size={32} /> : <AlertCircle color={COLORS.secondary} size={32} />}
                        <View style={styles.feedbackTextWrapper}>
                            <Text style={[styles.feedbackTitle, !isCorrect && { color: COLORS.secondary }]}>
                                {isCorrect ? t('lessons.goodJob') : t('lessons.almost')}
                            </Text>
                            <Text style={[styles.feedbackTranslation, !isCorrect && { color: COLORS.secondary }]}>
                                {isCorrect ? t('lessons.correctAnswer') : `${t('lessons.correctWas')}: ${currentExercise.correctAnswer}`}
                            </Text>
                        </View>
                    </View>
                )}

                <Button
                    title={isChecked ? t('common.continue') : t('lessons.check')}
                    onPress={isChecked ? handleContinue : handleCheck}
                    variant={selectedOption === null && !isChecked ? 'outline' : 'primary'}
                    style={styles.checkButton}
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
    loadingContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        padding: 20,
    },
    subtitle: {
        fontSize: 16,
        color: COLORS.textSecondary,
        textAlign: 'center',
        marginTop: 8,
    },
    header: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: SPACING.md,
        gap: SPACING.md,
    },
    progressBack: {
        flex: 1,
        height: 12,
        backgroundColor: '#E5E5E5',
        borderRadius: 6,
        overflow: 'hidden',
    },
    progressBar: {
        height: '100%',
        backgroundColor: COLORS.primary,
    },
    statText: {
        fontWeight: '900',
        color: COLORS.textSecondary,
    },
    content: {
        flex: 1,
        padding: SPACING.lg,
    },
    topicLabel: {
        fontSize: 12,
        fontWeight: '900',
        color: COLORS.primary,
        letterSpacing: 1.2,
        marginBottom: 8,
        textTransform: 'uppercase',
    },
    question: {
        marginBottom: SPACING.xl,
    },
    optionsContainer: {
        gap: SPACING.md,
    },
    optionCard: {
        padding: SPACING.lg,
        borderRadius: 16,
        borderWidth: 2,
        borderColor: '#E5E5E5',
        backgroundColor: COLORS.white,
        borderBottomWidth: 5,
        borderBottomColor: '#E5E5E5',
    },
    selectedCard: {
        borderColor: COLORS.primary,
        borderBottomColor: COLORS.primaryDark,
        backgroundColor: '#F7FFF7',
    },
    correctCard: {
        backgroundColor: COLORS.primary,
        borderColor: COLORS.primary,
        borderBottomColor: COLORS.primaryDark,
    },
    wrongCard: {
        backgroundColor: COLORS.secondary,
        borderColor: COLORS.secondary,
        borderBottomColor: '#CC4B4B',
    },
    optionText: {
        fontSize: 18,
        fontWeight: '700',
        color: COLORS.text,
    },
    whiteText: {
        color: COLORS.white,
    },
    footer: {
        padding: SPACING.lg,
        borderTopWidth: 2,
        borderTopColor: '#E5E5E5',
        backgroundColor: COLORS.white,
    },
    footerCorrect: {
        backgroundColor: '#F7FFF7',
        borderTopColor: '#A5D6A7',
    },
    footerWrong: {
        backgroundColor: '#FFF5F5',
        borderTopColor: '#EF9A9A',
    },
    feedbackRow: {
        flexDirection: 'row',
        alignItems: 'center',
        marginBottom: SPACING.md,
        gap: SPACING.md,
    },
    feedbackTextWrapper: {
        flex: 1,
    },
    feedbackTitle: {
        fontSize: 20,
        fontWeight: '900',
        color: COLORS.primary,
    },
    feedbackTranslation: {
        color: COLORS.text,
        fontWeight: '600',
    },
    checkButton: {
        width: '100%',
    },
});
