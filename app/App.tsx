import React from 'react';
import { StatusBar } from 'expo-status-bar';
import { AppNavigation } from './src/navigation';
import { AuthProvider } from './src/context/AuthContext';
import './src/i18n'; // Initialize i18n

export default function App() {
  return (
    <AuthProvider>
      <AppNavigation />
      <StatusBar style="auto" />
    </AuthProvider>
  );
}
