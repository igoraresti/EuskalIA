// Admin Management Dashboard Screen
import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, ScrollView, TextInput, ActivityIndicator, Alert, Platform } from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import {
    ChevronLeft,
    Users,
    UserCheck,
    UserX,
    Search,
    Filter,
    ChevronRight,
    Shield,
    RefreshCw,
    Calendar
} from 'lucide-react-native';
import { apiService } from '../services/apiService';

export const AdminDashboardScreen = ({ navigation }: any) => {
    const { t } = useTranslation();
    const [stats, setStats] = useState<any>(null);
    const [usersData, setUsersData] = useState<any>({ items: [], totalCount: 0, page: 1, totalPages: 0 });
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);

    // Filters
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState<boolean | null>(null); // null = all, true = active, false = inactive
    const [page, setPage] = useState(1);
    const [showFilters, setShowFilters] = useState(false);

    const fetchData = useCallback(async (isRefresh = false) => {
        if (isRefresh) setRefreshing(true);
        else setLoading(true);

        try {
            const [statsData, usersList] = await Promise.all([
                apiService.getAdminStats(),
                apiService.getAdminUsers({
                    page,
                    search: search || undefined,
                    isActive: statusFilter === null ? undefined : statusFilter
                })
            ]);

            if (statsData) setStats(statsData);
            if (usersList) setUsersData(usersList);
        } catch (error) {
            console.error('Error fetching admin data:', error);
        } finally {
            setLoading(false);
            setRefreshing(false);
        }
    }, [page, search, statusFilter]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleToggleActive = async (userId: number, currentStatus: boolean) => {
        const result = await apiService.toggleUserActive(userId);
        if (result) {
            // Update local state instead of refetching everything
            setUsersData((prev: any) => ({
                ...prev,
                items: prev.items.map((u: any) =>
                    u.id === userId ? { ...u, isActive: !currentStatus } : u
                )
            }));

            // Update stats
            setStats((prev: any) => ({
                ...prev,
                activeUsers: currentStatus ? prev.activeUsers - 1 : prev.activeUsers + 1
            }));
        }
    };

    const renderStatCard = (title: string, value: number, icon: any, color: string) => (
        <View style={styles.statCard}>
            <View style={[styles.statIconContainer, { backgroundColor: color + '20' }]}>
                {icon}
            </View>
            <View>
                <Text style={styles.statValue}>{value}</Text>
                <Text style={styles.statTitle}>{title}</Text>
            </View>
        </View>
    );

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backButton}>
                    <ChevronLeft color={COLORS.text} size={28} />
                </TouchableOpacity>
                <View style={styles.headerTitleContainer}>
                    <Shield color={COLORS.primary} size={20} style={{ marginRight: 8 }} />
                    <Text style={TYPOGRAPHY.h2}>Panel Admin</Text>
                </View>
                <TouchableOpacity onPress={() => fetchData(true)} disabled={refreshing}>
                    <RefreshCw color={COLORS.primary} size={24} style={refreshing && { transform: [{ rotate: '45deg' }] }} />
                </TouchableOpacity>
            </View>

            <ScrollView contentContainerStyle={styles.scrollContent}>
                {/* Stats Row */}
                <View style={styles.statsRow}>
                    {renderStatCard("Total", stats?.totalUsers || 0, <Users color="#2196F3" size={24} />, "#2196F3")}
                    {renderStatCard("Activos", stats?.activeUsers || 0, <UserCheck color="#4CAF50" size={24} />, "#4CAF50")}
                    {renderStatCard("Hoy", stats?.registrationsToday || 0, <Calendar color="#FF9800" size={24} />, "#FF9800")}
                </View>

                {/* Filters Section */}
                <View style={styles.filterContainer}>
                    <View style={styles.searchBar}>
                        <Search color="#999" size={20} style={{ marginRight: 8 }} />
                        <TextInput
                            style={styles.searchInput}
                            placeholder="Buscar por nombre..."
                            value={search}
                            onChangeText={(text) => {
                                setSearch(text);
                                setPage(1); // Reset to first page on search
                            }}
                        />
                        <TouchableOpacity onPress={() => setShowFilters(!showFilters)}>
                            <Filter color={statusFilter !== null ? COLORS.primary : "#999"} size={20} />
                        </TouchableOpacity>
                    </View>

                    {showFilters && (
                        <View style={styles.filterOptions}>
                            <Text style={styles.filterLabel}>Estado:</Text>
                            <View style={styles.filterChips}>
                                <TouchableOpacity
                                    style={[styles.filterChip, statusFilter === null && styles.activeChip]}
                                    onPress={() => { setStatusFilter(null); setPage(1); }}
                                >
                                    <Text style={[styles.chipText, statusFilter === null && styles.activeChipText]}>Todos</Text>
                                </TouchableOpacity>
                                <TouchableOpacity
                                    style={[styles.filterChip, statusFilter === true && styles.activeChip]}
                                    onPress={() => { setStatusFilter(true); setPage(1); }}
                                >
                                    <Text style={[styles.chipText, statusFilter === true && styles.activeChipText]}>Activos</Text>
                                </TouchableOpacity>
                                <TouchableOpacity
                                    style={[styles.filterChip, statusFilter === false && styles.activeChip]}
                                    onPress={() => { setStatusFilter(false); setPage(1); }}
                                >
                                    <Text style={[styles.chipText, statusFilter === false && styles.activeChipText]}>Inactivos</Text>
                                </TouchableOpacity>
                            </View>
                        </View>
                    )}
                </View>

                {/* Users List / Table */}
                <View style={styles.tableCard}>
                    <View style={styles.tableHeader}>
                        <Text style={styles.tableHeaderTitle}>Usuarios ({usersData.totalCount})</Text>
                    </View>

                    {loading ? (
                        <ActivityIndicator style={{ margin: 40 }} color={COLORS.primary} size="large" />
                    ) : usersData.items.length === 0 ? (
                        <View style={styles.emptyContainer}>
                            <Text style={styles.emptyText}>No se encontraron usuarios</Text>
                        </View>
                    ) : (
                        <View>
                            {usersData.items.map((item: any) => (
                                <View key={item.id} style={styles.userRow}>
                                    <View style={styles.userInfo}>
                                        <Text style={styles.username}>{item.username}</Text>
                                        <Text style={styles.email}>{item.email}</Text>
                                        <View style={styles.badgeRow}>
                                            <View style={[styles.roleBadge, { backgroundColor: item.role === 'Admin' ? COLORS.primary + '20' : '#EEE' }]}>
                                                <Text style={[styles.roleText, { color: item.role === 'Admin' ? COLORS.primary : '#666' }]}>{item.role}</Text>
                                            </View>
                                            {item.isVerified && (
                                                <View style={styles.verifiedBadge}>
                                                    <Text style={styles.verifiedText}>Verificado</Text>
                                                </View>
                                            )}
                                        </View>
                                    </View>
                                    <View style={styles.actions}>
                                        <Text style={styles.xpMini}>{item.xp} XP</Text>
                                        <TouchableOpacity
                                            onPress={() => handleToggleActive(item.id, item.isActive)}
                                            style={[styles.toggleBtn, { backgroundColor: item.isActive ? '#E8F5E9' : '#FFEBEE' }]}
                                        >
                                            {item.isActive ? (
                                                <UserCheck color="#4CAF50" size={18} />
                                            ) : (
                                                <UserX color="#F44336" size={18} />
                                            )}
                                        </TouchableOpacity>
                                    </View>
                                </View>
                            ))}
                        </View>
                    )}

                    {/* Pagination */}
                    {usersData.totalPages > 1 && (
                        <View style={styles.pagination}>
                            <TouchableOpacity
                                disabled={page === 1}
                                onPress={() => setPage(page - 1)}
                                style={[styles.pageBtn, page === 1 && styles.disabledBtn]}
                            >
                                <ChevronLeft color={page === 1 ? "#CCC" : COLORS.primary} size={24} />
                            </TouchableOpacity>
                            <Text style={styles.pageIndicator}>Página {page} de {usersData.totalPages}</Text>
                            <TouchableOpacity
                                disabled={page === usersData.totalPages}
                                onPress={() => setPage(page + 1)}
                                style={[styles.pageBtn, page === usersData.totalPages && styles.disabledBtn]}
                            >
                                <ChevronRight color={page === usersData.totalPages ? "#CCC" : COLORS.primary} size={24} />
                            </TouchableOpacity>
                        </View>
                    )}
                </View>
            </ScrollView>
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
        backgroundColor: COLORS.white,
        borderBottomWidth: 1,
        borderBottomColor: '#EEE',
    },
    backButton: {
        padding: 4,
    },
    headerTitleContainer: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    scrollContent: {
        padding: SPACING.md,
    },
    statsRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: SPACING.lg,
        gap: SPACING.sm,
    },
    statCard: {
        flex: 1,
        backgroundColor: COLORS.white,
        borderRadius: 16,
        padding: SPACING.sm,
        flexDirection: 'row',
        alignItems: 'center',
        borderWidth: 1,
        borderColor: '#EEE',
        elevation: 2,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
    },
    statIconContainer: {
        width: 40,
        height: 40,
        borderRadius: 20,
        alignItems: 'center',
        justifyContent: 'center',
        marginRight: 8,
    },
    statValue: {
        fontSize: 16,
        fontWeight: 'bold',
        color: COLORS.text,
    },
    statTitle: {
        fontSize: 10,
        color: COLORS.textSecondary,
    },
    filterContainer: {
        backgroundColor: COLORS.white,
        borderRadius: 16,
        padding: SPACING.md,
        marginBottom: SPACING.lg,
        borderWidth: 1,
        borderColor: '#EEE',
    },
    searchBar: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: '#F5F5F5',
        borderRadius: 12,
        paddingHorizontal: SPACING.md,
        height: 44,
    },
    searchInput: {
        flex: 1,
        fontSize: 14,
        color: COLORS.text,
    },
    filterOptions: {
        marginTop: SPACING.md,
        borderTopWidth: 1,
        borderTopColor: '#EEE',
        paddingTop: SPACING.md,
    },
    filterLabel: {
        fontSize: 12,
        fontWeight: 'bold',
        color: COLORS.textSecondary,
        marginBottom: 8,
    },
    filterChips: {
        flexDirection: 'row',
        gap: 8,
    },
    filterChip: {
        paddingHorizontal: 16,
        paddingVertical: 6,
        borderRadius: 20,
        backgroundColor: '#F0F0F0',
    },
    activeChip: {
        backgroundColor: COLORS.primary,
    },
    chipText: {
        fontSize: 12,
        color: '#666',
    },
    activeChipText: {
        color: COLORS.white,
        fontWeight: 'bold',
    },
    tableCard: {
        backgroundColor: COLORS.white,
        borderRadius: 16,
        overflow: 'hidden',
        borderWidth: 1,
        borderColor: '#EEE',
        marginBottom: SPACING.xl,
    },
    tableHeader: {
        padding: SPACING.md,
        backgroundColor: '#F8F9FA',
        borderBottomWidth: 1,
        borderBottomColor: '#EEE',
    },
    tableHeaderTitle: {
        fontWeight: 'bold',
        color: COLORS.text,
    },
    userRow: {
        flexDirection: 'row',
        padding: SPACING.md,
        borderBottomWidth: 1,
        borderBottomColor: '#F0F0F0',
        alignItems: 'center',
    },
    userInfo: {
        flex: 1,
    },
    username: {
        fontWeight: 'bold',
        fontSize: 15,
        color: COLORS.text,
    },
    email: {
        fontSize: 12,
        color: COLORS.textSecondary,
        marginBottom: 4,
    },
    badgeRow: {
        flexDirection: 'row',
        gap: 6,
    },
    roleBadge: {
        paddingHorizontal: 8,
        paddingVertical: 2,
        borderRadius: 4,
    },
    roleText: {
        fontSize: 10,
        fontWeight: 'bold',
    },
    verifiedBadge: {
        paddingHorizontal: 8,
        paddingVertical: 2,
        borderRadius: 4,
        backgroundColor: '#E3F2FD',
    },
    verifiedText: {
        fontSize: 10,
        color: '#2196F3',
        fontWeight: 'bold',
    },
    actions: {
        alignItems: 'center',
        gap: 8,
    },
    xpMini: {
        fontSize: 10,
        fontWeight: 'bold',
        color: COLORS.primary,
    },
    toggleBtn: {
        padding: 8,
        borderRadius: 8,
    },
    emptyContainer: {
        padding: 40,
        alignItems: 'center',
    },
    emptyText: {
        color: '#999',
    },
    pagination: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: SPACING.md,
        backgroundColor: '#F8F9FA',
    },
    pageBtn: {
        padding: 4,
    },
    disabledBtn: {
        opacity: 0.3,
    },
    pageIndicator: {
        marginHorizontal: 20,
        fontSize: 13,
        color: COLORS.textSecondary,
    }
});
