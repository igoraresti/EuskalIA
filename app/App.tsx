import React from 'react';
import { StatusBar } from 'expo-status-bar';
import { AppNavigation } from './src/navigation';
import { AuthProvider } from './src/context/AuthContext';
import * as WebBrowser from 'expo-web-browser';
import { Platform } from 'react-native';
import './src/i18n'; // Initialize i18n

// Only instantiate at the top level for Web as recommended by Expo
if (Platform.OS === 'web') {
  WebBrowser.maybeCompleteAuthSession();
}

export default function App() {
  return (
    <AuthProvider>
      <AppNavigation />
      <StatusBar style="auto" />
    </AuthProvider>
  );
}
