// AdminExercisesScreen — multi-select with bulk status actions
import React, { useState, useEffect, useCallback } from 'react';
import {
    View, Text, StyleSheet, SafeAreaView, TouchableOpacity,
    ScrollView, TextInput, ActivityIndicator, Modal
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING } from '../theme';
import {
    ChevronLeft, ChevronUp, ChevronDown, ChevronRight,
    Search, Trash2, RefreshCw, Info, CheckSquare, Square, X
} from 'lucide-react-native';
import { apiService } from '../services/apiService';

const LEVELS = ['', 'A1', 'A2', 'B1'];
const STATUSES = ['', 'BETA', 'APPROVED', 'REJECTED'];
const PAGE_SIZE = 20;

type SortField = 'exerciseCode' | 'levelId' | 'templateType' | 'status' | 'correct' | 'wrong';

const COLS = [
    { key: 'exerciseCode' as SortField, label: 'Código', w: '27%', sort: true },
    { key: 'levelId' as SortField, label: 'Nv.', w: '7%', sort: true },
    { key: 'templateType' as SortField, label: 'Tipo', w: '16%', sort: true },
    { key: 'status' as SortField, label: 'Estado', w: '14%', sort: true },
    { key: 'correct' as SortField, label: '✅', w: '8%', sort: true },
    { key: 'wrong' as SortField, label: '❌', w: '8%', sort: true },
] as const;
const CB_W = '5%';   // checkbox column
const ACT_W = '15%';  // actions column

const STATUS_COLORS: Record<string, string> = {
    BETA: '#FF9800', APPROVED: '#4CAF50', REJECTED: '#F44336',
};
const statusColor = (s: string) => STATUS_COLORS[s] ?? '#999';

function parseExercise(jsonSchema: string, lang: string) {
    try {
        const p = JSON.parse(jsonSchema);
        if (p.question) return { type: 'choice', question: p.question[lang] ?? p.question['es'] ?? '', options: p.options ?? [], correct: p.correctAnswer ?? '' };
        if (p.promptLocal) return { type: 'block', question: p.promptLocal[lang] ?? p.promptLocal['es'] ?? '', target: p.validation?.targetWord ?? '', translation: p.validation?.targetTranslation?.[lang] ?? '', pieces: p.elements?.pieces?.map((x: any) => x.text) ?? [] };
        if (p.sentence) { const q = typeof p.sentence === 'object' ? (p.sentence[lang] ?? p.sentence['es']) : p.sentence; return { type: 'fill', question: q, options: p.options ?? [], correct: p.correctAnswer ?? '' }; }
    } catch { }
    return null;
}

