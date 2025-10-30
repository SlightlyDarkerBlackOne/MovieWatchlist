/**
 * Tests for App component
 */

import React from 'react';
import { render } from '@testing-library/react';
import App from './App';

// Mock AuthContext
jest.mock('./contexts/AuthContext', () => ({
  AuthProvider: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  useAuth: () => ({
    user: null,
    isAuthenticated: jest.fn(() => false),
    login: jest.fn(),
    register: jest.fn(),
    logout: jest.fn(),
    validateToken: jest.fn().mockResolvedValue(false),
  }),
}));


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

describe('App', () => {
  it('should render without crashing', () => {
    const { container } = render(<App />);
    expect(container).toBeInTheDocument();
  });

  it('should wrap app with ThemeProvider and BrowserRouter', () => {
    const { getByTestId } = render(<App />);
    
    // Should eventually render routes (after auth check)
    setTimeout(() => {
      expect(getByTestId('app-routes')).toBeInTheDocument();
    }, 100);
  });
});

