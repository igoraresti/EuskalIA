import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, Animated, ActivityIndicator } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { X, CheckCircle2, AlertCircle } from 'lucide-react-native';
import { Button } from '../components/Button';
import { apiService } from '../services/apiService';
import { useAuth } from '../context/AuthContext';
import { BlockBuilderExercise } from '../components/exercises/BlockBuilderExercise';

export const LessonScreen = ({ navigation, route }: any) => {
    const { t, i18n } = useTranslation();
    const { user } = useAuth();
    const { levelId, lessonId } = route.params;

    // We will hold a mix of legacy and AIGC exercises here
    const [exercises, setExercises] = useState<any[]>([]);
    const [currentExerciseIndex, setCurrentExerciseIndex] = useState(0);

    // Legacy MCQ State
    const [selectedOption, setSelectedOption] = useState<string | null>(null);

    // AIGC State
    const [aigcIsCorrect, setAigcIsCorrect] = useState<boolean | null>(null);
    const [shouldCheckAigc, setShouldCheckAigc] = useState(false);

    // Global Interaction State
    const [isChecked, setIsChecked] = useState(false);
    const [loading, setLoading] = useState(true);
    const [isCorrect, setIsCorrect] = useState(false);

    useEffect(() => {
        const fetchExercises = async () => {
            setLoading(true);
            try {
                if (levelId && user) {
                    // New Route: Intelligent SRS Session
                    const sessionData = await apiService.getSessionExercises(levelId, user.id);
                    if (sessionData && sessionData.length > 0) {
                        const parsedSession = sessionData.map((ex: any) => {
                            if (ex.templateType === 'block_builder') {
                                return {
                                    id: ex.id,
                                    isAigc: true,
                                    templateType: ex.templateType,
                                    content: JSON.parse(ex.jsonSchema),
                                    question: "Block Builder Exercise"
                                };
                            } else {
                                // Assume multiple_choice
                                const schema = JSON.parse(ex.jsonSchema);
                                const userLang = i18n.language || 'es';
                                return {
                                    id: ex.id,
                                    isAigc: true,
                                    templateType: ex.templateType,
                                    question: schema.question[userLang] || schema.question['es'] || schema.question,
                                    optionsJson: JSON.stringify(schema.options),
                                    correctAnswer: schema.correctAnswer
                                };
                            }
                        });
                        setExercises(parsedSession);
                    }
                } else if (lessonId) {
                    // Legacy Lesson Route
                    const lessonData = await apiService.getLesson(lessonId);
                    let loadedExercises = lessonData?.exercises || [];

                    const fallbackLevelId = lessonData?.level ? `A1_UNIT_${lessonData.level}` : 'A1';
                    const aigcExercises = await apiService.getAigcExercises(fallbackLevelId);

                    if (aigcExercises && aigcExercises.length > 0) {
                        const randomAigc = aigcExercises[Math.floor(Math.random() * aigcExercises.length)];
                        loadedExercises.push({
                            id: randomAigc.id,
                            isAigc: true,
                            templateType: randomAigc.templateType,
                            content: JSON.parse(randomAigc.jsonSchema),
                            question: "AIGC Exercise"
                        });
                    }
                    setExercises(loadedExercises);
                }
            } catch (e) {
                console.error("Failed loading exercises", e);
            } finally {
                setLoading(false);
            }
        };
        fetchExercises();
    }, [levelId, lessonId, user]);

    const handleCheckLegacy = async () => {
        if (selectedOption !== null && exercises.length > 0) {
            const currentExercise = exercises[currentExerciseIndex];
            const correct = selectedOption === currentExercise.correctAnswer;
            setIsCorrect(correct);
            setIsChecked(true);

            if (currentExercise.isAigc && user) {
                await apiService.submitExerciseAttempt(user.id, currentExercise.id, correct);
            }
        }
    };

    const handleCheckAigc = () => {
        setShouldCheckAigc(true); // Triggers the child component to call onResult
    };

    const handleAigcResult = async (correct: boolean) => {
        setIsCorrect(correct);
        setIsChecked(true);
        setShouldCheckAigc(false);
        const currentExercise = exercises[currentExerciseIndex];
        if (user) {
            await apiService.submitExerciseAttempt(user.id, currentExercise.id, correct);
        }
    };

    const handleCheck = () => {
        const currentExercise = exercises[currentExerciseIndex];
        if (currentExercise.isAigc && currentExercise.templateType === 'block_builder') {
            handleCheckAigc();
        } else {
            handleCheckLegacy();
        }
    };

    const handleContinue = async () => {
        if (currentExerciseIndex < exercises.length - 1) {
            setCurrentExerciseIndex(currentExerciseIndex + 1);
            // Reset state
            setSelectedOption(null);
            setAigcIsCorrect(null);
            setShouldCheckAigc(false);
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

    if (exercises.length === 0) {
        return (
            <View style={styles.loadingContainer}>
                <Text>{t('lesson.noExercises')}</Text>
                <Button title={t('common.back')} onPress={() => navigation.goBack()} style={{ marginTop: 20 }} />
            </View>
        );
    }

    const currentExercise = exercises[currentExerciseIndex];

    const renderExerciseContent = () => {
        if (currentExercise.isAigc && currentExercise.templateType === 'block_builder') {
            return (
                <BlockBuilderExercise
                    key={currentExercise.id}
                    data={currentExercise.content}
                    onResult={handleAigcResult}
                    checkTriggered={shouldCheckAigc}
                />
            );
        }

        // Legacy Multiple Choice Fallback
        const options = currentExercise.optionsJson ? JSON.parse(currentExercise.optionsJson) : [];
        return (
            <>
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
            </>
        );
    };

    const isCheckDisabled = (currentExercise.isAigc && currentExercise.templateType === 'block_builder') ? false : selectedOption === null;

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <X color={COLORS.textSecondary} size={28} />
                </TouchableOpacity>
                <View style={styles.progressBack}>
                    <View style={[styles.progressBar, { width: `${((currentExerciseIndex + 1) / exercises.length) * 100}%` }]} />
                </View>
                <Text style={styles.heartText}>❤️ 5</Text>
            </View>

            <View style={styles.content}>
                {renderExerciseContent()}
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
                                {isCorrect ? t('lessons.goodJob') : t('lessons.almost')}
                            </Text>
                            <Text style={[styles.feedbackTranslation, !isCorrect && { color: COLORS.secondary }]}>
                                {isCorrect ? t('lessons.correctAnswer') : (currentExercise.isAigc ? t('lessons.retry') : `${t('lessons.correctWas')}: ${currentExercise.correctAnswer}`)}
                            </Text>
                        </View>
                    </View>
                ) : null}

                <Button
                    title={isChecked ? t('common.continue') : t('lessons.check')}
                    onPress={isChecked ? handleContinue : handleCheck}
                    variant={isCheckDisabled ? 'outline' : 'primary'}
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
