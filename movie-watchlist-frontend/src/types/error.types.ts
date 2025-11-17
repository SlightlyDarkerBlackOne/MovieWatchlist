/**
 * Error-related TypeScript interfaces
 */

export interface ApiError {
  data?: {
    message?: string;
    errors?: Record<string, string[]>;
    [key: string]: unknown;
  };
  status?: number | string;
  message?: string;
}

