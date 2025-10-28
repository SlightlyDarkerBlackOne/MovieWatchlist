/**
 * Custom render utilities for testing with all required providers
 */

import React, { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { AuthProvider, AuthContextType } from '../contexts/AuthContext';
import { WatchlistProvider, WatchlistContextType } from '../contexts/WatchlistContext';
import { appTheme } from '../theme';

interface AllProvidersProps {
  children: React.ReactNode;
}

const AllProviders: React.FC<AllProvidersProps> = ({ children }) => {
  return (
    <ThemeProvider theme={appTheme}>
      <BrowserRouter>
        <AuthProvider>
          <WatchlistProvider>
            {children}
          </WatchlistProvider>
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
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
    // Build wrapper with conditional mocking
    let content = children;

    // Wrap with WatchlistProvider or mocked context
    if (mockWatchlistContext) {
      const defaultWatchlistContext: WatchlistContextType = {
        watchlistMovieIds: [],
        isInWatchlist: jest.fn(() => false),
        ...mockWatchlistContext,
      };
      
      // Use the default export context from WatchlistContext
      const WatchlistContext = WatchlistContextModule.default;
      content = (
        <WatchlistContext.Provider value={defaultWatchlistContext}>
          {content}
        </WatchlistContext.Provider>
      );
    } else {
      content = <WatchlistProvider>{content}</WatchlistProvider>;
    }

    // Wrap with AuthProvider or mocked context
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
      
      // Use the default export context from AuthContext
      const AuthContext = AuthContextModule.default;
      content = (
        <AuthContext.Provider value={defaultAuthContext}>
          {content}
        </AuthContext.Provider>
      );
    } else {
      content = <AuthProvider>{content}</AuthProvider>;
    }

    // Wrap with ThemeProvider and BrowserRouter
    return (
      <ThemeProvider theme={appTheme}>
        <BrowserRouter>
          {content}
        </BrowserRouter>
      </ThemeProvider>
    );
  };

  return render(ui, { wrapper: Wrapper, ...renderOptions });
};

// Re-export everything from React Testing Library
export * from '@testing-library/react';

// Override render method
export { customRender as render, renderWithMocks };


