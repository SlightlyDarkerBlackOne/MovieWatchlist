import { fetchBaseQuery, BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { API_ENDPOINTS } from '../../utils/constants';

let navigateHandler: ((path: string) => void) | null = null;
let globalErrorHandler: ((message: string) => void) | null = null;

export const setGlobalErrorHandler = (handler: (message: string) => void) => {
  globalErrorHandler = handler;
};

export const setNavigateHandler = (handler: (path: string) => void) => {
  navigateHandler = handler;
};

const baseQuery = fetchBaseQuery({
  baseUrl: process.env.REACT_APP_API_URL || 'http://localhost:5250/api',
  credentials: 'include',
  prepareHeaders: (headers) => {
    headers.set('Content-Type', 'application/json');
    return headers;
  },
});

export const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  let result = await baseQuery(args, api, extraOptions);

  if (result.error) {
    const errorStatus = result.error.status;
    const url = typeof args === 'string' ? args : args.url || '';
    const isAuthEndpoint = url.includes('/Auth/');

    if (errorStatus === 401 && !isAuthEndpoint) {
      const refreshResult = await baseQuery(API_ENDPOINTS.AUTH.REFRESH, api, extraOptions);
      
      if (refreshResult.data) {
        result = await baseQuery(args, api, extraOptions);
      } else {
        if (globalErrorHandler) {
          globalErrorHandler('Your session has expired. Please log in again.');
        }
        if (navigateHandler) {
          navigateHandler('/login');
        }
      }
    } else if (errorStatus === 500 || errorStatus === 503) {
      if (globalErrorHandler) {
        globalErrorHandler('Server error. Please try again later.');
      }
    } else if (errorStatus === 'FETCH_ERROR' || errorStatus === 'PARSING_ERROR') {
      if (globalErrorHandler) {
        globalErrorHandler('Network error. Please check your connection.');
      }
    }
  }

  return result;
};

