import React from 'react';
import { render, fireEvent, waitFor, act } from '@testing-library/react-native';
import { LoginScreen } from '../screens/LoginScreen';
import { AuthProvider } from '../context/AuthContext';
import { NavigationContainer } from '@react-navigation/native';
import { apiService } from '../services/apiService';

// Mock apiService
jest.mock('../services/apiService', () => ({
    apiService: {
        login: jest.fn(),
    },
}));

describe('LoginScreen', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('renders correctly', async () => {
        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <AuthProvider>
                        <LoginScreen />
                    </AuthProvider>
                </NavigationContainer>
            );
        });

        const { getByPlaceholderText, getByText } = renderResult;
        expect(getByPlaceholderText('register.name')).toBeTruthy();
        expect(getByPlaceholderText('register.password')).toBeTruthy();
        expect(getByText('common.login')).toBeTruthy();
    });

    it('shows error on empty fields', async () => {
        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <AuthProvider>
                        <LoginScreen />
                    </AuthProvider>
                </NavigationContainer>
            );
        });

        const { getByText } = renderResult;
        await act(async () => {
            fireEvent.press(getByText('common.login'));
        });
        expect(apiService.login).not.toHaveBeenCalled();
    });

    it('navigates to Home on successful login', async () => {
        (apiService.login as jest.Mock).mockResolvedValueOnce({
            token: 'mock-jwt-token',
            user: { id: 1, username: 'testuser', role: 'User' }
        });

        const mockNavigate = jest.fn();
        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <AuthProvider>
                        <LoginScreen navigation={{ replace: mockNavigate } as any} />
                    </AuthProvider>
                </NavigationContainer>
            );
        });

        const { getByPlaceholderText, getByText } = renderResult;

        await act(async () => {
            fireEvent.changeText(getByPlaceholderText('register.name'), 'testuser');
            fireEvent.changeText(getByPlaceholderText('register.password'), 'password');
            fireEvent.press(getByText('common.login'));
        });

        await waitFor(() => {
            expect(apiService.login).toHaveBeenCalledWith({
                username: 'testuser',
                password: 'password',
            });
            expect(mockNavigate).toHaveBeenCalledWith('Home');
        });
    });
});
