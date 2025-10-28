/**
 * Custom render utilities for testing with all required providers
 */

import React, { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { AuthProvider, AuthContextType } from '../contexts/AuthContext';
import { WatchlistProvider, WatchlistContextType } from '../contexts/WatchlistContext';
import { appTheme } from '../theme';
import { moviesApi } from '../store/api/moviesApi';
import { watchlistApi } from '../store/api/watchlistApi';

interface AllProvidersProps {
  children: React.ReactNode;
}

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

const AllProviders: React.FC<AllProvidersProps> = ({ children }) => {
  const store = createTestStore();
  
  return (
    <Provider store={store}>
      <ThemeProvider theme={appTheme}>
        <BrowserRouter>
          <AuthProvider>
            <WatchlistProvider>
              {children}
            </WatchlistProvider>
          </AuthProvider>
        </BrowserRouter>
      </ThemeProvider>
    </Provider>
  );
};

/**
 * Custom render function that wraps components with all providers
 */
const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>,
) => render(ui, { wrapper: AllProviders, ...options });

// Custom render options with context mocking support
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  mockWatchlistContext?: Partial<WatchlistContextType>;
  mockAuthContext?: Partial<AuthContextType>;
}

/**
 * Render function with support for mocked contexts
 * Allows tests to provide their own context mocks while keeping other providers real
 * 
 * Note: This uses React Context directly, bypassing the useAuth/useWatchlist hooks
 * The components must be wrapped in a way that the mocked context values are available
 */
const renderWithMocks = (
  ui: ReactElement,
  options?: CustomRenderOptions
) => {
  const { mockWatchlistContext, mockAuthContext, ...renderOptions } = options || {};

  // Import the actual context objects (not the hooks)
  const AuthContextModule = require('../contexts/AuthContext');
  const WatchlistContextModule = require('../contexts/WatchlistContext');

  const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const store = createTestStore();
    
    let content = children;

    if (mockWatchlistContext) {
      const defaultWatchlistContext: WatchlistContextType = {
        watchlistMovieIds: new Set<number>(),
        isInWatchlist: jest.fn(() => false),
      };
      
      Object.assign(defaultWatchlistContext, mockWatchlistContext);
      
      const WatchlistContext = WatchlistContextModule.default;
      content = (
        <WatchlistContext.Provider value={defaultWatchlistContext}>
          {content}
        </WatchlistContext.Provider>
      );
    } else {
      content = <WatchlistProvider>{content}</WatchlistProvider>;
    }

    if (mockAuthContext) {
      const defaultAuthContext: AuthContextType = {
        user: null,
        isLoading: false,
        login: jest.fn(),
        register: jest.fn(),
        logout: jest.fn(),
        forgotPassword: jest.fn(),
        resetPassword: jest.fn(),
        validateToken: jest.fn(),
        isAuthenticated: jest.fn(() => false),
        getToken: jest.fn(() => null),
        ...mockAuthContext,
      };
      
      const AuthContext = AuthContextModule.default;
      content = (
        <AuthContext.Provider value={defaultAuthContext}>
          {content}
        </AuthContext.Provider>
      );
    } else {
      content = <AuthProvider>{content}</AuthProvider>;
    }

    return (
      <Provider store={store}>
        <ThemeProvider theme={appTheme}>
          <BrowserRouter>
            {content}
          </BrowserRouter>
        </ThemeProvider>
      </Provider>
    );
  };

  return render(ui, { wrapper: Wrapper, ...renderOptions });
};

// Re-export specific items from React Testing Library (not render)
export {
  screen,
  waitFor,
  fireEvent,
  within,
  waitForElementToBeRemoved,
  act,
} from '@testing-library/react';

// Export our custom render methods
export { customRender as render, renderWithMocks };


