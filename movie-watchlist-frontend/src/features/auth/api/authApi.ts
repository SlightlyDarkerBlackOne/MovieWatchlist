import { API_ENDPOINTS, HTTP_METHODS, RTK_TAG_TYPES, SUCCESS_MESSAGES } from '../../../shared/constants/appConstants';
import { baseApiSlice } from '../../../shared/api/baseApiSlice';
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

export const authApi = baseApiSlice.injectEndpoints({
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
      invalidatesTags: [RTK_TAG_TYPES.AUTH],
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
      invalidatesTags: [RTK_TAG_TYPES.AUTH],
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: API_ENDPOINTS.AUTH.LOGOUT,
        method: HTTP_METHODS.POST,
      }),
      invalidatesTags: [RTK_TAG_TYPES.AUTH],
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
      providesTags: [RTK_TAG_TYPES.AUTH],
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


