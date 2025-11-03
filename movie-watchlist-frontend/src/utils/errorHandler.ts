import { AxiosError } from '../types/error.types';

export interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]> | string[];
  title?: string;
  status?: number;
}

export function extractErrorMessage(error: unknown): string {
  if (!error) return 'An unknown error occurred';

  const axiosError = error as AxiosError;
  
  if (axiosError.response?.data) {
    const data = axiosError.response.data as ApiErrorResponse;
    
    if (data.errors) {
      if (Array.isArray(data.errors)) {
        return data.errors.join(', ');
      }
      const errorMessages = Object.values(data.errors).flat();
      return errorMessages.join(', ');
    }
    
    if (data.message) {
      return data.message;
    }
    
    if (data.title) {
      return data.title;
    }
    
    if (typeof data === 'string') {
      return data;
    }
  }
  
  if (axiosError.message) {
    return axiosError.message;
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return String(error);
}

export function createApiError(error: unknown, defaultMessage: string): Error {
  const message = extractErrorMessage(error) || defaultMessage;
  return new Error(message);
}

