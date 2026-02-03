import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, SafeAreaView, ActivityIndicator } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { BookOpen, Trophy, MessageCircle, User as UserIcon } from 'lucide-react-native';
import { apiService } from '../services/apiService';

export const HomeScreen = ({ navigation }: any) => {
    const [lessons, setLessons] = useState<any[]>([]);
    const [progress, setProgress] = useState<any>(null);
    const [user, setUser] = useState<any>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            const [lessonsData, progressData, userData] = await Promise.all([
                apiService.getLessons(),
                apiService.getUserProgress(1), // Assuming user ID 1 for now
                apiService.getUser(1)
            ]);
            setLessons(lessonsData);
            setProgress(progressData);
            setUser(userData);
            setLoading(false);
        };
        fetchData();
    }, []);

    if (loading) {
        return (
            <View style={styles.loadingContainer}>
                <ActivityIndicator size="large" color={COLORS.primary} />
            </View>
        );
    }

    return (
        <SafeAreaView style={styles.container}>
            {/* Header */}
            <View style={styles.header}>
                <View style={styles.headerLeft}>
                    <Text style={styles.userName}>{user?.username || 'Usuario'}</Text>
                    <View style={styles.statsRow}>
                        <View style={styles.stat}>
                            <Text style={styles.statIcon}>🔥</Text>
                            <Text style={styles.statText}>{progress?.streak || 0}</Text>
                        </View>
                        <View style={styles.stat}>
                            <Text style={styles.statIcon}>💎</Text>
                            <Text style={styles.statText}>{progress?.txanponak || 0}</Text>
                        </View>
                        <View style={styles.stat}>
                            <Text style={styles.statIcon}>⭐</Text>
                            <Text style={styles.statText}>{progress?.xp || 0}</Text>
                        </View>
                    </View>
                </View>
                <TouchableOpacity
                    style={styles.profileIcon}
                    onPress={() => navigation.navigate('Profile')}
                >
                    <UserIcon color={COLORS.primary} size={28} />
                </TouchableOpacity>
            </View>

            <ScrollView contentContainerStyle={styles.scrollContent}>
                <Text style={[TYPOGRAPHY.h2, styles.sectionTitle]}>Unidad 1: Conceptos Básicos</Text>

                <View style={styles.pathContainer}>
                    {lessons.map((lesson: any, index: number) => (
                        <TouchableOpacity
                            key={lesson.id}
                            style={[
                                styles.lessonNode,
                                { marginLeft: index % 2 === 0 ? 0 : 60 }
                            ]}
                            onPress={() => navigation.navigate('Lesson', { lessonId: lesson.id })}
                        >
                            <View style={[
                                styles.nodeCircle,
                                styles.activeNode
                            ]}>
                                <BookOpen color={COLORS.white} size={28} />
                            </View>
                            <Text style={styles.lessonTitle}>{lesson.title}</Text>
                        </TouchableOpacity>
                    ))}
                </View>
            </ScrollView>

            {/* Bottom Tabs Placeholder */}
            <View style={styles.tabBar}>
                <TouchableOpacity style={styles.tabItem}><BookOpen color={COLORS.primary} /></TouchableOpacity>
                <TouchableOpacity style={styles.tabItem} onPress={() => navigation.navigate('AIChat')}><MessageCircle color={COLORS.textSecondary} /></TouchableOpacity>
                <TouchableOpacity style={styles.tabItem} onPress={() => navigation.navigate('Leaderboard')}><Trophy color={COLORS.textSecondary} /></TouchableOpacity>
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
    scrollContent: {
        padding: SPACING.lg,
        alignItems: 'center',
    },
    sectionTitle: {
        marginBottom: SPACING.xl,
        textAlign: 'center',
    },
    pathContainer: {
        width: '100%',
        alignItems: 'center',
    },
    lessonNode: {
        alignItems: 'center',
        marginBottom: SPACING.xl,
        width: 100,
    },
    nodeCircle: {
        width: 80,
        height: 80,
        borderRadius: 40,
        backgroundColor: '#DDD',
        alignItems: 'center',
        justifyContent: 'center',
        marginBottom: SPACING.xs,
    },
    activeNode: {
        backgroundColor: COLORS.primary,
        transform: [{ scale: 1.1 }],
        borderWidth: 4,
        borderColor: '#A5D6A7',
    },
    lessonTitle: {
        fontWeight: '600',
        color: COLORS.text,
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
    }
});
