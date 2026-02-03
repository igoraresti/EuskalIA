import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { OnboardingScreen } from '../screens/OnboardingScreen';
import { HomeScreen } from '../screens/HomeScreen';
import { LessonScreen } from '../screens/LessonScreen';
import { AIChatScreen } from '../screens/AIChatScreen';
import { LeaderboardScreen } from '../screens/LeaderboardScreen';
import { ProfileScreen } from '../screens/ProfileScreen';

const Stack = createNativeStackNavigator();

export const AppNavigation = () => {
    return (
        <NavigationContainer>
            <Stack.Navigator
                initialRouteName="Onboarding"
                screenOptions={{
                    headerShown: false,
                    animation: 'slide_from_right',
                }}
            >
                <Stack.Screen name="Onboarding" component={OnboardingScreen} />
                <Stack.Screen name="Home" component={HomeScreen} />
                <Stack.Screen name="Lesson" component={LessonScreen} />
                <Stack.Screen name="AIChat" component={AIChatScreen} />
                <Stack.Screen name="Leaderboard" component={LeaderboardScreen} />
                <Stack.Screen name="Profile" component={ProfileScreen} />
            </Stack.Navigator>
        </NavigationContainer>
    );
};
