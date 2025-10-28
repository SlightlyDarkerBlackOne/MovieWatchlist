import axios from 'axios';
import { STORAGE_KEYS, APP_CONFIG } from '../utils/constants';

// Navigation helper for redirects outside React components
let navigateHandler: ((path: string) => void) | null = null;

export const setNavigateHandler = (handler: (path: string) => void) => {
  navigateHandler = handler;
};

// Create axios instance with base configuration
const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5250/api',
  timeout: APP_CONFIG.API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding auth token
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

// Response interceptor for handling errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Don't redirect if we're already on the login endpoint
      const isLoginEndpoint = error.config?.url?.includes('/Auth/login');
      if (!isLoginEndpoint) {
        // Token expired or invalid - clear tokens
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
        
        // Navigate to login using React Router
        if (navigateHandler) {
          navigateHandler('/login');
        }
      }
    }
    return Promise.reject(error);
  }
);

export default api;
