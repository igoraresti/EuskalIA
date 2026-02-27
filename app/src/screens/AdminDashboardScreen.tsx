// Admin Management Dashboard Screen
import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, SafeAreaView, TouchableOpacity, ScrollView, TextInput, ActivityIndicator, Alert, Platform, Modal } from 'react-native';
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
    Calendar,
    Menu,
    BookOpen,
    Upload,
    X,
    Filter as FilterIcon
} from 'lucide-react-native';
import { apiService } from '../services/apiService';

export const AdminDashboardScreen = ({ navigation }: any) => {
    const { t } = useTranslation();
    const [stats, setStats] = useState<any>(null);
    const [usersData, setUsersData] = useState<any>({ items: [], totalCount: 0, page: 1, totalPages: 0 });
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [drawerOpen, setDrawerOpen] = useState(false);

    // Filters
    const [search, setSearch] = useState('');
    const [isActiveFilter, setIsActiveFilter] = useState<boolean | null>(null);
    const [page, setPage] = useState(1);
    const PAGE_SIZE = 10;

    const fetchData = useCallback(async (isRefreshing = false) => {
        if (isRefreshing) setRefreshing(true);
        else setLoading(true);

        const [statsResult, usersResult] = await Promise.all([
            apiService.getAdminStats(),
            apiService.getAdminUsers({
                search: search || undefined,
                isActive: isActiveFilter !== null ? isActiveFilter : undefined,
                page,
                pageSize: PAGE_SIZE,
            }),
        ]);

        setStats(statsResult);
        if (usersResult) {
            setUsersData({
                ...usersResult,
                totalPages: Math.ceil((usersResult.totalCount || 0) / PAGE_SIZE),
            });
        }

        if (isRefreshing) setRefreshing(false);
        else setLoading(false);
    }, [search, isActiveFilter, page]);

    useEffect(() => { fetchData(); }, [fetchData]);

    const handleToggleActive = async (userId: number, currentStatus: boolean) => {
        const confirmed = Platform.OS === 'web'
            ? window.confirm(`¿${currentStatus ? 'Desactivar' : 'Activar'} este usuario?`)
            : await new Promise(resolve => {
                Alert.alert(
                    'Confirmar',
                    `¿${currentStatus ? 'Desactivar' : 'Activar'} este usuario?`,
                    [{ text: 'Cancelar', onPress: () => resolve(false) }, { text: 'Confirmar', onPress: () => resolve(true) }]
                );
            });

        if (!confirmed) return;

        const result = await apiService.toggleUserActive(userId);
        if (result) fetchData(true);
    };

    const formatDate = (dateString: string) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: '2-digit' });
    };

    if (loading && !refreshing) {
        return (
            <SafeAreaView style={styles.container}>
                <ActivityIndicator size="large" color={COLORS.primary} style={{ flex: 1 }} />
            </SafeAreaView>
        );
    }

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => setDrawerOpen(true)} style={styles.backButton}>
                    <Menu color={COLORS.text} size={28} />
                </TouchableOpacity>
                <View style={styles.headerTitleContainer}>
                    <Shield color={COLORS.primary} size={20} style={{ marginRight: 8 }} />
                    <Text style={TYPOGRAPHY.h2}>Panel Admin — Usuarios</Text>
                </View>
                <TouchableOpacity onPress={() => fetchData(true)} disabled={refreshing}>
                    <RefreshCw color={COLORS.primary} size={24} style={refreshing ? { opacity: 0.5 } : undefined} />
                </TouchableOpacity>
            </View>

            {/* Side Drawer */}
            <Modal
                transparent
                animationType="slide"
                visible={drawerOpen}
                onRequestClose={() => setDrawerOpen(false)}
            >
                <TouchableOpacity
                    style={styles.drawerOverlay}
                    activeOpacity={1}
                    onPress={() => setDrawerOpen(false)}
                >
                    <View style={styles.drawer}>
                        <View style={styles.drawerHeader}>
                            <Shield color={COLORS.primary} size={22} />
                            <Text style={styles.drawerTitle}>Admin</Text>
                            <TouchableOpacity onPress={() => setDrawerOpen(false)} style={{ marginLeft: 'auto' as any }}>
                                <X color={COLORS.text} size={22} />
                            </TouchableOpacity>
                        </View>

                        {[
                            { icon: <Users color={COLORS.primary} size={20} />, label: 'Usuarios', onPress: () => setDrawerOpen(false) },
                            { icon: <BookOpen color={COLORS.primary} size={20} />, label: 'Ejercicios', onPress: () => { setDrawerOpen(false); navigation.navigate('AdminExercises'); } },
                            { icon: <Upload color={COLORS.primary} size={20} />, label: 'Importar', onPress: () => { setDrawerOpen(false); navigation.navigate('AdminImport'); } },
                        ].map(item => (
                            <TouchableOpacity key={item.label} style={styles.drawerItem} onPress={item.onPress}>
                                {item.icon}
                                <Text style={styles.drawerItemText}>{item.label}</Text>
                                <ChevronRight color="#CCC" size={16} style={{ marginLeft: 'auto' as any }} />
                            </TouchableOpacity>
                        ))}

                        <TouchableOpacity style={styles.drawerItem} onPress={() => { setDrawerOpen(false); navigation.goBack(); }}>
                            <ChevronLeft color="#999" size={20} />
                            <Text style={[styles.drawerItemText, { color: '#999' }]}>Salir del panel</Text>
                        </TouchableOpacity>
                    </View>
                </TouchableOpacity>
            </Modal>

            <ScrollView contentContainerStyle={styles.scrollContent}>
                {/* Stats Row */}
                <View style={styles.statsRow}>
                    {[
                        { label: 'Total', value: stats?.totalUsers ?? 0, icon: '👥' },
                        { label: 'Activos', value: stats?.activeUsers ?? 0, icon: '✅' },
                        { label: 'Hoy', value: stats?.registrationsToday ?? 0, icon: '🆕' },
                    ].map(s => (
                        <View key={s.label} style={styles.statCard}>
                            <Text style={styles.statNum}>{s.icon} {s.value}</Text>
                            <Text style={styles.statLabel}>{s.label}</Text>
                        </View>
                    ))}
                </View>

                {/* Filters */}
                <View style={styles.filterCard}>
                    <Text style={styles.filterTitle}>🔍 Filtros</Text>
                    <View style={styles.searchBar}>
                        <Search color="#999" size={16} />
                        <TextInput
                            style={styles.searchInput}
                            placeholder="Buscar usuario..."
                            value={search}
                            onChangeText={t => { setSearch(t); setPage(1); }}
                        />
                    </View>
                    <View style={styles.filterRow}>
                        {[
                            { label: 'Todos', value: null },
                            { label: 'Activos', value: true },
                            { label: 'Inactivos', value: false },
                        ].map(f => (
                            <TouchableOpacity
                                key={String(f.value)}
                                style={[styles.filterChip, isActiveFilter === f.value && styles.filterChipActive]}
                                onPress={() => { setIsActiveFilter(f.value); setPage(1); }}
                            >
                                <Text style={[styles.filterChipText, isActiveFilter === f.value && styles.filterChipTextActive]}>
                                    {f.label}
                                </Text>
                            </TouchableOpacity>
                        ))}
                    </View>
                </View>

                {/* Users Table */}
                <View style={styles.tableCard}>
                    <View style={styles.tableHeader}>
                        <Text style={styles.tableHeaderTitle}>
                            👤 Usuarios ({usersData.totalCount})
                        </Text>
                    </View>

                    {refreshing && <ActivityIndicator color={COLORS.primary} style={{ marginVertical: 16 }} />}

                    {!refreshing && usersData.items.length === 0 ? (
                        <View style={styles.emptyContainer}>
                            <Text style={styles.emptyText}>No se encontraron usuarios</Text>
                        </View>
                    ) : (
                        usersData.items.map((item: any) => (
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
                        ))
                    )}
                </View>

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
            </ScrollView>
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: COLORS.background },
    header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', padding: SPACING.md, backgroundColor: COLORS.white, borderBottomWidth: 1, borderBottomColor: '#EEE' },
    backButton: { padding: 4 },
    headerTitleContainer: { flexDirection: 'row', alignItems: 'center' },
    scrollContent: { padding: SPACING.md },
    statsRow: { flexDirection: 'row', gap: SPACING.sm, marginBottom: SPACING.md },
    statCard: { flex: 1, backgroundColor: '#fff', borderRadius: 16, padding: SPACING.md, alignItems: 'center', borderWidth: 1, borderColor: '#EEE', elevation: 2 },
    statNum: { fontSize: 20, fontWeight: 'bold', color: COLORS.primary },
    statLabel: { fontSize: 11, color: COLORS.textSecondary, marginTop: 2 },
    filterCard: { backgroundColor: '#fff', borderRadius: 16, padding: SPACING.md, marginBottom: SPACING.md, borderWidth: 1, borderColor: '#EEE' },
    filterTitle: { fontWeight: 'bold', color: COLORS.textSecondary, marginBottom: SPACING.sm, fontSize: 13 },
    searchBar: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#F5F5F5', borderRadius: 10, paddingHorizontal: 12, height: 40, marginBottom: 8, gap: 8 },
    searchInput: { flex: 1, fontSize: 14, color: COLORS.text },
    filterRow: { flexDirection: 'row', gap: 8, flexWrap: 'wrap' },
    filterChip: { paddingHorizontal: 12, paddingVertical: 5, borderRadius: 16, backgroundColor: '#F0F0F0' },
    filterChipActive: { backgroundColor: COLORS.primary },
    filterChipText: { fontSize: 12, fontWeight: '600', color: '#555' },
    filterChipTextActive: { color: '#fff' },
    tableCard: { backgroundColor: '#fff', borderRadius: 16, overflow: 'hidden', borderWidth: 1, borderColor: '#EEE', marginBottom: SPACING.md },
    tableHeader: { padding: SPACING.md, backgroundColor: '#F8F9FA', borderBottomWidth: 1, borderBottomColor: '#EEE' },
    tableHeaderTitle: { fontWeight: 'bold', fontSize: 15, color: COLORS.text },
    emptyContainer: { padding: 32, alignItems: 'center' },
    emptyText: { color: COLORS.textSecondary, fontSize: 14 },
    userRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SPACING.md, paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: '#F5F5F5' },
    userInfo: { flex: 1 },
    username: { fontWeight: 'bold', fontSize: 15, color: COLORS.text },
    email: { fontSize: 12, color: COLORS.textSecondary, marginTop: 2 },
    badgeRow: { flexDirection: 'row', gap: 6, marginTop: 6 },
    roleBadge: { paddingHorizontal: 8, paddingVertical: 2, borderRadius: 8 },
    roleText: { fontSize: 10, fontWeight: 'bold' },
    verifiedBadge: { paddingHorizontal: 8, paddingVertical: 2, borderRadius: 8, backgroundColor: '#E3F2FD' },
    verifiedText: { fontSize: 10, fontWeight: 'bold', color: '#1976D2' },
    actions: { alignItems: 'center', gap: 6 },
    xpMini: { fontSize: 11, color: COLORS.primary, fontWeight: 'bold' },
    toggleBtn: { padding: 8, borderRadius: 12 },
    pagination: { flexDirection: 'row', justifyContent: 'center', alignItems: 'center', marginTop: SPACING.md, gap: 16 },
    pageBtn: { padding: 8, borderRadius: 12, backgroundColor: '#F0F0F0' },
    disabledBtn: { opacity: 0.4 },
    pageIndicator: { fontSize: 13, color: COLORS.textSecondary },
    // Drawer
    drawerOverlay: { flex: 1, flexDirection: 'row', backgroundColor: 'rgba(0,0,0,0.45)' },
    drawer: { width: '70%', maxWidth: 320, backgroundColor: '#fff', paddingTop: 48, shadowColor: '#000', shadowOffset: { width: 4, height: 0 }, shadowOpacity: 0.2, shadowRadius: 12, elevation: 16 },
    drawerHeader: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SPACING.md, paddingBottom: SPACING.md, borderBottomWidth: 1, borderBottomColor: '#EEE', gap: 8 },
    drawerTitle: { fontSize: 18, fontWeight: 'bold', color: COLORS.text },
    drawerItem: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SPACING.md, paddingVertical: 16, borderBottomWidth: 1, borderBottomColor: '#F5F5F5', gap: 12 },
    drawerItemText: { fontSize: 15, fontWeight: '600', color: COLORS.text },
});
