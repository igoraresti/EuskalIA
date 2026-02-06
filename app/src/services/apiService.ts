import axios from 'axios';
import { CONFIG } from '../config';

const BASE_URL = CONFIG.BASE_URL;

export const apiService = {
    login: async (loginDto: any) => {
        try {
            const response = await axios.post(`${BASE_URL}/users/login`, loginDto);
            return response.data;
        } catch (error: any) {
            console.error('Error logging in:', error);
            // Return error object to handle it in UI
            if (error.response) {
                return { error: error.response.data.message || 'Error en el servidor' };
            } else if (error.request) {
                return { error: 'No se pudo conectar con el servidor. Verifica tu conexión.' };
            }
            return { error: 'Error desconocido' };
        }
    },

    getLessons: async () => {
        try {
            const response = await axios.get(`${BASE_URL}/lessons`);
            return response.data;
        } catch (error) {
            console.error('Error fetching lessons:', error);
            return [];
        }
    },

    getLesson: async (id: number) => {
        try {
            const response = await axios.get(`${BASE_URL}/lessons/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching lesson ${id}:`, error);
            return null;
        }
    },

    getUserProgress: async (userId: number) => {
        try {
            const response = await axios.get(`${BASE_URL}/users/${userId}/progress`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching progress for user ${userId}:`, error);
            return null;
        }
    },

    addXP: async (userId: number, xp: number, lessonTitle?: string) => {
        try {
            const response = await axios.post(`${BASE_URL}/users/${userId}/xp`, { xp, lessonTitle }, {
                headers: { 'Content-Type': 'application/json' }
            });
            return response.data;
        } catch (error) {
            console.error(`Error adding XP for user ${userId}:`, error);
            return null;
        }
    },

    getUser: async (userId: number) => {
        try {
            const response = await axios.get(`${BASE_URL}/users/${userId}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching user ${userId}:`, error);
            return null;
        }
    },

    updateProfile: async (userId: number, data: any) => {
        try {
            const response = await axios.put(`${BASE_URL}/users/${userId}/profile`, data);
            return response.data;
        } catch (error) {
            console.error(`Error updating profile for user ${userId}:`, error);
            return null;
        }
    },

    requestDeletion: async (userId: number) => {
        try {
            const response = await axios.post(`${BASE_URL}/users/${userId}/request-deletion`);
            return response.data;
        } catch (error) {
            console.error(`Error requesting deletion for user ${userId}:`, error);
            return null;
        }
    },

    deleteAccount: async (userId: number, code: string) => {
        try {
            const response = await axios.delete(`${BASE_URL}/users/${userId}?code=${code}`);
            return response.data;
        } catch (error) {
            console.error(`Error deleting account for user ${userId}:`, error);
            return null;
        }
    },

    getWorldLeaderboard: async (period: string) => {
        try {
            const response = await axios.get(`${BASE_URL}/leaderboard/world?period=${period}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching world leaderboard:', error);
            return [];
        }
    },

    getUserLeaderboard: async (userId: number, period: string) => {
        try {
            const response = await axios.get(`${BASE_URL}/leaderboard/me/${userId}?period=${period}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching user leaderboard:', error);
            return [];
        }
    }
};
