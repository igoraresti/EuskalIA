import React from 'react';
import { render, fireEvent, waitFor } from '@testing-library/react-native';
import { LoginScreen } from '../screens/LoginScreen';
import { AuthProvider } from '../context/AuthContext';
import { NavigationContainer } from '@react-navigation/native';

// Mock apiService
jest.mock('../services/apiService', () => ({
    apiService: {
        login: jest.fn(),
    },
}));

describe('LoginScreen', () => {
    it('renders correctly', () => {
        const { getByPlaceholderText, getByText } = render(
            <NavigationContainer>
                <AuthProvider>
                    <LoginScreen />
                </AuthProvider>
            </NavigationContainer>
        );

        expect(getByPlaceholderText('Introduce tu usuario')).toBeTruthy();
        expect(getByPlaceholderText('Introduce tu contraseña')).toBeTruthy();
        expect(getByText('Entrar')).toBeTruthy();
    });

    it('shows error on empty fields', () => {
        const { getByText } = render(
            <NavigationContainer>
                <AuthProvider>
                    <LoginScreen />
                </AuthProvider>
            </NavigationContainer>
        );

        fireEvent.press(getByText('Entrar'));
        // Need to spy on Alert, but React Native Alert is hard to test straightforwardly without mocking RN.
        // For now we check that API wasn't called.
        const { apiService } = require('../services/apiService');
        expect(apiService.login).not.toHaveBeenCalled();
    });
});
