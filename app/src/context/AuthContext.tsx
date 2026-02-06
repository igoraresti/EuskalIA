import React, { createContext, useState, useContext, useEffect } from 'react';
import { apiService } from '../services/apiService';
import { Alert, Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

interface AuthContextType {
    user: any | null;
    login: (username: string, password: string) => Promise<{ success: boolean; error?: string }>;
    logout: () => void;
    updateUser: (userData: any) => void;
    isLoading: boolean;
}

const AuthContext = createContext<AuthContextType>({
    user: null,
    login: async () => ({ success: false }),
    logout: () => { },
    updateUser: () => { },
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
                    setUser(JSON.parse(jsonValue));
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
            try {
                await AsyncStorage.setItem('@user', JSON.stringify(result));
            } catch (e) {
                console.error("Error saving user", e);
            }
            return { success: true };
        }
        return { success: false, error: result?.error };
    };

    const logout = async () => {
        setUser(null);
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

    return (
        <AuthContext.Provider value={{ user, login, logout, updateUser, isLoading }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
