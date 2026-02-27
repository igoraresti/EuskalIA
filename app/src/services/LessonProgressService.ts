import AsyncStorage from '@react-native-async-storage/async-storage';

const STORAGE_KEY = '@euskalia_lesson_progress';

export interface LessonProgress {
    levelId: string;
    exercises: any[];
    currentExerciseIndex: number;
    correctCount: number;
    errorsCount: number;
    startTime: number;
    savedAt: number;
}

export const LessonProgressService = {
    /** Save the current lesson state. Called on every exercise advance. */
    async save(progress: LessonProgress): Promise<void> {
        try {
            await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(progress));
        } catch (e) {
            console.warn('[LessonProgress] Could not save progress', e);
        }
    },

    /** Load saved progress. Returns null if nothing is stored. */
    async load(): Promise<LessonProgress | null> {
        try {
            const raw = await AsyncStorage.getItem(STORAGE_KEY);
            return raw ? (JSON.parse(raw) as LessonProgress) : null;
        } catch (e) {
            console.warn('[LessonProgress] Could not load progress', e);
            return null;
        }
    },

    /** Clear saved progress (called on lesson completion or explicit discard). */
    async clear(): Promise<void> {
        try {
            await AsyncStorage.removeItem(STORAGE_KEY);
        } catch (e) {
            console.warn('[LessonProgress] Could not clear progress', e);
        }
    },
};
