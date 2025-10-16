/**
 * Error-related TypeScript interfaces
 */

export interface ApiError {
  response?: {
    data?: {
      message?: string;
      errors?: Record<string, string[]>;
      [key: string]: unknown;
    };
    status?: number;
  };
  message?: string;
}

export interface AxiosError extends Error {
  response?: {
    data?: {
      message?: string;
      errors?: Record<string, string[]>;
      [key: string]: unknown;
    };
    status?: number;
    statusText?: string;
  };
  request?: unknown;
  config?: unknown;
  code?: string;
  isAxiosError: boolean;
}

