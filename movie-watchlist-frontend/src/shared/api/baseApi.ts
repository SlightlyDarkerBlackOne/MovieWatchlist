import { fetchBaseQuery, BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import {
  API_ENDPOINTS,
  ERROR_MESSAGES,
  RTK_QUERY_ERROR_STATUS,
  HTTP_STATUS_CODES,
  ROUTE_PATHS,
  HTTP_HEADERS,
  HTTP_HEADER_VALUES,
  FETCH_CREDENTIALS,
  API_ENDPOINT_PATTERNS,
  DEFAULT_API_CONFIG,
  TYPE_OF_VALUES,
  RETRY_CONFIG,
} from '../constants/appConstants';
import { retryWithBackoff, shouldRetry } from '../lib/retryUtils';
import { transformError } from '../lib/errorHandler';

let navigateHandler: ((path: string) => void) | null = null;
let globalErrorHandler: ((message: string) => void) | null = null;

export const setGlobalErrorHandler = (handler: (message: string) => void) => {
  globalErrorHandler = handler;
};

export const setNavigateHandler = (handler: (path: string) => void) => {
  navigateHandler = handler;
};

const baseQuery = fetchBaseQuery({
  baseUrl: process.env.REACT_APP_API_URL || DEFAULT_API_CONFIG.BASE_URL,
  credentials: FETCH_CREDENTIALS.INCLUDE,
  prepareHeaders: (headers) => {
    headers.set(HTTP_HEADERS.CONTENT_TYPE, HTTP_HEADER_VALUES.APPLICATION_JSON);
    return headers;
  },
});

/**
 * Extracts endpoint URL from args
 */
function getEndpointUrl(args: string | FetchArgs): string {
  if (typeof args === TYPE_OF_VALUES.STRING) {
    return args as string;
  }
  return ((args as FetchArgs).url ?? '') as string;
}

export const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  const endpoint = getEndpointUrl(args);
  const isAuthEndpoint = endpoint.includes(API_ENDPOINT_PATTERNS.AUTH);

  // Execute query with retry logic for retryable errors
  let result: Awaited<ReturnType<typeof baseQuery>>;
  
  try {
    result = await retryWithBackoff(
      async () => {
        const queryResult = await baseQuery(args, api, extraOptions);
        
        if (!queryResult) {
          return {
            error: {
              status: RTK_QUERY_ERROR_STATUS.FETCH_ERROR,
              error: ERROR_MESSAGES.UNEXPECTED_ERROR,
            },
            data: undefined,
          };
        }
        
        // If error is retryable, throw to trigger retry
        if (queryResult.error && shouldRetry(queryResult, isAuthEndpoint)) {
          throw queryResult.error;
        }
        
        return queryResult;
      },
      {
        maxRetries: RETRY_CONFIG.MAX_RETRIES,
        baseDelayMs: RETRY_CONFIG.RETRY_DELAY_BASE_MS,
      }
    );
  } catch (error) {
    // If retry failed, return error result
    result = {
      error: error as FetchBaseQueryError,
      data: undefined,
    };
  }

  if (!result) {
    return {
      error: {
        status: RTK_QUERY_ERROR_STATUS.FETCH_ERROR,
        error: ERROR_MESSAGES.UNEXPECTED_ERROR,
      },
      data: undefined,
    };
  }

  if (result.error) {
    const errorStatus = result.error.status;

    // Handle 401 unauthorized with token refresh
    if (errorStatus === HTTP_STATUS_CODES.UNAUTHORIZED && !isAuthEndpoint) {
      const refreshResult = await baseQuery(API_ENDPOINTS.AUTH.REFRESH, api, extraOptions);
      
      if (refreshResult && !refreshResult.error && refreshResult.data) {
        // Retry original query after successful refresh
        result = await baseQuery(args, api, extraOptions);
        if (!result) {
          return {
            error: {
              status: RTK_QUERY_ERROR_STATUS.FETCH_ERROR,
              error: ERROR_MESSAGES.UNEXPECTED_ERROR,
            },
            data: undefined,
          };
        }
      } else {
        // Refresh failed, redirect to login
        if (globalErrorHandler) {
          globalErrorHandler(ERROR_MESSAGES.SESSION_EXPIRED);
        }
        if (navigateHandler) {
          navigateHandler(ROUTE_PATHS.LOGIN);
        }
      }
    }

    // Transform error for consistent format and add metadata
    // transformError only returns null if error is undefined, which can't happen here
    const transformedError = transformError(result.error, endpoint);
    
    if (!transformedError) {
      return result;
    }
    
    // Show global error notifications for specific error types
    if (errorStatus === HTTP_STATUS_CODES.INTERNAL_SERVER_ERROR || errorStatus === HTTP_STATUS_CODES.SERVICE_UNAVAILABLE) {
      if (globalErrorHandler) {
        globalErrorHandler(ERROR_MESSAGES.SERVER_ERROR);
      }
    } else if (errorStatus === RTK_QUERY_ERROR_STATUS.FETCH_ERROR || errorStatus === RTK_QUERY_ERROR_STATUS.PARSING_ERROR) {
      if (globalErrorHandler) {
        globalErrorHandler(ERROR_MESSAGES.NETWORK_ERROR);
      }
    }
    
    // Store transformed error in error.data for components to access
    // ALL RTK Query errors are transformed, so components can rely on transformed data
    const errorWithTransformedData = {
      ...result.error,
      data: transformedError as unknown,
    } as FetchBaseQueryError;
    
    return {
      error: errorWithTransformedData,
      data: undefined,
      meta: result.meta,
    };
  }

  return result;
};

