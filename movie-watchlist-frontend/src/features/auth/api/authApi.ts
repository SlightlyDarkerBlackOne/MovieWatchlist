import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryWithReauth } from '../../../shared/api/baseApi';
import { API_ENDPOINTS, HTTP_METHODS, RTK_REDUCER_PATHS, RTK_TAG_TYPES, SUCCESS_MESSAGES } from '../../../shared/constants/appConstants';
import {
  LoginCredentials,
  RegisterData,
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  UserInfo,
} from '../model/auth.types';
interface LoginResponse {
  user: UserInfo;
  expiresAt: string;
}

interface RegisterResponse {
  user: UserInfo;
  expiresAt: string;
}

export const authApi = createApi({
  reducerPath: RTK_REDUCER_PATHS.AUTH_API,
  baseQuery: baseQueryWithReauth,
  tagTypes: [RTK_TAG_TYPES.AUTH],
  endpoints: (builder) => ({
    login: builder.mutation<AuthenticationResult, LoginCredentials>({
      query: (credentials) => ({
        url: API_ENDPOINTS.AUTH.LOGIN,
        method: HTTP_METHODS.POST,
        body: credentials,
      }),
      transformResponse: (response: LoginResponse): AuthenticationResult => ({
        isSuccess: true,
        user: response.user,
        expiresAt: response.expiresAt,
      }),
    }),
    register: builder.mutation<AuthenticationResult, RegisterData>({
      query: (userData) => ({
        url: API_ENDPOINTS.AUTH.REGISTER,
        method: HTTP_METHODS.POST,
        body: userData,
      }),
      transformResponse: (response: RegisterResponse): AuthenticationResult => ({
        isSuccess: true,
        user: response.user,
        expiresAt: response.expiresAt,
      }),
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: API_ENDPOINTS.AUTH.LOGOUT,
        method: HTTP_METHODS.POST,
      }),
    }),
    forgotPassword: builder.mutation<PasswordResetResponse, ForgotPasswordData>({
      query: (data) => ({
        url: API_ENDPOINTS.AUTH.FORGOT_PASSWORD,
        method: HTTP_METHODS.POST,
        body: data,
      }),
      transformResponse: (response: PasswordResetResponse): PasswordResetResponse => ({
        success: true,
        message: response.message || SUCCESS_MESSAGES.PASSWORD_RESET_EMAIL_SENT,
      }),
    }),
    resetPassword: builder.mutation<PasswordResetResponse, ResetPasswordData>({
      query: (data) => ({
        url: API_ENDPOINTS.AUTH.RESET_PASSWORD,
        method: HTTP_METHODS.POST,
        body: data,
      }),
      transformResponse: (response: PasswordResetResponse): PasswordResetResponse => ({
        success: true,
        message: response.message || SUCCESS_MESSAGES.PASSWORD_RESET_SUCCESS,
      }),
    }),
    getCurrentUser: builder.query<UserInfo, void>({
      query: () => API_ENDPOINTS.AUTH.ME,
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useGetCurrentUserQuery,
  useLazyGetCurrentUserQuery,
} = authApi;


