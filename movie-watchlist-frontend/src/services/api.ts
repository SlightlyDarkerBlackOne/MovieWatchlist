import axios, { AxiosError } from 'axios';
import { STORAGE_KEYS, APP_CONFIG } from '../utils/constants';
import { extractErrorMessage } from '../utils/errorHandler';

let navigateHandler: ((path: string) => void) | null = null;

export const setNavigateHandler = (handler: (path: string) => void) => {
  navigateHandler = handler;
};

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5250/api',
  timeout: APP_CONFIG.API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      const isLoginEndpoint = error.config?.url?.includes('/Auth/login');
      if (!isLoginEndpoint) {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
        
        if (navigateHandler) {
          navigateHandler('/login');
        }
      }
    }
    
    const enhancedError = new Error(extractErrorMessage(error));
    enhancedError.name = 'ApiError';
    (enhancedError as any).status = error.response?.status;
    (enhancedError as any).originalError = error;
    
    return Promise.reject(enhancedError);
  }
);

export default api;
