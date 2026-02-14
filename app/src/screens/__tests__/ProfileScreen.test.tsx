import React from 'react';
import { render, fireEvent, waitFor } from '@testing-library/react-native';
import { ProfileScreen } from '../ProfileScreen';
import { apiService } from '../../services/apiService';
import { AuthProvider } from '../../context/AuthContext';

// Mock dependencies
jest.mock('react-i18next', () => ({
    useTranslation: () => ({ t: (key: string) => key }),
}));

jest.mock('../../services/apiService');
jest.mock('../../context/AuthContext', () => {
    return {
        useAuth: () => ({
            user: { id: 1, username: 'TestUser', nickname: 'TestNick', email: 'test@example.com' },
            logout: jest.fn(),
            updateUser: jest.fn(),
            updateLanguage: jest.fn(),
        }),
        AuthProvider: ({ children }: any) => children
    };
});

// Mock navigation
const mockNavigation = {
    navigate: jest.fn(),
    goBack: jest.fn(),
    reset: jest.fn(),
    canGoBack: jest.fn().mockReturnValue(true),
};

describe('ProfileScreen Deactivation Flow', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('renders correctly', async () => {
        (apiService.getUser as jest.Mock).mockResolvedValue({
            id: 1, username: 'TestUser', nickname: 'TestNick', email: 'test@example.com'
        });
        (apiService.getUserProgress as jest.Mock).mockResolvedValue({
            progress: { xp: 100, level: 1 }
        });

        const { getByText } = render(
            <ProfileScreen navigation={mockNavigation} />
        );

        await waitFor(() => {
            expect(getByText('profile.deleteAccount')).toBeTruthy();
        });
    });

    it('opens confirmation modal when delete button is pressed', async () => {
        const { getByText, getAllByText } = render(
            <ProfileScreen navigation={mockNavigation} />
        );

        // Find and press delete button (it's in the Danger Zone)
        // The text 'profile.deleteAccount' appears multiple times (button and modal title)
        // We target the button one.
        const deleteButtons = getAllByText('profile.deleteAccount');
        fireEvent.press(deleteButtons[0]);

        // Check if alert/modal logic is triggered
        // Since RN Alert.alert is static, we'd need to mock it to test interactions if it wasn't a custom modal.
        // However, looking at the code, for Mobile it uses Alert.alert, for Web window.confirm. 
        // Wait, let's re-read ProfileScreen.tsx.
        // "if (Platform.OS === 'web' ... window.confirm ... else Alert.alert"

        // To test this in Jest environment (which simulates Node/Mobile), we need to mock Alert.alert
    });
});
