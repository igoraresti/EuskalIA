import React, { useState } from 'react';
import { View, Text, StyleSheet, SafeAreaView, ScrollView, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform } from 'react-native';
import { COLORS, SPACING, TYPOGRAPHY } from '../theme';
import { Send, ArrowLeft } from 'lucide-react-native';

export const AIChatScreen = ({ navigation }: any) => {
    const [message, setMessage] = useState('');
    const [messages, setMessages] = useState([
        { id: 1, text: 'Kaixo! Soy tu tutor de IA. ¿En qué puedo ayudarte hoy?', sender: 'ai' },
        { id: 2, text: 'Quisiera practicar saludos en euskera.', sender: 'user' },
        { id: 3, text: '¡Excelente! "Kaixo" es Hola. ¿Sabes cómo se dice "Buenos días"?', sender: 'ai' },
    ]);

    const handleSend = () => {
        if (message.trim()) {
            setMessages([...messages, { id: Date.now(), text: message, sender: 'user' }]);
            setMessage('');
            // Simulate IA response
            setTimeout(() => {
                setMessages(prev => [...prev, { id: Date.now() + 1, text: '¡Muy bien! "Egun on" es correcto.', sender: 'ai' }]);
            }, 1000);
        }
    };

    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.header}>
                <TouchableOpacity onPress={() => navigation.goBack()}>
                    <ArrowLeft color={COLORS.primary} size={28} />
                </TouchableOpacity>
                <Text style={[TYPOGRAPHY.h2, styles.title]}>Tutor IA</Text>
                <View style={{ width: 28 }} />
            </View>

            <ScrollView contentContainerStyle={styles.chatContent}>
                {messages.map((msg) => (
                    <View
                        key={msg.id}
                        style={[
                            styles.bubble,
                            msg.sender === 'user' ? styles.userBubble : styles.aiBubble
                        ]}
                    >
                        <Text style={[
                            styles.messageText,
                            msg.sender === 'user' ? styles.userText : styles.aiText
                        ]}>
                            {msg.text}
                        </Text>
                    </View>
                ))}
            </ScrollView>

            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                keyboardVerticalOffset={100}
            >
                <View style={styles.inputContainer}>
                    <TextInput
                        style={styles.input}
                        placeholder="Escribe en euskera..."
                        value={message}
                        onChangeText={setMessage}
                    />
                    <TouchableOpacity style={styles.sendButton} onPress={handleSend}>
                        <Send color={COLORS.white} size={20} />
                    </TouchableOpacity>
                </View>
            </KeyboardAvoidingView>
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
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: SPACING.md,
        backgroundColor: COLORS.white,
        elevation: 2,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 1 },
        shadowOpacity: 0.1,
        shadowRadius: 2,
    },
    title: {
        color: COLORS.primary,
    },
    chatContent: {
        padding: SPACING.md,
    },
    bubble: {
        maxWidth: '80%',
        padding: SPACING.md,
        borderRadius: 20,
        marginBottom: SPACING.md,
    },
    userBubble: {
        alignSelf: 'flex-end',
        backgroundColor: COLORS.primary,
        borderBottomRightRadius: 4,
    },
    aiBubble: {
        alignSelf: 'flex-start',
        backgroundColor: COLORS.surface,
        borderBottomLeftRadius: 4,
    },
    messageText: {
        fontSize: 16,
    },
    userText: {
        color: COLORS.white,
    },
    aiText: {
        color: COLORS.text,
    },
    inputContainer: {
        flexDirection: 'row',
        padding: SPACING.md,
        borderTopWidth: 1,
        borderTopColor: '#EEE',
        backgroundColor: COLORS.white,
    },
    input: {
        flex: 1,
        backgroundColor: COLORS.surface,
        borderRadius: 24,
        paddingHorizontal: SPACING.lg,
        paddingVertical: 10,
        marginRight: SPACING.md,
        fontSize: 16,
    },
    sendButton: {
        backgroundColor: COLORS.primary,
        width: 44,
        height: 44,
        borderRadius: 22,
        alignItems: 'center',
        justifyContent: 'center',
    }
});
