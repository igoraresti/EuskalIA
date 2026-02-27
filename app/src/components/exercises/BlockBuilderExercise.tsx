import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { View, Text, StyleSheet, TouchableOpacity, Animated } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../../theme';
import { BlockBuilderContent } from '../../types/aigc';

interface BlockBuilderExerciseProps {
    data: BlockBuilderContent;
    onResult: (isCorrect: boolean) => void;
    checkTriggered: boolean; // Flag to know when the user clicked 'Check' in LessonScreen
}

export const BlockBuilderExercise: React.FC<BlockBuilderExerciseProps> = ({ data, onResult, checkTriggered }) => {
    const { i18n } = useTranslation();
    const userLang = i18n.language || 'es';
    // Piece = { id, text, type, colorCode? }
    const [bankPieces, setBankPieces] = useState([...data.elements.pieces].sort(() => Math.random() - 0.5)); // Shuffle bank
    const [constructedPieces, setConstructedPieces] = useState<any[]>([]);

    useEffect(() => {
        if (checkTriggered) {
            // Compare constructedSequence vs correctSequence
            const currentSequence = [data.elements.stem.id, ...constructedPieces.map(p => p.id)];
            const isCorrect = JSON.stringify(currentSequence) === JSON.stringify(data.validation.correctSequence);
            onResult(isCorrect);
        }
    }, [checkTriggered]);

    const handleTapBankPiece = (piece: any) => {
        // Move from Bank to Constructed
        setBankPieces(prev => prev.filter(p => p.id !== piece.id));
        setConstructedPieces(prev => [...prev, piece]);
    };

    const handleTapConstructedPiece = (piece: any) => {
        // Move from Constructed back to Bank
        setConstructedPieces(prev => prev.filter(p => p.id !== piece.id));
        setBankPieces(prev => [...prev, piece]);
    };

    return (
        <View style={styles.container}>
            <Text style={[TYPOGRAPHY.h2, styles.prompt]}>{data.promptLocal[userLang] || data.promptLocal['es'] || data.promptLocal}</Text>

            <View style={styles.constructionArea}>
                <View style={styles.constructionBoard}>
                    {/* STEM (Fixed) */}
                    <View style={[styles.piece, { backgroundColor: data.elements.stem.colorCode || COLORS.primary }]}>
                        <Text style={styles.pieceText}>{data.elements.stem.text}</Text>
                    </View>

                    {/* Constructed Pieces (Tappable) */}
                    {constructedPieces.map((p, index) => (
                        <TouchableOpacity
                            key={`cons-${p.id}`}
                            onPress={() => !checkTriggered && handleTapConstructedPiece(p)}
                            style={[
                                styles.piece,
                                styles.suffixPiece,
                                p.colorCode && { backgroundColor: p.colorCode }
                            ]}
                        >
                            <Text style={styles.pieceText}>{p.text}</Text>
                        </TouchableOpacity>
                    ))}

                    {/* Empty Dropzone Marker */}
                    {constructedPieces.length < data.elements.pieces.length && !checkTriggered && (
                        <View style={styles.emptySlot} />
                    )}
                </View>
            </View>

            {/* Separator */}
            <View style={styles.separator} />

            <View style={styles.bankArea}>
                {bankPieces.map((p) => (
                    <TouchableOpacity
                        key={`bank-${p.id}`}
                        onPress={() => !checkTriggered && handleTapBankPiece(p)}
                        style={[
                            styles.piece,
                            styles.bankPiece,
                            p.colorCode && { backgroundColor: p.colorCode }
                        ]}
                    >
                        <Text style={styles.pieceText}>{p.text}</Text>
                    </TouchableOpacity>
                ))}
            </View>

            {checkTriggered && data.validation.feedback && (
                <View style={styles.feedbackContainer}>
                    <Text style={styles.feedbackText}>
                        {JSON.stringify([data.elements.stem.id, ...constructedPieces.map(p => p.id)]) === JSON.stringify(data.validation.correctSequence)
                            ? (data.validation.feedback.onSuccess[userLang] || data.validation.feedback.onSuccess['es'] || data.validation.feedback.onSuccess)
                            : (data.validation.feedback.onFail[userLang] || data.validation.feedback.onFail['es'] || data.validation.feedback.onFail)}
                    </Text>
                </View>
            )}
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
    prompt: {
        marginBottom: SPACING.xl,
        textAlign: 'center'
    },
    constructionArea: {
        minHeight: 120,
        backgroundColor: COLORS.white,
        borderRadius: 16,
        padding: SPACING.lg,
        borderWidth: 2,
        borderColor: '#E2E8F0',
        justifyContent: 'center',
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.05,
        shadowRadius: 5,
        elevation: 2,
    },
    constructionBoard: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 4, // Tight gap to simulate agglutination
    },
    separator: {
        height: 1,
        backgroundColor: '#E2E8F0',
        marginVertical: SPACING.xl,
    },
    bankArea: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        gap: SPACING.md,
        justifyContent: 'center',
    },
    piece: {
        paddingHorizontal: SPACING.md,
        paddingVertical: 12,
        borderRadius: 12,
        backgroundColor: COLORS.secondary,
        minWidth: 50,
        alignItems: 'center',
        justifyContent: 'center',
    },
    pieceText: {
        color: COLORS.white,
        fontSize: 20,
        fontWeight: '700',
    },
    suffixPiece: {
        borderTopLeftRadius: 4, // Flatten left side to "connect" with previous block visually
        borderBottomLeftRadius: 4,
    },
    bankPiece: {
        borderWidth: 2,
        borderColor: 'rgba(0,0,0,0.1)'
    },
    emptySlot: {
        width: 40,
        height: 48,
        borderWidth: 2,
        borderColor: '#CBD5E1',
        borderStyle: 'dashed',
        borderRadius: 8,
        marginLeft: 4
    },
    feedbackContainer: {
        marginTop: SPACING.lg,
        padding: SPACING.md,
        backgroundColor: '#F8FAFC',
        borderRadius: 8,
    },
    feedbackText: {
        fontSize: 16,
        color: COLORS.textSecondary,
        fontStyle: 'italic',
        textAlign: 'center'
    }
});
