import React, { useEffect } from 'react';
import { View, StyleSheet, TouchableOpacity, Text, Platform } from 'react-native';
import * as WebBrowser from 'expo-web-browser';
import * as Google from 'expo-auth-session/providers/google';
import * as Facebook from 'expo-auth-session/providers/facebook';
import * as AuthSession from 'expo-auth-session';
import { useNavigation } from '@react-navigation/native';
import { useAuth } from '../context/AuthContext';
import { useTranslation } from 'react-i18next';
import { COLORS, SPACING } from '../theme';

// Setup OAuth configuration here or use environment variables

// Setup OAuth configuration from Environment Variables
// Ensure these are set in your .env file or deployment environment
const GOOGLE_CLIENT_ID = process.env.EXPO_PUBLIC_GOOGLE_CLIENT_ID || 'your-google-client-id.apps.googleusercontent.com';
const FACEBOOK_CLIENT_ID = process.env.EXPO_PUBLIC_FACEBOOK_CLIENT_ID || 'your-facebook-app-id';

export const LoginButtons = () => {
    const { t } = useTranslation();
    const navigation = useNavigation<any>();
    const { socialLogin } = useAuth() as any; // Cast temporarily until AuthContext is updated

    // We instantiate the redirect URI explicitly to avoid Expo navigating to the root Onboarding screen
    // On web, we must use the standard Expo redirect URI because Google requires exact matching URIs
    // in the Cloud Console (e.g. http://localhost:8081) and won't match http://localhost:8081/login
    const redirectUri = AuthSession.makeRedirectUri();

    // Request Google Auth Session
    const [googleRequest, googleResponse, promptGoogleAsync] = Google.useAuthRequest({
        clientId: GOOGLE_CLIENT_ID,
        redirectUri,
        prompt: AuthSession.Prompt.SelectAccount,
    });

    // Request Facebook Auth Session
    const [facebookRequest, facebookResponse, promptFacebookAsync] = Facebook.useAuthRequest({
        clientId: FACEBOOK_CLIENT_ID,
        redirectUri,
    });

    useEffect(() => {
        console.log('[Auth] redirectUri:', redirectUri);
        console.log('[Auth] Google Response:', googleResponse);
    }, [googleResponse, redirectUri]);

    // Handle Google Response
    useEffect(() => {
        const handleGoogleLogin = async () => {
            if (googleResponse?.type === 'success' && googleResponse.authentication?.accessToken) {
                console.log("Success! Extracted Token:", googleResponse.authentication.accessToken);
                const result = await socialLogin('google', googleResponse.authentication.accessToken);
                if (result.success) {
                    navigation.reset({
                        index: 0,
                        routes: [{ name: 'Home' as never }],
                    });
                } else {
                    console.error("Social login failed:", result.error);
                }
            } else if (googleResponse?.type === 'error') {
                console.error("Google Auth Error:", googleResponse.error);
            }
        };
        handleGoogleLogin();
    }, [googleResponse]);

    // Handle Facebook Response
    useEffect(() => {
        const handleFacebookLogin = async () => {
            if (facebookResponse?.type === 'success' && facebookResponse.authentication?.accessToken) {
                const result = await socialLogin('facebook', facebookResponse.authentication.accessToken);
                if (result.success) {
                    navigation.reset({
                        index: 0,
                        routes: [{ name: 'Home' as never }],
                    });
                } else {
                    console.error("Social login failed:", result.error);
                }
            }
        };
        handleFacebookLogin();
    }, [facebookResponse]);

    return (
        <View style={styles.container}>
            <Text style={styles.divider}>{t('login.orSocial', 'Or connect with')}</Text>

            <TouchableOpacity
                style={[styles.socialButton, styles.googleButton]}
                onPress={() => promptGoogleAsync()}
                disabled={!googleRequest}
            >
                <Text style={styles.googleText}>Continuar con Google</Text>
            </TouchableOpacity>

            <TouchableOpacity
                style={[styles.socialButton, styles.facebookButton]}
                onPress={() => promptFacebookAsync()}
                disabled={!facebookRequest}
            >
                <Text style={styles.facebookText}>Continuar con Facebook</Text>
            </TouchableOpacity>
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        marginTop: SPACING.md,
        width: '100%',
        gap: SPACING.sm,
    },
    divider: {
        textAlign: 'center',
        marginVertical: SPACING.md,
        color: COLORS.textSecondary,
    },
    socialButton: {
        padding: SPACING.md,
        borderRadius: 12,
        alignItems: 'center',
        justifyContent: 'center',
        borderWidth: 1,
    },
    googleButton: {
        backgroundColor: '#FFFFFF',
        borderColor: '#E0E0E0',
    },
    googleText: {
        color: '#757575',
        fontSize: 16,
        fontWeight: 'bold',
    },
    facebookButton: {
        backgroundColor: '#1877F2',
        borderColor: '#1877F2',
    },
    facebookText: {
        color: '#FFFFFF',
        fontSize: 16,
        fontWeight: 'bold',
    }
});
