import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { CONFIG } from '../config';

const BASE_URL = `${CONFIG.BASE_URL}/euskalia`;

// Add a request interceptor to include the JWT token in all requests
axios.interceptors.request.use(
    async (config) => {
        try {
            const token = await AsyncStorage.getItem('@token');
            if (token) {
                config.headers.Authorization = `Bearer ${token}`;
            }
        } catch (e) {
            console.error('Error fetching token from storage', e);
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

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
        }
    },

    socialLogin: async (data: { provider: string, token: string }) => {
        try {
            const response = await axios.post(`${BASE_URL}/auth/social-login`, data);
            return response.data;
        } catch (error: any) {
            console.error('Error with social login:', error);
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

    getAigcExercises: async (levelId?: string) => {
        try {
            const url = levelId ? `${BASE_URL}/aigcexercises?levelId=${levelId}` : `${BASE_URL}/aigcexercises`;
            const response = await axios.get(url);
            return response.data; // Returns Array of AigcExerciseResponseDto
        } catch (error) {
            console.error('Error fetching AIGC exercises:', error);
            return [];
        }
    },

    getSessionExercises: async (levelId: string, userId: number) => {
        try {
            const response = await axios.get(`${BASE_URL}/aigcexercises/session?levelId=${levelId}&userId=${userId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching session exercises:', error);
            return [];
        }
    },

    submitExerciseAttempt: async (userId: number, exerciseId: string, isCorrect: boolean) => {
        try {
            await axios.post(`${BASE_URL}/aigcexercises/attempt`, {
                userId,
                exerciseId,
                isCorrect
            });
            return true;
        } catch (error) {
            console.error('Error submitting attempt:', error);
            return false;
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

    requestAccountDeactivation: async (userId: number) => {
        try {
            const response = await axios.post(`${BASE_URL}/users/${userId}/request-deactivation`);
            return response.data;
        } catch (error: any) {
            console.error(`Error requesting deactivation for user ${userId}:`, error);
            if (error.response) {
                return { error: error.response.data.message || 'Error al solicitar desactivación' };
            }
            return { error: 'Error de conexión' };
        }
    },

    confirmAccountDeactivation: async (token: string) => {
        try {
            const response = await axios.get(`${BASE_URL}/users/confirm-deactivation?token=${token}`);
            return response.data;
        } catch (error: any) {
            console.error('Error confirming deactivation:', error);
            if (error.response) {
                return { error: error.response.data.message || 'Error al confirmar desactivación' };
            }
            return { error: 'Error de conexión' };
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
    },

    getGlobalRank: async (userId: number) => {
        try {
            const response = await axios.get(`${BASE_URL}/leaderboard/rank/${userId}`);
            return response.data as { rank: number; total: number };
        } catch (error) {
            return null;
        }
    },

    register: async (registerDto: { username: string; email: string; password: string; language?: string }) => {
        try {
            const response = await axios.post(`${BASE_URL}/users/register`, registerDto);
            return response.data;
        } catch (error: any) {
            console.error('Error registering user:', error);
            if (error.response) {
                return { error: error.response.data.message || 'Error en el servidor' };
            } else if (error.request) {
                return { error: 'No se pudo conectar con el servidor. Verifica tu conexión.' };
            }
            return { error: 'Error desconocido' };
        }
    },

    updateLanguage: async (userId: number, language: string) => {
        try {
            const response = await axios.put(`${BASE_URL}/users/${userId}/language`, { language });
            return response.data;
        } catch (error: any) {
            console.error(`Error updating language for user ${userId}:`, error);
            if (error.response) {
                return { error: error.response.data.message || 'Error al actualizar el idioma' };
            }
            return { error: 'Error desconocido' };
        }
    },

    getAdminStats: async () => {
        try {
            const response = await axios.get(`${BASE_URL}/admin/stats`);
            return response.data;
        } catch (error) {
            console.error('Error fetching admin stats:', error);
            return null;
        }
    },

    getAdminUsers: async (filters: any) => {
        try {
            const response = await axios.get(`${BASE_URL}/admin/users`, { params: filters });
            return response.data;
        } catch (error) {
            console.error('Error fetching admin users:', error);
            return { items: [], totalCount: 0, page: 1, totalPages: 0 };
        }
    },

    toggleUserActive: async (userId: number) => {
        try {
            const response = await axios.put(`${BASE_URL}/admin/users/${userId}/toggle-active`);
            return response.data;
        } catch (error) {
            console.error(`Error toggling active status for user ${userId}:`, error);
            return null;
        }
    },

    getAdminExercises: async (filters: {
        levelId?: string; topic?: string; status?: string; search?: string;
        sortBy?: string; sortDir?: string; page?: number; pageSize?: number;
    }) => {
        try {
            const response = await axios.get(`${BASE_URL}/admin/exercises`, { params: filters });
            return response.data;
        } catch (error) {
            console.error('Error fetching admin exercises:', error);
            return { total: 0, page: 1, pageSize: 20, items: [] };
        }
    },

    getAdminExerciseStats: async (id: string) => {
        try {
            const response = await axios.get(`${BASE_URL}/admin/exercises/${id}/stats`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching exercise stats ${id}:`, error);
            return null;
        }
    },

    deleteAdminExercise: async (id: string) => {
        try {
            const response = await axios.delete(`${BASE_URL}/admin/exercises/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Error deleting exercise ${id}:`, error);
            return null;
        }
    },

    previewImport: async (exercises: any[], threshold: number) => {
        try {
            const response = await axios.post(`${BASE_URL}/admin/exercises/import`, {
                exercises,
                confirm: false,
                threshold
            });
            return response.data;
        } catch (error) {
            console.error('Error previewing import:', error);
            return null;
        }
    },

    confirmImport: async (exercises: any[], threshold: number) => {
        try {
            const response = await axios.post(`${BASE_URL}/admin/exercises/import`, {
                exercises,
                confirm: true,
                threshold
            });
            return response.data;
        } catch (error) {
            console.error('Error confirming import:', error);
            return null;
        }
    },

    bulkUpdateExerciseStatus: async (ids: string[], status: string) => {
        try {
            const response = await axios.patch(`${BASE_URL}/admin/exercises/bulk-status`, { ids, status });
            return response.data;
        } catch (error) {
            console.error('Error bulk updating exercise status:', error);
            return null;
        }
    },
};

