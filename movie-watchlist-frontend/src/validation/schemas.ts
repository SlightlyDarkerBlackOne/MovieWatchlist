import { z } from 'zod';
import { WatchlistStatus } from '../types/watchlist.types';

export const loginSchema = z.object({
  usernameOrEmail: z
    .string()
    .min(1, 'Username or email is required')
    .min(3, 'Username must be 3-50 characters')
    .max(50, 'Username must be 3-50 characters'),
  password: z.string().min(1, 'Password is required'),
});

export const registerSchema = z
  .object({
    username: z
      .string()
      .min(1, 'Username is required')
      .min(3, 'Username must be 3-50 characters')
      .max(50, 'Username must be 3-50 characters')
      .regex(/^[a-zA-Z0-9_-]{3,50}$/, 'Username must be 3-50 characters and contain only letters, numbers, underscores, and hyphens'),
    email: z
      .string()
      .min(1, 'Email is required')
      .email('Invalid email address')
      .max(100, 'Email must be at most 100 characters'),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .max(100, 'Password must be at most 100 characters')
      .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
      .regex(/[0-9]/, 'Password must contain at least one number')
      .regex(/[@$!%*?&]/, 'Password must contain at least one special character (@, $, !, %, *, ?, or &)'),
    confirmPassword: z.string().min(1, 'Please confirm your password'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

export const forgotPasswordSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address')
    .min(5, 'Email must be at least 5 characters')
    .max(100, 'Email must be at most 100 characters'),
});

export const resetPasswordSchema = z.object({
  newPassword: z.string()
    .min(8, 'Password must be at least 8 characters')
    .max(100, 'Password must be at most 100 characters')
  .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
  .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
  .regex(/[0-9]/, 'Password must contain at least one number')
  .regex(/[@$!%*?&]/, 'Password must contain at least one special character (@, $, !, %, *, ?, or &)'),
  confirmPassword: z.string()
    .min(1, 'Please confirm your password'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

export const watchlistItemSchema = z.object({
  status: z.nativeEnum(WatchlistStatus),
  notes: z.string().max(1000, 'Notes must be at most 1000 characters').optional(),
});

export type LoginSchemaType = z.infer<typeof loginSchema>;
export type RegisterSchemaType = z.infer<typeof registerSchema>;
export type ForgotPasswordSchemaType = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordSchemaType = z.infer<typeof resetPasswordSchema>;
export type WatchlistItemSchemaType = z.infer<typeof watchlistItemSchema>;

export type LoginSchema = LoginSchemaType;
export type RegisterSchema = RegisterSchemaType;
export type ForgotPasswordSchema = ForgotPasswordSchemaType;
export type ResetPasswordSchema = ResetPasswordSchemaType;
export type WatchlistItemSchema = WatchlistItemSchemaType;


