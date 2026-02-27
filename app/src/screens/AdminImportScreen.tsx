// AdminImportScreen — JSON import with checkbox-driven duplicate detection
import React, { useState, useRef } from 'react';
import {
    View, Text, StyleSheet, SafeAreaView, TouchableOpacity,
    ScrollView, ActivityIndicator
} from 'react-native';
import { COLORS, SPACING } from '../theme';
import { ChevronLeft, Upload, Download, CheckSquare, Square } from 'lucide-react-native';
import { apiService } from '../services/apiService';

const LEVELS = ['A1', 'A2', 'B1'];
const THRESHOLDS = [50, 60, 70, 80, 90, 100];

export const AdminImportScreen = ({ navigation }: any) => {
    const [level, setLevel] = useState('A1');
    const [threshold, setThreshold] = useState(0.8);
    const [preview, setPreview] = useState<any[]>([]);
    const [rawItems, setRawItems] = useState<any[]>([]);
    // checked[i] = whether item i is selected for import
    const [checked, setChecked] = useState<boolean[]>([]);
    const [loading, setLoading] = useState(false);
    const [importing, setImporting] = useState(false);
    const [resultMsg, setResultMsg] = useState<string | null>(null);
    const [isDragging, setDragging] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    const parseAndPreview = async (file: File) => {
        setLoading(true);
        setPreview([]);
        setRawItems([]);
        setChecked([]);
        setResultMsg(null);
        try {
            const text = await file.text();
            const json: any[] = JSON.parse(text);
            // Strip exerciseCode (auto-generated), inject selected level
            const items = json.map(({ exerciseCode: _ignored, ...rest }: any) => ({
                ...rest,
                levelId: level,
            }));
            setRawItems(items);
            const result = await apiService.previewImport(items, threshold);
            if (result?.preview) {
                setPreview(result.preview);
                // Pre-check non-duplicates, uncheck duplicates
                setChecked(result.preview.map((p: any) => !p.isDuplicate));
            }
        } catch (e: any) {
            setResultMsg('❌ Error al procesar el JSON: ' + e.message);
        }
        setLoading(false);
    };

    const toggleCheck = (i: number) => {
        setChecked(prev => {
            const next = [...prev];
            next[i] = !next[i];
            return next;
        });
    };

    const toggleAll = () => {
        const anyChecked = checked.some(Boolean);
        setChecked(checked.map(() => !anyChecked));
    };

    const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) parseAndPreview(file);
    };

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault();
        setDragging(false);
        const file = e.dataTransfer.files?.[0];
        if (file) parseAndPreview(file);
    };

    const selectedItems = rawItems.filter((_, i) => checked[i]);
    const checkedCount = checked.filter(Boolean).length;
    const canImport = checkedCount > 0;

    const handleImport = async () => {
        if (!canImport) return;
        setImporting(true);
        const result = await apiService.confirmImport(selectedItems, threshold);
        setImporting(false);
        if (result?.imported !== undefined) {
            setResultMsg(`✅ ${result.imported} ejercicio${result.imported !== 1 ? 's' : ''} importado${result.imported !== 1 ? 's' : ''} correctamente.`);
            setPreview([]);
            setRawItems([]);
            setChecked([]);
        } else {
            setResultMsg('❌ Error al importar. Revisa la consola.');
        }
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <ChevronLeft color={COLORS.text} size={28} />
                </TouchableOpacity>
                <Text style={styles.title}>Importar Ejercicios</Text>
                <View style={{ width: 28 }} />
            </View>

            <ScrollView contentContainerStyle={styles.content}>
                {/* Level selector */}
                <Text style={styles.label}>Nivel</Text>
                <View style={styles.chips}>
                    {LEVELS.map(l => (
                        <TouchableOpacity
                            key={l}
                            style={[styles.chip, level === l && styles.chipActive]}
                            onPress={() => { setLevel(l); setPreview([]); setRawItems([]); setChecked([]); }}
                        >
                            <Text style={[styles.chipText, level === l && styles.chipTextActive]}>{l}</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                {/* Similarity threshold */}
                <Text style={styles.label}>
                    Umbral de similitud: <Text style={styles.thBold}>{Math.round(threshold * 100)}%</Text>
                </Text>
                <Text style={styles.hint}>Similitud ≥ {Math.round(threshold * 100)}% en ES + EN → posible duplicado</Text>
                <View style={styles.chips}>
                    {THRESHOLDS.map(v => (
                        <TouchableOpacity
                            key={v}
                            style={[styles.chipSm, threshold === v / 100 && styles.chipSmActive]}
                            onPress={() => setThreshold(v / 100)}
                        >
                            <Text style={[styles.chipSmText, threshold === v / 100 && styles.chipSmTextActive]}>{v}%</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                {/* Drop zone */}
                <View
                    // @ts-ignore web events
                    onDragOver={(e: any) => { e.preventDefault(); setDragging(true); }}
                    onDragLeave={() => setDragging(false)}
                    onDrop={handleDrop}
                    style={[styles.dropZone, isDragging && styles.dropZoneActive]}
                >
                    <Upload color={isDragging ? COLORS.primary : '#AAA'} size={30} />
                    <Text style={styles.dropText}>{isDragging ? 'Suelta aquí…' : 'Arrastra el JSON aquí'}</Text>
                    <Text style={styles.dropOr}>— o —</Text>
                    {/* @ts-ignore web */}
                    <input
                        ref={inputRef}
                        type="file"
                        accept=".json"
                        onChange={handleFileInput}
                        style={{ display: 'none' }}
                    />
                    <TouchableOpacity
                        style={styles.browseBtn}
                        onPress={() => (inputRef.current as any)?.click()}
                    >
                        <Text style={styles.browseBtnText}>Seleccionar archivo</Text>
                    </TouchableOpacity>
                </View>

                {/* Loading */}
                {loading && <ActivityIndicator color={COLORS.primary} style={{ marginTop: 24 }} size="large" />}

                {/* Preview list */}
                {preview.length > 0 && (
                    <View style={styles.previewCard}>
                        {/* Summary header */}
                        <View style={styles.previewHeader}>
                            <Text style={styles.previewTitle}>{preview.length} ejercicios</Text>
                            <Text style={styles.previewSub}>
                                <Text style={{ color: '#4CAF50' }}>✅ {preview.filter(p => !p.isDuplicate).length} nuevos</Text>
                                {'  '}
                                <Text style={{ color: '#F44336' }}>❌ {preview.filter(p => p.isDuplicate).length} duplicados</Text>
                                {'  '}
                                <Text style={{ color: COLORS.primary }}>☑ {checkedCount} selec.</Text>
                            </Text>
                        </View>

                        {/* Column headings */}
                        <View style={styles.colHeadRow}>
                            {/* Checkbox header — toggle all */}
                            <TouchableOpacity onPress={toggleAll} style={styles.colCheck}>
                                {checked.every(Boolean)
                                    ? <CheckSquare color={COLORS.primary} size={16} />
                                    : <Square color="#AAA" size={16} />
                                }
                            </TouchableOpacity>
                            <Text style={[styles.colHead, { flex: 2 }]}>Pregunta (ES)</Text>
                            <Text style={[styles.colHead, { flex: 0.6 }]}>Topic</Text>
                            <Text style={[styles.colHead, { flex: 0.45, textAlign: 'center' }]}>%</Text>
                            <Text style={[styles.colHead, { flex: 1 }]}>Coincide con</Text>
                        </View>

                        {preview.map((p, i) => {
                            const isChecked = checked[i] ?? false;
                            return (
                                <TouchableOpacity
                                    key={i}
                                    style={[styles.previewRow, isChecked ? styles.rowChecked : styles.rowUnchecked]}
                                    onPress={() => toggleCheck(i)}
                                    activeOpacity={0.7}
                                >
                                    {/* Checkbox */}
                                    <View style={styles.colCheck}>
                                        {isChecked
                                            ? <CheckSquare color={COLORS.primary} size={18} />
                                            : <Square color="#CCC" size={18} />
                                        }
                                    </View>
                                    <Text style={[styles.previewCell, { flex: 2 }]} numberOfLines={2}>
                                        {p.isDuplicate ? '❌' : '✅'} {p.questionEs}
                                    </Text>
                                    <Text style={[styles.previewCell, { flex: 0.6 }]}>{p.topics}</Text>
                                    <Text style={[styles.previewCell, styles.pctCell, { flex: 0.45 }]}>
                                        {p.similarityPct}%
                                    </Text>
                                    <Text style={[styles.previewCell, styles.matchCode, { flex: 1 }]} numberOfLines={1}>
                                        {p.matchedCode ?? '—'}
                                    </Text>
                                </TouchableOpacity>
                            );
                        })}

                        {/* Import button */}
                        <TouchableOpacity
                            style={[styles.importBtn, !canImport && styles.importBtnDisabled]}
                            onPress={handleImport}
                            disabled={!canImport || importing}
                        >
                            {importing
                                ? <ActivityIndicator color="#fff" />
                                : <>
                                    <Download color="#fff" size={18} />
                                    <Text style={styles.importBtnText}>
                                        Importar {checkedCount} ejercicio{checkedCount !== 1 ? 's' : ''}
                                    </Text>
                                </>
                            }
                        </TouchableOpacity>
                    </View>
                )}

                {/* Result */}
                {resultMsg && (
                    <View style={[styles.resultBox, resultMsg.startsWith('✅') ? styles.resultOk : styles.resultErr]}>
                        <Text style={styles.resultText}>{resultMsg}</Text>
                    </View>
                )}
            </ScrollView>
        </SafeAreaView>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: COLORS.background },
    header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SPACING.md, paddingVertical: 12, backgroundColor: '#fff', borderBottomWidth: 1, borderBottomColor: '#EEE' },
    title: { fontSize: 17, fontWeight: 'bold', color: COLORS.text },
    content: { padding: SPACING.md, gap: 12 },
    label: { fontSize: 13, fontWeight: 'bold', color: COLORS.textSecondary, marginBottom: 2 },
    hint: { fontSize: 12, color: '#AAA', marginBottom: 4 },
    chips: { flexDirection: 'row', gap: 8, flexWrap: 'wrap', marginBottom: 4 },
    chip: { paddingHorizontal: 16, paddingVertical: 7, borderRadius: 20, backgroundColor: '#F0F0F0' },
    chipActive: { backgroundColor: COLORS.primary },
    chipText: { fontSize: 13, fontWeight: '600', color: '#555' },
    chipTextActive: { color: '#fff' },
    chipSm: { paddingHorizontal: 11, paddingVertical: 5, borderRadius: 14, backgroundColor: '#F0F0F0' },
    chipSmActive: { backgroundColor: COLORS.secondary },
    chipSmText: { fontSize: 12, fontWeight: '600', color: '#555' },
    chipSmTextActive: { color: '#fff' },
    thBold: { color: COLORS.primary, fontWeight: 'bold' },
    dropZone: { borderWidth: 2, borderStyle: 'dashed', borderColor: '#CCC', borderRadius: 16, padding: 28, alignItems: 'center', backgroundColor: '#FAFAFA', gap: 8 },
    dropZoneActive: { borderColor: COLORS.primary, backgroundColor: '#E8F5E9' },
    dropText: { fontSize: 15, color: '#666', fontWeight: '600' },
    dropOr: { color: '#AAA', fontSize: 13 },
    browseBtn: { backgroundColor: COLORS.primary, paddingHorizontal: 20, paddingVertical: 10, borderRadius: 12 },
    browseBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 14 },
    previewCard: { backgroundColor: '#fff', borderRadius: 16, overflow: 'hidden', borderWidth: 1, borderColor: '#EEE' },
    previewHeader: { padding: SPACING.md, backgroundColor: '#F8F9FA', borderBottomWidth: 1, borderBottomColor: '#EEE' },
    previewTitle: { fontWeight: 'bold', fontSize: 15, color: COLORS.text },
    previewSub: { fontSize: 13, marginTop: 4 },
    colHeadRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 8, paddingVertical: 7, backgroundColor: '#F0F0F0', borderBottomWidth: 1, borderBottomColor: '#E0E0E0' },
    colCheck: { width: 30, alignItems: 'center' },
    colHead: { fontSize: 10, fontWeight: 'bold', color: '#888', textTransform: 'uppercase' },
    previewRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 8, paddingVertical: 10, borderBottomWidth: 1, borderBottomColor: '#F5F5F5' },
    rowChecked: { backgroundColor: '#F9FFF9' },
    rowUnchecked: { backgroundColor: '#FFF9F9' },
    previewCell: { fontSize: 12, color: COLORS.text },
    pctCell: { fontWeight: 'bold', color: COLORS.textSecondary, textAlign: 'center' },
    matchCode: { fontSize: 11, fontFamily: 'monospace', color: '#F44336' },
    importBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8, margin: SPACING.md, backgroundColor: COLORS.primary, borderRadius: 12, paddingVertical: 14 },
    importBtnDisabled: { opacity: 0.35 },
    importBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 15 },
    resultBox: { borderRadius: 12, padding: SPACING.md },
    resultOk: { backgroundColor: '#E8F5E9' },
    resultErr: { backgroundColor: '#FFEBEE' },
    resultText: { fontSize: 14, fontWeight: '600', color: COLORS.text },
});
