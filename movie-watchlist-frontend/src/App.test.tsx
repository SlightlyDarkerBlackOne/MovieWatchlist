/**
 * Tests for App component
 */

import React from 'react';
import { render as rtlRender, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import App from './App';
import { AuthProvider } from './contexts/AuthContext';
import { ErrorProvider } from './contexts/ErrorContext';
import { appTheme } from './theme';
import { moviesApi } from './store/api/moviesApi';
import { watchlistApi } from './store/api/watchlistApi';

// Mock child components to avoid deep rendering
jest.mock('./routes/AppRoutes', () => {
  return function MockAppRoutes() {
    return <div data-testid="app-routes">App Routes</div>;
  };
});

jest.mock('./components/common/LoadingSpinner', () => {
  return function MockLoadingSpinner() {
    return <div data-testid="loading-spinner">Loading...</div>;
  };
});

jest.mock('./services/api', () => ({
  setNavigateHandler: jest.fn(),
  setGlobalErrorHandler: jest.fn(),
}));

const createTestStore = () => {
  return configureStore({
    reducer: {
      [moviesApi.reducerPath]: moviesApi.reducer,
      [watchlistApi.reducerPath]: watchlistApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(moviesApi.middleware, watchlistApi.middleware),
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

