import { FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { RETRY_CONFIG, RETRYABLE_STATUS_CODES, RETRYABLE_ERROR_TYPES } from '../constants/appConstants';

export interface RetryOptions {
  maxRetries?: number;
  baseDelayMs?: number;
}

/**
 * Gets base delay for retries - uses 0ms in test environment to avoid delays
 */
function getBaseDelayMs(baseDelayMs?: number): number {
  if (process.env.NODE_ENV === 'test') {
    return 0;
  }
  return baseDelayMs ?? RETRY_CONFIG.RETRY_DELAY_BASE_MS;
}

/**
 * Calculates exponential backoff delay
 * @param attemptNumber - Zero-based attempt number (0 = first retry)
 * @param baseDelayMs - Base delay in milliseconds
 * @returns Delay in milliseconds
 */
function calculateBackoffDelay(attemptNumber: number, baseDelayMs: number): number {
  return baseDelayMs * Math.pow(2, attemptNumber);
}

/**
 * Checks if an error is retryable
 * @param error - RTK Query error result
 * @param isAuthEndpoint - Whether the endpoint is an auth endpoint
 * @returns True if error should be retried
 */
function isRetryableError(
  error: FetchBaseQueryError | undefined,
  isAuthEndpoint: boolean
): boolean {
  if (!error) return false;
  
  // Don't retry auth endpoints
  if (isAuthEndpoint) return false;
  
  const errorStatus = error.status;
  
  // Check if it's a retryable RTK Query error type
  if (typeof errorStatus === 'string' && RETRYABLE_ERROR_TYPES.includes(errorStatus as typeof RETRYABLE_ERROR_TYPES[number])) {
    return true;
  }
  
  // Check if it's a retryable HTTP status code
  if (typeof errorStatus === 'number' && RETRYABLE_STATUS_CODES.includes(errorStatus as typeof RETRYABLE_STATUS_CODES[number])) {
    return true;
  }
  
  return false;
}

/**
 * Delays execution for specified milliseconds
 */
function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Retries a function with exponential backoff
 * @param fn - Function to retry
 * @param options - Retry options
 * @returns Result of the function or throws last error
 */
export async function retryWithBackoff<T>(
  fn: () => Promise<T>,
  options: RetryOptions = {}
): Promise<T> {
  const maxRetries = options.maxRetries ?? RETRY_CONFIG.MAX_RETRIES;
  const baseDelayMs = getBaseDelayMs(options.baseDelayMs);
  
  let lastError: unknown;
  
  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;
      
      // Don't retry on last attempt
      if (attempt === maxRetries) {
        throw error;
      }
      
      // Calculate delay with exponential backoff
      const delayMs = calculateBackoffDelay(attempt, baseDelayMs);
      await delay(delayMs);
    }
  }
  
  throw lastError;
}

/**
 * Checks if an error should be retried based on RTK Query error result
 * @param result - RTK Query result with potential error
 * @param isAuthEndpoint - Whether the endpoint is an auth endpoint
 * @returns True if error should be retried
 */
export function shouldRetry(
  result: { error?: FetchBaseQueryError } | undefined,
  isAuthEndpoint: boolean
): boolean {
  if (!result || !result.error) return false;
  return isRetryableError(result.error, isAuthEndpoint);
}

