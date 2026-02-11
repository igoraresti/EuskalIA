import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Platform, Modal, TouchableWithoutFeedback } from 'react-native';
import { useTranslation } from 'react-i18next';
import { ChevronDown, Globe } from 'lucide-react-native';

interface LanguageSelectorProps {
    onLanguageChange?: (language: string) => void;
    compact?: boolean;
}

const LANGUAGES = [
    { code: 'es', name: 'Español', nativeName: 'Español' },
    { code: 'en', name: 'English', nativeName: 'English' },
    { code: 'pl', name: 'Polish', nativeName: 'Polski' },
    { code: 'eu', name: 'Basque', nativeName: 'Euskera' },
    { code: 'fr', name: 'French', nativeName: 'Français' },
];

export const LanguageSelector: React.FC<LanguageSelectorProps> = ({ onLanguageChange, compact = false }) => {
    const { i18n, t } = useTranslation();
    const [isOpen, setIsOpen] = React.useState(false);

    const currentLanguage = LANGUAGES.find(lang => lang.code === i18n.language) || LANGUAGES[0];

    const handleLanguageSelect = async (languageCode: string) => {
        await i18n.changeLanguage(languageCode);
        setIsOpen(false);

        // Store in localStorage for persistence
        if (Platform.OS === 'web') {
            localStorage.setItem('i18nextLng', languageCode);
        }

        if (onLanguageChange) {
            onLanguageChange(languageCode);
        }
    };

    return (
        <View style={styles.container}>
            <TouchableOpacity
                style={[styles.selector, compact && styles.selectorCompact]}
                onPress={() => setIsOpen(true)}
            >
                <View style={styles.selectorLeft}>
                    <Globe size={compact ? 16 : 18} color="#2D5F3F" style={{ marginRight: 8 }} />
                    <Text style={[styles.selectedText, compact && styles.selectedTextCompact]}>
                        {currentLanguage.nativeName}
                    </Text>
                </View>
                <ChevronDown size={compact ? 16 : 20} color="#2D5F3F" />
            </TouchableOpacity>

            <Modal
                visible={isOpen}
                transparent={true}
                animationType="fade"
                onRequestClose={() => setIsOpen(false)}
            >
                <TouchableOpacity
                    style={styles.modalOverlay}
                    activeOpacity={1}
                    onPress={() => setIsOpen(false)}
                >
                    <TouchableWithoutFeedback>
                        <View style={styles.modalContent}>
                            <Text style={styles.modalTitle}>{t('profile.changeLanguage')}</Text>
                            {LANGUAGES.map((language) => (
                                <TouchableOpacity
                                    key={language.code}
                                    style={[
                                        styles.option,
                                        language.code === i18n.language && styles.optionSelected
                                    ]}
                                    onPress={() => handleLanguageSelect(language.code)}
                                >
                                    <Text style={[
                                        styles.optionText,
                                        language.code === i18n.language && styles.optionTextSelected
                                    ]}>
                                        {language.nativeName}
                                    </Text>
                                    {language.code === i18n.language && (
                                        <View style={styles.checkIcon} />
                                    )}
                                </TouchableOpacity>
                            ))}
                        </View>
                    </TouchableWithoutFeedback>
                </TouchableOpacity>
            </Modal>
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: '100%',
    },
    selector: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingHorizontal: 16,
        paddingVertical: 12,
        backgroundColor: '#F5F7F9',
        borderRadius: 12,
        borderWidth: 1,
        borderColor: '#E0E0E0',
        width: '100%',
    },
    selectorCompact: {
        paddingHorizontal: 12,
        paddingVertical: 8,
        backgroundColor: '#fff',
        borderColor: '#2D5F3F',
        minWidth: 120,
        width: 'auto',
    },
    selectorLeft: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    selectedText: {
        fontSize: 16,
        color: '#333',
        fontWeight: '500',
    },
    selectedTextCompact: {
        fontSize: 14,
        color: '#2D5F3F',
        fontWeight: '600',
    },
    modalOverlay: {
        flex: 1,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        justifyContent: 'center',
        alignItems: 'center',
        padding: 20,
    },
    modalContent: {
        backgroundColor: '#fff',
        borderRadius: 20,
        width: '100%',
        maxWidth: 340,
        paddingVertical: 10,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 10 },
        shadowOpacity: 0.25,
        shadowRadius: 15,
        elevation: 10,
    },
    modalTitle: {
        fontSize: 18,
        fontWeight: 'bold',
        padding: 20,
        textAlign: 'center',
        color: '#333',
        borderBottomWidth: 1,
        borderBottomColor: '#F0F0F0',
    },
    option: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingHorizontal: 20,
        paddingVertical: 16,
        borderBottomWidth: 1,
        borderBottomColor: '#F0F0F0',
    },
    optionSelected: {
        backgroundColor: '#F0F7F3',
    },
    optionText: {
        fontSize: 16,
        color: '#333',
    },
    optionTextSelected: {
        color: '#2D5F3F',
        fontWeight: 'bold',
    },
    checkIcon: {
        width: 10,
        height: 10,
        borderRadius: 5,
        backgroundColor: '#2D5F3F',
    },
});
