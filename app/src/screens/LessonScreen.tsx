import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, Animated, ActivityIndicator } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { X, CheckCircle2, AlertCircle } from 'lucide-react-native';
import { Button } from '../components/Button';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';

export const LessonScreen = ({ navigation, route }: any) => {
    const { t } = useTranslation();
    const { user } = useAuth();
    const { lessonId } = route.params;
    const [lesson, setLesson] = useState<any>(null);
    const [currentExerciseIndex, setCurrentExerciseIndex] = useState(0);
    const [selectedOption, setSelectedOption] = useState<string | null>(null);
    const [isChecked, setIsChecked] = useState(false);
    const [loading, setLoading] = useState(true);
    const [isCorrect, setIsCorrect] = useState(false);

    useEffect(() => {
        const fetchLesson = async () => {
            setLoading(true);
            const data = await apiService.getLesson(lessonId);
            setLesson(data);
            setLoading(false);
        };
        fetchLesson();
    }, [lessonId]);

    const handleCheck = () => {
        if (selectedOption !== null && lesson) {
            const currentExercise = lesson.exercises[currentExerciseIndex];
            const correct = selectedOption === currentExercise.correctAnswer;
            setIsCorrect(correct);
            setIsChecked(true);
        }
    };

    const handleContinue = async () => {
        if (currentExerciseIndex < lesson.exercises.length - 1) {
            setCurrentExerciseIndex(currentExerciseIndex + 1);
            setSelectedOption(null);
            setIsChecked(false);
        } else {
            // Lesson completed! Add XP
            if (user) {
                await apiService.addXP(user.id, 10);
            }
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

    if (!lesson || lesson.exercises.length === 0) {
        return (
            <View style={styles.loadingContainer}>
                <Text>{t('lesson.noExercises')}</Text>
                <Button title={t('common.back')} onPress={() => navigation.goBack()} style={{ marginTop: 20 }} />
            </View>
        );
    }

    const currentExercise = lesson.exercises[currentExerciseIndex];
    const options = JSON.parse(currentExercise.optionsJson);

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <X color={COLORS.textSecondary} size={28} />
                </TouchableOpacity>
                <View style={styles.progressBack}>
                    <View style={[styles.progressBar, { width: `${((currentExerciseIndex + 1) / lesson.exercises.length) * 100}%` }]} />
                </View>
                <Text style={styles.heartText}>❤️ 5</Text>
            </View>

            <View style={styles.content}>
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
                {isChecked ? (
                    <View style={styles.feedbackRow}>
                        {isCorrect ? <CheckCircle2 color={COLORS.success} size={32} /> : <AlertCircle color={COLORS.secondary} size={32} />}
                        <View style={styles.feedbackTextWrapper}>
                            <Text style={[styles.feedbackTitle, !isCorrect && { color: COLORS.secondary }]}>
                                {isCorrect ? t('lesson.goodJob') : t('lesson.almost')}
                            </Text>
                            <Text style={[styles.feedbackTranslation, !isCorrect && { color: COLORS.secondary }]}>
                                {isCorrect ? t('lesson.correctAnswer') : `${t('lesson.correctWas')}: ${currentExercise.correctAnswer}`}
                            </Text>
                        </View>
                    </View>
                ) : null}

                <Button
                    title={isChecked ? t('common.continue') : t('lesson.check')}
                    onPress={isChecked ? handleContinue : handleCheck}
                    variant={selectedOption === null ? 'outline' : 'primary'}
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
    header: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: SPACING.md,
        gap: SPACING.md,
    },
    progressBack: {
        flex: 1,
        height: 12,
        backgroundColor: '#EEE',
        borderRadius: 6,
        overflow: 'hidden',
    },
    progressBar: {
        height: '100%',
        backgroundColor: COLORS.primary,
    },
    heartText: {
        fontWeight: 'bold',
        color: COLORS.secondary,
    },
    content: {
        flex: 1,
        padding: SPACING.lg,
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
        borderColor: '#EEE',
        backgroundColor: COLORS.white,
    },
    selectedCard: {
        borderColor: COLORS.primary,
        backgroundColor: '#E8F5E9',
    },
    correctCard: {
        backgroundColor: COLORS.primary,
        borderColor: COLORS.primary,
    },
    wrongCard: {
        backgroundColor: COLORS.secondary,
        borderColor: COLORS.secondary,
    },
    optionText: {
        fontSize: 18,
        fontWeight: '600',
    },
    whiteText: {
        color: COLORS.white,
    },
    footer: {
        padding: SPACING.lg,
        borderTopWidth: 1,
        borderTopColor: '#EEE',
    },
    footerCorrect: {
        backgroundColor: '#E8F5E9',
        borderTopColor: '#C8E6C9',
    },
    footerWrong: {
        backgroundColor: '#FFEBEE',
        borderTopColor: '#FFCDD2',
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
        fontWeight: 'bold',
        color: COLORS.success,
    },
    feedbackTranslation: {
        color: COLORS.success,
    },
    checkButton: {
        width: '100%',
    }
});
