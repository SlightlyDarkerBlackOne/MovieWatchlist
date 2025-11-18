export interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]> | string[];
  title?: string;
  status?: number;
}

export interface TransformedApiError {
  message: string;
  status: number | string;
  endpoint: string;
  timestamp: number;
  originalError: unknown;
  retryable: boolean;
}


