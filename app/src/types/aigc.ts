export interface AigcExercise {
    id: string;
    exerciseCode: string;
    templateType: string;
    levelId: string;
    topics: string;
    difficulty: number;
    status: string;
    jsonSchema: string; // Serialized JSON string
}

export interface BlockBuilderContent {
    promptLocal: string;
    validation: {
        correctSequence: string[];
        targetWord?: string;
        targetTranslation?: string;
        feedback?: {
            onSuccess?: string;
            onFail?: string;
        };
    };
    elements: {
        stem: {
            id: string;
            text: string;
            type: string;
            colorCode?: string;
        };
        pieces: {
            id: string;
            text: string;
            type: string;
            colorCode?: string;
        }[];
    };
}
