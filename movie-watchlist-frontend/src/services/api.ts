import axios, { AxiosError } from 'axios';
import { APP_CONFIG, API_ENDPOINTS } from '../utils/constants';
import { extractErrorMessage } from '../utils/errorHandler';

interface ApiError extends Error {
  status?: number;
  originalError?: AxiosError;
}

let navigateHandler: ((path: string) => void) | null = null;
let globalErrorHandler: ((message: string) => void) | null = null;

export const setGlobalErrorHandler = (handler: (message: string) => void) => {
  globalErrorHandler = handler;
};

export const setNavigateHandler = (handler: (path: string) => void) => {
  navigateHandler = handler;
};

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5250/api',
  timeout: APP_CONFIG.API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Do not inject Authorization header; cookies are sent automatically
api.interceptors.request.use(
  (config) => config,
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    if (error.response?.status === 401) {
      const isAuthEndpoint = error.config?.url?.includes('/Auth/');
      if (!isAuthEndpoint) {
        const refreshed = await attemptSilentRefresh();
        if (refreshed) {
          return api.request(error.config!);
        }
        if (globalErrorHandler) {
          globalErrorHandler('Your session has expired. Please log in again.');
        }
        if (navigateHandler) {
          navigateHandler('/login');
        }
      }
    } else if (error.response?.status === 500 || error.response?.status === 503) {
      if (globalErrorHandler) {
        globalErrorHandler('Server error. Please try again later.');
      }
    } else if (!error.response && error.request) {
      if (globalErrorHandler) {
        globalErrorHandler('Network error. Please check your connection.');
      }
    }

    const enhancedError = new Error(extractErrorMessage(error)) as ApiError;
    enhancedError.name = 'ApiError';
    enhancedError.status = error.response?.status;
    enhancedError.originalError = error;
    return Promise.reject(enhancedError);
  }
);

async function attemptSilentRefresh(): Promise<boolean> {
  try {
    await api.post(API_ENDPOINTS.AUTH.REFRESH);
    return true;
  } catch {
    return false;
  }
}

export default api;
