/**
 * Application Color Palette
 * Centralized color definitions for consistent theming across the app
 */

export const colors = {
  // Primary Brand Colors
  primary: {
    main: '#1976d2',
    light: '#42a5f5',
    dark: '#1565c0',
    contrastText: '#ffffff',
  },

  // IMDb-inspired Colors
  imdb: {
    yellow: '#f5c518',
    yellowDark: '#e6b800',
    headerDark: '#1f1f1f',
    backgroundLight: '#f5f5f5',
    backgroundDark: '#121212',
  },

  // Neutral Colors
  neutral: {
    white: '#ffffff',
    black: '#000000',
    gray50: '#fafafa',
    gray100: '#f5f5f5',
    gray200: '#eeeeee',
    gray300: '#e0e0e0',
    gray400: '#bdbdbd',
    gray500: '#9e9e9e',
    gray600: '#757575',
    gray700: '#616161',
    gray800: '#424242',
    gray900: '#212121',
  },

  // Semantic Colors
  success: {
    main: '#2e7d32',
    light: '#4caf50',
    dark: '#1b5e20',
  },

  error: {
    main: '#d32f2f',
    light: '#ef5350',
    dark: '#c62828',
  },

  warning: {
    main: '#ed6c02',
    light: '#ff9800',
    dark: '#e65100',
  },

  info: {
    main: '#0288d1',
    light: '#03a9f4',
    dark: '#01579b',
  },

  // Overlay Colors (with transparency)
  overlay: {
    dark: 'rgba(0, 0, 0, 0.6)',
    darker: 'rgba(0, 0, 0, 0.8)',
    light: 'rgba(255, 255, 255, 0.1)',
    lighter: 'rgba(255, 255, 255, 0.2)',
  },
} as const;

export type ColorKey = keyof typeof colors;

