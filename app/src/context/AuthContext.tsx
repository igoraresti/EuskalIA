import React, { createContext, useState, useContext, useEffect } from 'react';
import { apiService } from '../services/apiService';
import { Alert, Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import i18n from '../i18n';
import { registerForPushNotificationsAsync } from '../services/notificationService';

interface AuthContextType {
    user: any | null;
    login: (username: string, password: string) => Promise<{ success: boolean; error?: string }>;
    socialLogin: (provider: string, token: string) => Promise<{ success: boolean; error?: string }>;
    logout: () => void;
    updateUser: (userData: any) => void;
    updateLanguage: (language: string) => Promise<{ success: boolean; error?: string }>;
    isLoading: boolean;
    isAdmin: boolean;
}

export const AuthContext = createContext<AuthContextType>({
    user: null,
    login: async () => ({ success: false }),
    socialLogin: async () => ({ success: false }),
    logout: () => { },
    updateUser: () => { },
    updateLanguage: async () => ({ success: false }),
    isLoading: true,
    isAdmin: false
});

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = useState<any | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    // Check for persisted user
    useEffect(() => {
        const checkPersistedUser = async () => {
            try {
                const jsonValue = await AsyncStorage.getItem('@user');
                const token = await AsyncStorage.getItem('@token');
                console.log('Persisted user state:', { hasUser: !!jsonValue, hasToken: !!token });
                if (jsonValue != null) {
                    const parsedUser = JSON.parse(jsonValue);
                    setUser(parsedUser);
                    // Sync i18n with stored user language
                    if (parsedUser.language) {
                        i18n.changeLanguage(parsedUser.language);
                    }
                    // Register for push notifications if we have a user
                    registerForPushNotificationsAsync(parsedUser.id);
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
        console.log('Login API response:', result);
        setIsLoading(false);

        const token = result?.token || result?.Token;
        const userData = result?.user || result?.User;

        if (result && !result.error && token) {
            setUser(userData);

            // Sync i18n with user language from login response
            if (userData.language) {
                i18n.changeLanguage(userData.language);
                if (Platform.OS === 'web') {
                    localStorage.setItem('i18nextLng', userData.language);
                }
            }

            try {
                await AsyncStorage.setItem('@user', JSON.stringify(userData));
                await AsyncStorage.setItem('@token', token);
                console.log('Successfully saved user and token to storage');
                // Register for push notifications
                registerForPushNotificationsAsync(userData.id);
            } catch (e) {
                console.error("Error saving user or token", e);
            }
            return { success: true };
        }

        // If the error message is a specific string from backend, we might want to map it
        let errorKey = result?.error;
        if (errorKey === 'Usuario o contraseña incorrectos') errorKey = 'invalidCredentials';
        if (errorKey === 'Por favor verifica tu correo electrónico antes de iniciar sesión') errorKey = 'unverifiedEmail';

        return { success: false, error: errorKey || 'unknownError' };
    };

    const socialLogin = async (provider: string, externalToken: string) => {
        setIsLoading(true);
        const result = await apiService.socialLogin({ provider, token: externalToken });
        console.log(`Social Login (${provider}) API response:`, result);
        setIsLoading(false);

        const token = result?.token || result?.Token;
        const userData = result?.user || result?.User;

        if (result && !result.error && token) {
            setUser(userData);

            if (userData.language) {
                i18n.changeLanguage(userData.language);
                if (Platform.OS === 'web') {
                    localStorage.setItem('i18nextLng', userData.language);
                }
            }

            try {
                await AsyncStorage.setItem('@user', JSON.stringify(userData));
                await AsyncStorage.setItem('@token', token);
                console.log('Successfully saved user and token to storage after social login');
                // Register for push notifications
                registerForPushNotificationsAsync(userData.id);
            } catch (e) {
                console.error("Error saving user or token", e);
            }
            return { success: true };
        }

        return { success: false, error: result?.error || 'unknownError' };
    };

    const logout = async () => {
        setUser(null);
        // Reset to default language 'es' on logout
        i18n.changeLanguage('es');
        try {
            await AsyncStorage.removeItem('@user');
            await AsyncStorage.removeItem('@token');
        } catch (e) {
            console.error("Error removing user info", e);
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

    const isAdmin = user?.role === 'Admin';

    return (
        <AuthContext.Provider value={{ user, login, socialLogin, logout, updateUser, updateLanguage, isLoading, isAdmin }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
