import { FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { SerializedError } from '@reduxjs/toolkit';
import { ERROR_MESSAGES, RTK_QUERY_ERROR_STATUS, ERROR_PROPERTIES, TYPE_OF_VALUES, API_ENDPOINT_PATTERNS } from '../constants/appConstants';
import { TransformedApiError } from '../types/error.types';
import { shouldRetry } from './retryUtils';

export interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]> | string[];
  title?: string;
  status?: number;
}

function isFetchBaseQueryError(error: unknown): error is FetchBaseQueryError {
  if (typeof error !== TYPE_OF_VALUES.OBJECT || error === null) {
    return false;
  }
  
  const errorObj = error as Record<string, unknown>;
  
  return (
    ERROR_PROPERTIES.STATUS in errorObj &&
    (ERROR_PROPERTIES.DATA in errorObj || 
     errorObj.status === RTK_QUERY_ERROR_STATUS.FETCH_ERROR || 
     errorObj.status === RTK_QUERY_ERROR_STATUS.PARSING_ERROR)
  );
}

function isSerializedError(error: unknown): error is SerializedError {
  if (typeof error !== TYPE_OF_VALUES.OBJECT || error === null) {
    return false;
  }
  
  const errorObj = error as Record<string, unknown>;
  
  return (
    ERROR_PROPERTIES.MESSAGE in errorObj &&
    !(ERROR_PROPERTIES.DATA in errorObj)
  );
}

export function createApiError(error: unknown, defaultMessage: string): Error {
  const message = getErrorMessage(error) || defaultMessage;
  return new Error(message);
}

/**
 * Extracts user-friendly error message from FetchBaseQueryError
 */
function extractMessageFromError(error: FetchBaseQueryError): string {
  if (error.data) {
    const data = error.data;
    
    if (typeof data === TYPE_OF_VALUES.OBJECT && data !== null) {
      const apiError = data as ApiErrorResponse;
      
      if (apiError.errors) {
        if (Array.isArray(apiError.errors)) {
          return apiError.errors.join(', ');
        }
        const errorMessages = Object.values(apiError.errors).flat();
        return errorMessages.join(', ');
      }
      
      if (apiError.message) {
        return apiError.message;
      }
      
      if (apiError.title) {
        return apiError.title;
      }
    }
    
    if (typeof data === TYPE_OF_VALUES.STRING) {
      return data as string;
    }
  }
  
  if (error.status === RTK_QUERY_ERROR_STATUS.FETCH_ERROR) {
    return ERROR_MESSAGES.NETWORK_ERROR;
  }
  
  if (error.status === RTK_QUERY_ERROR_STATUS.PARSING_ERROR) {
    return ERROR_MESSAGES.PARSING_ERROR;
  }
  
  return ERROR_MESSAGES.UNKNOWN_ERROR;
}

/**
 * Transforms an RTK Query error into a standardized format with metadata
 * @param error - RTK Query error result
 * @param endpoint - API endpoint that failed
 * @returns Transformed error with consistent structure and metadata
 */
export function transformError(
  error: FetchBaseQueryError | undefined,
  endpoint: string
): TransformedApiError | null {
  if (!error) return null;
  
  const errorStatus = error.status;
  const isAuthEndpoint = endpoint.includes(API_ENDPOINT_PATTERNS.AUTH);
  const retryable = shouldRetry({ error }, isAuthEndpoint);
  
  const message = extractMessageFromError(error);
  
  return {
    message,
    status: errorStatus,
    endpoint,
    timestamp: Date.now(),
    originalError: error,
    retryable,
  };
}

/**
 * Extracts error message from transformed error
 * @param error - RTK Query error (always transformed) or other error types
 * @returns User-friendly error message
 */
export function getErrorMessage(error: unknown): string {
  if (!error) return ERROR_MESSAGES.UNKNOWN_ERROR;
  
  // All RTK Query errors are transformed in baseApi and stored in error.data
  if (isFetchBaseQueryError(error) && error.data) {
    const data = error.data;
    if (typeof data === TYPE_OF_VALUES.OBJECT && data !== null) {
      const transformedError = data as TransformedApiError;
      if (transformedError.message && transformedError.endpoint) {
        return transformedError.message;
      }
    }
  }
  
  // Handle non-RTK Query errors (Error instances, SerializedError, etc.)
  if (isSerializedError(error)) {
    if (error.message) {
      return error.message;
    }
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return String(error);
}

