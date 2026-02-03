import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, FlatList, ActivityIndicator } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Trophy, ChevronLeft, Award } from 'lucide-react-native';
import { apiService } from '../services/apiService';

const PERIODS = [
    { id: 'week', label: 'Semanal' },
    { id: 'month', label: 'Mensual' },
    { id: 'all', label: 'Histórico' }
];

export const LeaderboardScreen = ({ navigation }: any) => {
    const [period, setPeriod] = useState('week');
    const [viewMode, setViewMode] = useState('world'); // 'world' or 'me'
    const [data, setData] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const currentUserId = 1; // Assuming user ID 1 for now

    const fetchData = async () => {
        setLoading(true);
        let rankingData = [];
        if (viewMode === 'world') {
            rankingData = await apiService.getWorldLeaderboard(period);
        } else {
            rankingData = await apiService.getUserLeaderboard(currentUserId, period);
        }
        setData(rankingData);
        setLoading(false);
    };

    useEffect(() => {
        fetchData();
    }, [period, viewMode]);

    const renderItem = ({ item, index }: { item: any; index: number }) => {
        const rank = viewMode === 'world' ? index + 1 : item.rank;
        const isMe = item.userId === currentUserId;

        return (
            <View style={[styles.rankingItem, isMe && styles.meItem]}>
                <View style={styles.rankContainer}>
                    {rank === 1 && <Text style={styles.medal}>🥇</Text>}
                    {rank === 2 && <Text style={styles.medal}>🥈</Text>}
                    {rank === 3 && <Text style={styles.medal}>🥉</Text>}
                    {rank > 3 && <Text style={styles.rankText}>{rank}</Text>}
                </View>

                <View style={styles.userInfo}>
                    <Text style={[styles.username, isMe && styles.meText]}>{item.username}</Text>
                    <Text style={styles.levelText}>Nivel {item.level}</Text>
                </View>

                <View style={styles.xpContainer}>
                    <Text style={styles.xpText}>{item.xp} XP</Text>
                </View>
            </View>
        );
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <ChevronLeft color={COLORS.text} size={28} />
                </TouchableOpacity>
                <Text style={TYPOGRAPHY.h2}>Clasificación</Text>
                <View style={{ width: 28 }} />
            </View>

            <View style={styles.viewSelector}>
                <TouchableOpacity
                    style={[styles.viewBtn, viewMode === 'me' && styles.activeViewBtn]}
                    onPress={() => setViewMode('me')}
                >
                    <Text style={[styles.viewBtnText, viewMode === 'me' && styles.activeViewBtnText]}>Mi clasificación</Text>
                </TouchableOpacity>
                <TouchableOpacity
                    style={[styles.viewBtn, viewMode === 'world' && styles.activeViewBtn]}
                    onPress={() => setViewMode('world')}
                >
                    <Text style={[styles.viewBtnText, viewMode === 'world' && styles.activeViewBtnText]}>Clasificación mundial</Text>
                </TouchableOpacity>
            </View>

            <View style={styles.periodSelector}>
                {PERIODS.map(p => (
                    <TouchableOpacity
                        key={p.id}
                        style={[styles.periodBtn, period === p.id && styles.activePeriodBtn]}
                        onPress={() => setPeriod(p.id)}
                    >
                        <Text style={[styles.periodBtnText, period === p.id && styles.activePeriodBtnText]}>{p.label}</Text>
                    </TouchableOpacity>
                ))}
            </View>

            {loading ? (
                <View style={styles.loader}>
                    <ActivityIndicator size="large" color={COLORS.primary} />
                </View>
            ) : (
                <FlatList
                    data={data}
                    renderItem={renderItem}
                    keyExtractor={(item) => item.userId.toString()}
                    contentContainerStyle={styles.listContent}
                    ListEmptyComponent={
                        <View style={styles.emptyState}>
                            <Award size={48} color="#CCC" />
                            <Text style={styles.emptyText}>No hay datos disponibles</Text>
                        </View>
                    }
                />
            )}
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
    },
    header: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: SPACING.md,
    },
    viewSelector: {
        flexDirection: 'row',
        backgroundColor: '#F0F0F0',
        margin: SPACING.md,
        borderRadius: 12,
        padding: 4,
    },
    viewBtn: {
        flex: 1,
        paddingVertical: 10,
        alignItems: 'center',
        borderRadius: 8,
    },
    activeViewBtn: {
        backgroundColor: COLORS.white,
        elevation: 2,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
    },
    viewBtnText: {
        fontWeight: '600',
        color: COLORS.textSecondary,
    },
    activeViewBtnText: {
        color: COLORS.primary,
    },
    periodSelector: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        paddingHorizontal: SPACING.md,
        marginBottom: SPACING.md,
    },
    periodBtn: {
        paddingVertical: 8,
        paddingHorizontal: 16,
        borderRadius: 20,
    },
    activePeriodBtn: {
        backgroundColor: COLORS.primary,
    },
    periodBtnText: {
        fontWeight: 'bold',
        color: COLORS.textSecondary,
    },
    activePeriodBtnText: {
        color: COLORS.white,
    },
    listContent: {
        padding: SPACING.md,
    },
    rankingItem: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: SPACING.md,
        backgroundColor: COLORS.white,
        borderRadius: 16,
        marginBottom: SPACING.sm,
        borderWidth: 1,
        borderColor: '#EEE',
    },
    meItem: {
        borderColor: COLORS.primary,
        backgroundColor: '#E8F5E9',
    },
    rankContainer: {
        width: 40,
        alignItems: 'center',
    },
    medal: {
        fontSize: 24,
    },
    rankText: {
        fontWeight: 'bold',
        color: COLORS.textSecondary,
        fontSize: 16,
    },
    userInfo: {
        flex: 1,
        marginLeft: SPACING.sm,
    },
    username: {
        fontWeight: 'bold',
        fontSize: 16,
        color: COLORS.text,
    },
    meText: {
        color: COLORS.primary,
    },
    levelText: {
        fontSize: 12,
        color: COLORS.textSecondary,
    },
    xpContainer: {
        alignItems: 'flex-end',
    },
    xpText: {
        fontWeight: 'bold',
        color: COLORS.primary,
    },
    loader: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    emptyState: {
        alignItems: 'center',
        marginTop: 40,
    },
    emptyText: {
        marginTop: 10,
        color: '#999',
    }
});
