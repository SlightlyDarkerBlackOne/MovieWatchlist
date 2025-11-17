import { FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { SerializedError } from '@reduxjs/toolkit';
import { ERROR_MESSAGES, RTK_QUERY_ERROR_STATUS, ERROR_PROPERTIES, TYPE_OF_VALUES } from '../constants/appConstants';

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

export function extractErrorMessage(error: unknown): string {
  if (!error) return ERROR_MESSAGES.UNKNOWN_ERROR;

  if (isFetchBaseQueryError(error)) {
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
  }

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

export function createApiError(error: unknown, defaultMessage: string): Error {
  const message = extractErrorMessage(error) || defaultMessage;
  return new Error(message);
}

