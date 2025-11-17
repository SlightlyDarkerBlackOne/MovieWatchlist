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
} from '../constants/appConstants';

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

export const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  let result = await baseQuery(args, api, extraOptions);

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
    let url: string;
    if (typeof args === TYPE_OF_VALUES.STRING) {
      url = args as string;
    } else {
      url = (args as FetchArgs).url ?? '';
    }
    const isAuthEndpoint = url.includes(API_ENDPOINT_PATTERNS.AUTH);

    if (errorStatus === HTTP_STATUS_CODES.UNAUTHORIZED && !isAuthEndpoint) {
      const refreshResult = await baseQuery(API_ENDPOINTS.AUTH.REFRESH, api, extraOptions);
      
      if (refreshResult && !refreshResult.error && refreshResult.data) {
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
        if (globalErrorHandler) {
          globalErrorHandler(ERROR_MESSAGES.SESSION_EXPIRED);
        }
        if (navigateHandler) {
          navigateHandler(ROUTE_PATHS.LOGIN);
        }
      }
    } else if (errorStatus === HTTP_STATUS_CODES.INTERNAL_SERVER_ERROR || errorStatus === HTTP_STATUS_CODES.SERVICE_UNAVAILABLE) {
      if (globalErrorHandler) {
        globalErrorHandler(ERROR_MESSAGES.SERVER_ERROR);
      }
    } else if (errorStatus === RTK_QUERY_ERROR_STATUS.FETCH_ERROR || errorStatus === RTK_QUERY_ERROR_STATUS.PARSING_ERROR) {
      if (globalErrorHandler) {
        globalErrorHandler(ERROR_MESSAGES.NETWORK_ERROR);
      }
    }
  }

  return result;
};

