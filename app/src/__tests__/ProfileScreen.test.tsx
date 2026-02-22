import React from 'react';
import { render, waitFor, fireEvent, act } from '@testing-library/react-native';
import { ProfileScreen } from '../screens/ProfileScreen';
import { AuthContext } from '../context/AuthContext';
import { NavigationContainer } from '@react-navigation/native';
import { apiService } from '../services/apiService';

// Mock apiService
jest.mock('../services/apiService', () => ({
    apiService: {
        getUser: jest.fn(),
        getUserProgress: jest.fn(),
        updateProfile: jest.fn(),
        requestAccountDeactivation: jest.fn(),
        deleteAccount: jest.fn(),
    },
}));

const mockUser = {
    id: 1,
    username: 'testuser',
    nickname: 'testnick',
    email: 'test@example.com',
    joinedAt: '2026-01-01T00:00:00Z',
    role: 'User'
};

const mockProgress = {
    xp: 500,
    level: 1,
    lastLessonTitle: 'Basics 1'
};

const MockAuthProvider = ({ children, user = mockUser }: any) => {
    const authContextValue = {
        user,
        isLoading: false,
        isAdmin: user?.role === 'Admin',
        login: jest.fn(),
        logout: jest.fn(),
        updateUser: jest.fn(),
        updateLanguage: jest.fn().mockResolvedValue({ success: true }),
    };
    return (
        <AuthContext.Provider value={authContextValue as any}>
            {children}
        </AuthContext.Provider>
    );
};

describe('ProfileScreen', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        (apiService.getUser as jest.Mock).mockResolvedValue(mockUser);
        (apiService.getUserProgress as jest.Mock).mockResolvedValue(mockProgress);
    });

    it('renders profile data correctly after loading', async () => {
        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <MockAuthProvider>
                        <ProfileScreen navigation={{ navigate: jest.fn() } as any} />
                    </MockAuthProvider>
                </NavigationContainer>
            );
        });

        const { getByText } = renderResult;

        await waitFor(() => {
            expect(getByText('testuser')).toBeTruthy();
            expect(getByText('@testnick')).toBeTruthy();
            expect(getByText('500')).toBeTruthy(); // XP
        }, { timeout: 5000 });
    });

    it('handles profile update', async () => {
        (apiService.updateProfile as jest.Mock).mockResolvedValue({ success: true });

        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <MockAuthProvider>
                        <ProfileScreen navigation={{ navigate: jest.fn() } as any} />
                    </MockAuthProvider>
                </NavigationContainer>
            );
        });

        const { getByPlaceholderText, getByText } = renderResult;

        await waitFor(() => expect(getByText('testuser')).toBeTruthy());

        const nicknameInput = getByPlaceholderText('profile.nickname');
        await act(async () => {
            fireEvent.changeText(nicknameInput, 'newnick');
        });

        const saveButton = getByText('profile.saveChanges');
        await act(async () => {
            fireEvent.press(saveButton);
        });

        await waitFor(() => {
            expect(apiService.updateProfile).toHaveBeenCalledWith(1, expect.objectContaining({
                nickname: 'newnick'
            }));
        });
    });

    it('shows logout confirmation button', async () => {
        let renderResult: any;
        await act(async () => {
            renderResult = render(
                <NavigationContainer>
                    <MockAuthProvider>
                        <ProfileScreen navigation={{ reset: jest.fn() } as any} />
                    </MockAuthProvider>
                </NavigationContainer>
            );
        });

        const { getByText } = renderResult;

        await waitFor(() => expect(getByText('testuser')).toBeTruthy());

        expect(getByText('profile.logout')).toBeTruthy();
    });
});
