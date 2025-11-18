/**
 * Tests for App component
 */

import React from 'react';
import { render as rtlRender, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import App from './App';
import { AuthProvider } from './features/auth/contexts/AuthContext';
import { ErrorProvider } from './shared/contexts/ErrorContext';
import { appTheme } from './shared/theme';
import { baseApiSlice } from './shared/api/baseApiSlice';
import * as authApi from './features/auth/api/authApi';

// Mock child components to avoid deep rendering
jest.mock('./routes/AppRoutes', () => {
  return function MockAppRoutes() {
    return <div data-testid="app-routes">App Routes</div>;
  };
});

jest.mock('./shared/components/common/LoadingSpinner', () => {
  return function MockLoadingSpinner() {
    return <div data-testid="loading-spinner">Loading...</div>;
  };
});

jest.mock('./shared/api/baseApi', () => ({
  setNavigateHandler: jest.fn(),
  setGlobalErrorHandler: jest.fn(),
  baseQueryWithReauth: jest.fn().mockResolvedValue({ data: {} }),
}));

jest.mock('./features/auth/api/authApi', () => {
  const actual = jest.requireActual('./features/auth/api/authApi');
  return {
    ...actual,
    useGetCurrentUserQuery: jest.fn(),
    useLoginMutation: jest.fn(),
    useRegisterMutation: jest.fn(),
    useLogoutMutation: jest.fn(),
    useForgotPasswordMutation: jest.fn(),
    useResetPasswordMutation: jest.fn(),
  };
});

const createTestStore = () => {
  return configureStore({
    reducer: {
      [baseApiSlice.reducerPath]: baseApiSlice.reducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(baseApiSlice.middleware),
  });
};

// Custom render without Router since App already includes BrowserRouter
const render = (ui: React.ReactElement) => {
  const store = createTestStore();
  return rtlRender(
    <Provider store={store}>
      <ThemeProvider theme={appTheme}>
        <ErrorProvider>
          <AuthProvider>
            {ui}
          </AuthProvider>
        </ErrorProvider>
      </ThemeProvider>
    </Provider>
  );
};

describe('App', () => {
  const createMockMutation = () => {
    const mockFn = jest.fn() as any;
    mockFn.unwrap = jest.fn().mockResolvedValue({ isSuccess: true });
    return mockFn;
  };

  beforeEach(() => {
    jest.clearAllMocks();

    (authApi.useGetCurrentUserQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    });

    (authApi.useLoginMutation as jest.Mock).mockReturnValue([
      createMockMutation(),
      { isLoading: false },
    ]);

    (authApi.useRegisterMutation as jest.Mock).mockReturnValue([
      createMockMutation(),
      { isLoading: false },
    ]);

    (authApi.useLogoutMutation as jest.Mock).mockReturnValue([
      createMockMutation(),
      { isLoading: false },
    ]);

    (authApi.useForgotPasswordMutation as jest.Mock).mockReturnValue([
      createMockMutation(),
      { isLoading: false },
    ]);

    (authApi.useResetPasswordMutation as jest.Mock).mockReturnValue([
      createMockMutation(),
      { isLoading: false },
    ]);
  });

  it('should render without crashing', () => {
    const { container } = render(<App />);
    expect(container).toBeInTheDocument();
  });

  it('should wrap app with ThemeProvider and BrowserRouter', async () => {
    const { getByTestId } = render(<App />);
    
    // Should eventually render routes (after auth check)
    await waitFor(() => {
      expect(getByTestId('app-routes')).toBeInTheDocument();
    });
  });
});