export const AdminExercisesScreen = ({ navigation }: any) => {
    const { i18n } = useTranslation();
    const lang = i18n.language?.slice(0, 2) ?? 'es';

    const [data, setData] = useState<any>({ total: 0, items: [] });
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [search, setSearch] = useState('');
    const [levelFilter, setLevel] = useState('');
    const [statusFilter, setStatus] = useState('');
    const [sortBy, setSortBy] = useState<SortField>('exerciseCode');
    const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');

    // Multi-select
    const [selected, setSelected] = useState<Set<string>>(new Set());
    const [bulkLoading, setBulkLoading] = useState(false);

    // Modals
    const [deleteTarget, setDeleteTarget] = useState<{ id: string; code: string } | null>(null);
    const [detailEx, setDetailEx] = useState<any | null>(null);
    const [detailStats, setDetailStats] = useState<any | null>(null);
    const [loadingDetail, setLoadingDetail] = useState(false);

    const totalPages = Math.max(1, Math.ceil(data.total / PAGE_SIZE));
    const pageIds: string[] = data.items.map((e: any) => e.id);
    const allPageSelected = pageIds.length > 0 && pageIds.every(id => selected.has(id));
    const someSelected = selected.size > 0;

    const fetchData = useCallback(async () => {
        setLoading(true);
        const result = await apiService.getAdminExercises({
            levelId: levelFilter || undefined,
            status: statusFilter || undefined,
            search: search || undefined,
            sortBy, sortDir, page, pageSize: PAGE_SIZE,
        });
        if (result) setData(result);
        setLoading(false);
    }, [levelFilter, statusFilter, search, sortBy, sortDir, page]);

    useEffect(() => { fetchData(); }, [fetchData]);

    const handleSort = (field: SortField) => {
        if (sortBy === field) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
        else { setSortBy(field); setSortDir('asc'); }
        setPage(1);
    };

    // Selection helpers
    const toggleSelect = (id: string) => {
        setSelected(prev => {
            const next = new Set(prev);
            next.has(id) ? next.delete(id) : next.add(id);
            return next;
        });
    };
    const toggleSelectAll = () => {
        if (allPageSelected) {
            setSelected(prev => { const n = new Set(prev); pageIds.forEach(id => n.delete(id)); return n; });
        } else {
            setSelected(prev => { const n = new Set(prev); pageIds.forEach(id => n.add(id)); return n; });
        }
    };
    const clearSelection = () => setSelected(new Set());

    // Bulk status change
    const handleBulkStatus = async (status: string) => {
        if (selected.size === 0) return;
        setBulkLoading(true);
        const result = await apiService.bulkUpdateExerciseStatus(Array.from(selected), status);
        setBulkLoading(false);
        if (result) { clearSelection(); fetchData(); }
    };

    // Detail
    const openDetail = async (ex: any) => {
        setDetailEx(ex);
        setDetailStats(null);
        setLoadingDetail(true);
        const s = await apiService.getAdminExerciseStats(ex.id);
        setDetailStats(s);
        setLoadingDetail(false);
    };

    // Delete
    const handleDelete = async () => {
        if (!deleteTarget) return;
        await apiService.deleteAdminExercise(deleteTarget.id);
        setDeleteTarget(null);
        fetchData();
    };

    return (
        <SafeAreaView style={styles.container}>
            {/* Header */}
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()} style={styles.headerBtn}>
                    <ChevronLeft color={COLORS.text} size={28} />
                </TouchableOpacity>
                <Text style={styles.title}>Ejercicios ({data.total})</Text>
                <TouchableOpacity onPress={fetchData} style={styles.headerBtn}>
                    <RefreshCw color={COLORS.primary} size={22} />
                </TouchableOpacity>
            </View>

            {/* Filters */}
            <View style={styles.filterBar}>
                <View style={styles.searchBox}>
                    <Search color="#999" size={16} />
                    <TextInput style={styles.searchInput} placeholder="Buscar código o texto..." value={search} onChangeText={t => { setSearch(t); setPage(1); }} />
                </View>
                <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ marginTop: 4 }}>
                    {LEVELS.map(l => (
                        <TouchableOpacity key={'l' + l} style={[styles.chip, levelFilter === l && styles.chipActive]} onPress={() => { setLevel(l); setPage(1); }}>
                            <Text style={[styles.chipText, levelFilter === l && styles.chipTextActive]}>{l || 'Todos'}</Text>
                        </TouchableOpacity>
                    ))}
                    <View style={styles.chipSep} />
                    {STATUSES.map(s => (
                        <TouchableOpacity key={'s' + s} style={[styles.chip, statusFilter === s && styles.chipActive]} onPress={() => { setStatus(s); setPage(1); }}>
                            <Text style={[styles.chipText, statusFilter === s && styles.chipTextActive]}>{s || 'Estado'}</Text>
                        </TouchableOpacity>
                    ))}
                </ScrollView>
            </View>

            {/* Bulk action bar — appears when something is selected */}
            {someSelected && (
                <View style={styles.bulkBar}>
                    <TouchableOpacity onPress={clearSelection} style={styles.bulkClear}>
                        <X color="#fff" size={16} />
                    </TouchableOpacity>
                    <Text style={styles.bulkCount}>{selected.size} seleccionado{selected.size !== 1 ? 's' : ''}</Text>
                    <View style={styles.bulkActions}>
                        {bulkLoading ? <ActivityIndicator color="#fff" size="small" /> : (
                            <>
                                <TouchableOpacity style={[styles.bulkBtn, { backgroundColor: '#4CAF50' }]} onPress={() => handleBulkStatus('APPROVED')}>
                                    <Text style={styles.bulkBtnText}>✅ Aprobar</Text>
                                </TouchableOpacity>
                                <TouchableOpacity style={[styles.bulkBtn, { backgroundColor: '#FF9800' }]} onPress={() => handleBulkStatus('BETA')}>
                                    <Text style={styles.bulkBtnText}>🔄 BETA</Text>
                                </TouchableOpacity>
                                <TouchableOpacity style={[styles.bulkBtn, { backgroundColor: '#F44336' }]} onPress={() => handleBulkStatus('REJECTED')}>
                                    <Text style={styles.bulkBtnText}>❌ Rechazar</Text>
                                </TouchableOpacity>
                            </>
                        )}
                    </View>
                </View>
            )}

            {/* Table */}
            <View style={styles.tableWrap}>
                {/* Header row */}
                <View style={styles.headRow}>
                    {/* Select-all checkbox */}
                    <TouchableOpacity onPress={toggleSelectAll} style={[styles.thCell, { width: CB_W }]}>
                        {allPageSelected
                            ? <CheckSquare color={COLORS.primary} size={14} />
                            : <Square color="#CCC" size={14} />
                        }
                    </TouchableOpacity>
                    {COLS.map(col => (
                        <TouchableOpacity key={col.key} style={[styles.thCell, { width: col.w }]} onPress={() => col.sort && handleSort(col.key)} disabled={!col.sort}>
                            <Text style={styles.thText}>{col.label}</Text>
                            {col.sort && (
                                sortBy === col.key
                                    ? (sortDir === 'asc' ? <ChevronUp color={COLORS.primary} size={10} /> : <ChevronDown color={COLORS.primary} size={10} />)
                                    : <ChevronUp color="#DDD" size={10} />
                            )}
                        </TouchableOpacity>
                    ))}
                    <View style={[styles.thCell, { width: ACT_W, justifyContent: 'center' }]}>
                        <Text style={styles.thText}>⚙</Text>
                    </View>
                </View>

                {/* Data */}
                <ScrollView>
                    {loading ? (
                        <ActivityIndicator size="large" color={COLORS.primary} style={{ margin: 40 }} />
                    ) : data.items.length === 0 ? (
                        <View style={styles.empty}><Text style={styles.emptyText}>Sin resultados</Text></View>
                    ) : data.items.map((ex: any, idx: number) => {
                        const isSelected = selected.has(ex.id);
                        return (
                            <TouchableOpacity
                                key={ex.id}
                                style={[styles.dataRow, idx % 2 === 1 && styles.dataRowAlt, isSelected && styles.dataRowSelected]}
                                onPress={() => toggleSelect(ex.id)}
                                activeOpacity={0.7}
                            >
                                {/* Checkbox */}
                                <View style={{ width: CB_W, alignItems: 'center' }}>
                                    {isSelected
                                        ? <CheckSquare color={COLORS.primary} size={16} />
                                        : <Square color="#CCC" size={16} />
                                    }
                                </View>
                                <Text style={[styles.tdCode, { width: '27%' }]} numberOfLines={1}>{ex.exerciseCode}</Text>
                                <Text style={[styles.td, { width: '7%' }, styles.tdCenter]}>{ex.levelId}</Text>
                                <Text style={[styles.td, { width: '16%' }]} numberOfLines={1}>{ex.templateType?.replace('_', ' ')}</Text>
                                <View style={{ width: '14%', justifyContent: 'center', alignItems: 'flex-start' }}>
                                    <View style={[styles.badge, { backgroundColor: statusColor(ex.status) + '22' }]}>
                                        <Text style={[styles.badgeText, { color: statusColor(ex.status) }]}>{ex.status}</Text>
                                    </View>
                                </View>
                                <Text style={[styles.td, { width: '8%' }, styles.tdCenter, styles.tdGreen]}>{ex.correct ?? '—'}</Text>
                                <Text style={[styles.td, { width: '8%' }, styles.tdCenter, styles.tdRed]}>{ex.wrong ?? '—'}</Text>
                                {/* Actions — stop propagation so tap on icon doesn't toggle row */}
                                <View style={[{ width: ACT_W }, styles.actCell]}>
                                    <TouchableOpacity onPress={(e) => { e.stopPropagation?.(); openDetail(ex); }} style={styles.iconBtn}>
                                        <Info color={COLORS.primary} size={16} />
                                    </TouchableOpacity>
                                    <TouchableOpacity onPress={(e) => { e.stopPropagation?.(); setDeleteTarget({ id: ex.id, code: ex.exerciseCode }); }} style={styles.iconBtn}>
                                        <Trash2 color="#F44336" size={16} />
                                    </TouchableOpacity>
                                </View>
                            </TouchableOpacity>
                        );
                    })}
                </ScrollView>
            </View>

            {/* Pagination */}
            <View style={styles.pagination}>
                <TouchableOpacity onPress={() => setPage(p => p - 1)} disabled={page === 1} style={[styles.pageBtn, page === 1 && styles.pageBtnDisabled]}>
                    <ChevronLeft color={page === 1 ? '#CCC' : COLORS.primary} size={22} />
                </TouchableOpacity>
                <Text style={styles.pageText}>Pág. {page} / {totalPages}</Text>
                <TouchableOpacity onPress={() => setPage(p => p + 1)} disabled={page === totalPages} style={[styles.pageBtn, page === totalPages && styles.pageBtnDisabled]}>
                    <ChevronRight color={page === totalPages ? '#CCC' : COLORS.primary} size={22} />
                </TouchableOpacity>
            </View>

            {/* ── Detail Modal ── */}
            {detailEx && (
                <Modal transparent animationType="fade" visible onRequestClose={() => setDetailEx(null)}>
                    <View style={styles.overlay}>
                        <View style={styles.modal}>
                            <View style={styles.modalHead}>
                                <Text style={styles.modalCode} numberOfLines={1}>{detailEx.exerciseCode}</Text>
                                <TouchableOpacity onPress={() => setDetailEx(null)} style={{ padding: 4 }}>
                                    <Text style={styles.modalClose}>✕</Text>
                                </TouchableOpacity>
                            </View>
                            <ScrollView style={{ maxHeight: 500 }}>
                                <View style={styles.metaRow}>
                                    {[detailEx.levelId, detailEx.templateType, detailEx.topics].map(t => (
                                        <Text key={t} style={styles.metaTag}>{t}</Text>
                                    ))}
                                    <View style={[styles.badge, { backgroundColor: statusColor(detailEx.status) + '22' }]}>
                                        <Text style={[styles.badgeText, { color: statusColor(detailEx.status) }]}>{detailEx.status}</Text>
                                    </View>
                                </View>
                                {loadingDetail ? (
                                    <ActivityIndicator color={COLORS.primary} style={{ marginVertical: 16 }} />
                                ) : detailStats && (
                                    <View style={styles.statsGrid}>
                                        {[
                                            { label: '✅ Aciertos', value: detailStats.correct, color: '#4CAF50' },
                                            { label: '❌ Fallos', value: detailStats.wrong, color: '#F44336' },
                                            { label: '📊 Total', value: detailStats.total, color: COLORS.primary },
                                            { label: '👤 Usuarios', value: detailStats.uniqueUsers, color: '#9C27B0' },
                                        ].map(s => (
                                            <View key={s.label} style={styles.statCard}>
                                                <Text style={[styles.statNum, { color: s.color }]}>{s.value}</Text>
                                                <Text style={styles.statLabel}>{s.label}</Text>
                                            </View>
                                        ))}
                                    </View>
                                )}
                                {(() => {
                                    const ex = parseExercise(detailEx.jsonSchema, lang);
                                    if (!ex) return null;
                                    return (
                                        <View style={styles.exContent}>
                                            <Text style={styles.sectionTitle}>📝 Pregunta ({lang.toUpperCase()})</Text>
                                            <Text style={styles.questionText}>{ex.question}</Text>
                                            {ex.type === 'choice' && ex.options.map((opt: string) => (
                                                <View key={opt} style={[styles.optRow, opt === ex.correct && styles.optCorrect]}>
                                                    <Text style={[styles.optText, opt === ex.correct && { color: '#4CAF50', fontWeight: 'bold' }]}>{opt === ex.correct ? '✅ ' : '○ '}{opt}</Text>
                                                </View>
                                            ))}
                                            {ex.type === 'block' && (
                                                <>
                                                    <Text style={styles.sectionTitle}>Objetivo</Text>
                                                    <Text style={styles.optText}><Text style={{ fontWeight: 'bold' }}>{ex.target}</Text>{ex.translation ? `  (${ex.translation})` : ''}</Text>
                                                    <Text style={[styles.sectionTitle, { marginTop: 8 }]}>Piezas</Text>
                                                    <Text style={styles.optText}>{ex.pieces.join('  ·  ')}</Text>
                                                </>
                                            )}
                                        </View>
                                    );
                                })()}
                            </ScrollView>
                        </View>
                    </View>
                </Modal>
            )}

            {/* ── Delete Modal ── */}
            {deleteTarget && (
                <Modal transparent animationType="fade" visible onRequestClose={() => setDeleteTarget(null)}>
                    <View style={styles.overlay}>
                        <View style={[styles.modal, { maxHeight: 260 }]}>
                            <Text style={styles.deleteTitle}>⚠️ Eliminar ejercicio</Text>
                            <Text style={styles.deleteBody}>¿Eliminar <Text style={{ fontWeight: 'bold' }}>{deleteTarget.code}</Text>?{'\n'}También se borrarán todos sus intentos.</Text>
                            <View style={styles.modalBtns}>
                                <TouchableOpacity style={styles.cancelBtn} onPress={() => setDeleteTarget(null)}><Text style={styles.cancelBtnText}>Cancelar</Text></TouchableOpacity>
                                <TouchableOpacity style={styles.confirmBtn} onPress={handleDelete}><Text style={styles.confirmBtnText}>Eliminar</Text></TouchableOpacity>
                            </View>
                        </View>
                    </View>
                </Modal>
            )}
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: COLORS.background },
    header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SPACING.md, paddingVertical: 10, backgroundColor: '#fff', borderBottomWidth: 1, borderBottomColor: '#EEE' },
    headerBtn: { padding: 4 },
    title: { fontSize: 17, fontWeight: 'bold', color: COLORS.text },
    filterBar: { backgroundColor: '#fff', paddingHorizontal: SPACING.md, paddingBottom: 8, paddingTop: 4, borderBottomWidth: 1, borderBottomColor: '#EEE' },
    searchBox: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#F5F5F5', borderRadius: 10, paddingHorizontal: 12, height: 36, gap: 8 },
    searchInput: { flex: 1, fontSize: 14, color: COLORS.text },
    chip: { paddingHorizontal: 12, paddingVertical: 4, borderRadius: 16, backgroundColor: '#F0F0F0', marginRight: 6 },
    chipActive: { backgroundColor: COLORS.primary },
    chipText: { fontSize: 12, fontWeight: '600', color: '#555' },
    chipTextActive: { color: '#fff' },
    chipSep: { width: 1, backgroundColor: '#DDD', marginHorizontal: 4, alignSelf: 'center', height: 18 },
    // Bulk bar
    bulkBar: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#333', paddingHorizontal: SPACING.md, paddingVertical: 8, gap: 10 },
    bulkClear: { padding: 4 },
    bulkCount: { color: '#fff', fontWeight: '600', fontSize: 13 },
    bulkActions: { flexDirection: 'row', gap: 8, marginLeft: 'auto' as any },
    bulkBtn: { paddingHorizontal: 12, paddingVertical: 6, borderRadius: 8 },
    bulkBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 12 },
    // Table
    tableWrap: { flex: 1, backgroundColor: '#fff' },
    headRow: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#F8F9FA', borderBottomWidth: 1, borderBottomColor: '#DDD', paddingHorizontal: SPACING.md, paddingVertical: 8 },
    thCell: { flexDirection: 'row', alignItems: 'center', gap: 3 },
    thText: { fontSize: 10, fontWeight: 'bold', color: '#888', textTransform: 'uppercase' },
    dataRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SPACING.md, paddingVertical: 10, borderBottomWidth: 1, borderBottomColor: '#F0F0F0' },
    dataRowAlt: { backgroundColor: '#FAFAFA' },
    dataRowSelected: { backgroundColor: '#EBF5FF' },
    td: { fontSize: 12, color: COLORS.text },
    tdCode: { fontSize: 11, fontFamily: 'monospace', color: COLORS.text },
    tdCenter: { textAlign: 'center' },
    tdGreen: { fontWeight: 'bold', color: '#4CAF50', textAlign: 'center' },
    tdRed: { fontWeight: 'bold', color: '#F44336', textAlign: 'center' },
    badge: { paddingHorizontal: 5, paddingVertical: 1, borderRadius: 4 },
    badgeText: { fontSize: 9, fontWeight: 'bold' },
    actCell: { flexDirection: 'row', alignItems: 'center', justifyContent: 'flex-end', gap: 2 },
    iconBtn: { padding: 6 },
    empty: { padding: 40, alignItems: 'center' },
    emptyText: { color: COLORS.textSecondary, fontSize: 14 },
    pagination: { flexDirection: 'row', justifyContent: 'center', alignItems: 'center', paddingVertical: 10, backgroundColor: '#fff', borderTopWidth: 1, borderTopColor: '#EEE', gap: 20 },
    pageBtn: { padding: 8, borderRadius: 8, backgroundColor: '#F0F0F0' },
    pageBtnDisabled: { opacity: 0.3 },
    pageText: { fontSize: 13, color: COLORS.textSecondary, fontWeight: '600', minWidth: 90, textAlign: 'center' },
    // Detail modal
    overlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.55)', justifyContent: 'center', alignItems: 'center', padding: 20 },
    modal: { backgroundColor: '#fff', borderRadius: 20, padding: 20, width: '100%', maxWidth: 420 },
    modalHead: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 },
    modalCode: { fontFamily: 'monospace', fontWeight: 'bold', fontSize: 13, color: COLORS.primary, flex: 1, marginRight: 8 },
    modalClose: { fontSize: 18, color: '#999' },
    metaRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 6, marginBottom: 12 },
    metaTag: { fontSize: 11, color: '#666', backgroundColor: '#F0F0F0', paddingHorizontal: 8, paddingVertical: 2, borderRadius: 8 },
    statsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: 12 },
    statCard: { flex: 1, minWidth: 70, backgroundColor: '#F8F9FA', borderRadius: 12, padding: 10, alignItems: 'center' },
    statNum: { fontSize: 22, fontWeight: 'bold' },
    statLabel: { fontSize: 11, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
    exContent: { borderTopWidth: 1, borderTopColor: '#EEE', paddingTop: 12 },
    sectionTitle: { fontSize: 10, fontWeight: 'bold', color: '#AAA', textTransform: 'uppercase', marginBottom: 6, marginTop: 10 },
    questionText: { fontSize: 15, color: COLORS.text, fontWeight: '600', lineHeight: 22 },
    optRow: { paddingVertical: 6, paddingHorizontal: 8, borderRadius: 8, marginBottom: 4, backgroundColor: '#F8F8F8' },
    optCorrect: { paddingVertical: 6, paddingHorizontal: 8, borderRadius: 8, marginBottom: 4, backgroundColor: '#E8F5E9' },
    optText: { fontSize: 13, color: COLORS.text },
    // Delete modal
    deleteTitle: { fontSize: 18, fontWeight: 'bold', color: COLORS.text, marginBottom: 12 },
    deleteBody: { fontSize: 14, color: COLORS.textSecondary, lineHeight: 22, marginBottom: 24 },
    modalBtns: { flexDirection: 'row', gap: 12 },
    cancelBtn: { flex: 1, padding: 14, borderRadius: 12, backgroundColor: '#F0F0F0', alignItems: 'center' },
    cancelBtnText: { fontWeight: '600', color: COLORS.text },
    confirmBtn: { flex: 1, padding: 14, borderRadius: 12, backgroundColor: '#F44336', alignItems: 'center' },
    confirmBtnText: { fontWeight: 'bold', color: '#fff' },
});
