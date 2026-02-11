import React, { createContext, useState, useContext, useEffect } from 'react';
import { apiService } from '../services/apiService';
import { Alert, Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import i18n from '../i18n';

interface AuthContextType {
    user: any | null;
    login: (username: string, password: string) => Promise<{ success: boolean; error?: string }>;
    logout: () => void;
    updateUser: (userData: any) => void;
    updateLanguage: (language: string) => Promise<{ success: boolean; error?: string }>;
    isLoading: boolean;
}

const AuthContext = createContext<AuthContextType>({
    user: null,
    login: async () => ({ success: false }),
    logout: () => { },
    updateUser: () => { },
    updateLanguage: async () => ({ success: false }),
    isLoading: true
});

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = useState<any | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    // Check for persisted user
    useEffect(() => {
        const checkPersistedUser = async () => {
            try {
                const jsonValue = await AsyncStorage.getItem('@user');
                if (jsonValue != null) {
                    const parsedUser = JSON.parse(jsonValue);
                    setUser(parsedUser);
                    // Sync i18n with stored user language
                    if (parsedUser.language) {
                        i18n.changeLanguage(parsedUser.language);
                    }
                }
            } catch (e) {
                console.error("Error reading value", e);
            }
            setIsLoading(false);
        };
        checkPersistedUser();
    }, []);

    const login = async (username: string, password: string) => {
        setIsLoading(true);
        const result = await apiService.login({ username, password });
        setIsLoading(false);

        if (result && !result.error) {
            setUser(result);
            // Sync i18n with user language from login response
            if (result.language) {
                i18n.changeLanguage(result.language);
                if (Platform.OS === 'web') {
                    localStorage.setItem('i18nextLng', result.language);
                }
            }

            try {
                await AsyncStorage.setItem('@user', JSON.stringify(result));
            } catch (e) {
                console.error("Error saving user", e);
            }
            return { success: true };
        }

        // If the error message is a specific string from backend, we might want to map it
        let errorKey = result?.error;
        if (errorKey === 'Usuario o contraseña incorrectos') errorKey = 'invalidCredentials';
        if (errorKey === 'Por favor verifica tu correo electrónico antes de iniciar sesión') errorKey = 'unverifiedEmail';

        return { success: false, error: errorKey || 'unknownError' };
    };

    const logout = async () => {
        setUser(null);
        // Reset to default language 'es' on logout
        i18n.changeLanguage('es');
        try {
            await AsyncStorage.removeItem('@user');
        } catch (e) {
            console.error("Error removing user", e);
        }
    };

    const updateUser = async (userData: any) => {
        setUser(userData);
        try {
            await AsyncStorage.setItem('@user', JSON.stringify(userData));
        } catch (e) {
            console.error("Error updating user", e);
        }
    };

    const updateLanguage = async (newLanguage: string) => {
        if (!user) return { success: false, error: 'User not logged in' };

        const result = await apiService.updateLanguage(user.id, newLanguage);
        if (result && !result.error) {
            const updatedUser = { ...user, language: newLanguage };
            setUser(updatedUser);
            i18n.changeLanguage(newLanguage);
            if (Platform.OS === 'web') {
                localStorage.setItem('i18nextLng', newLanguage);
            }
            try {
                await AsyncStorage.setItem('@user', JSON.stringify(updatedUser));
            } catch (e) {
                console.error("Error saving updated user language", e);
            }
            return { success: true };
        }
        return { success: false, error: result?.error };
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, updateUser, updateLanguage, isLoading }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
