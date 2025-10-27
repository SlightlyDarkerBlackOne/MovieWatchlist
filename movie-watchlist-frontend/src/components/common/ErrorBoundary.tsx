import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

/**
 * Error Boundary Component
 * 
 * Catches JavaScript errors anywhere in the child component tree,
 * logs those errors, and displays a fallback UI.
 * 
 * Note: Must be a class component (React requirement)
 */
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
    };
  }

  static getDerivedStateFromError(error: Error): Partial<ErrorBoundaryState> {
    // Update state so the next render will show the fallback UI
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Log error to console (in production, you'd log to error reporting service)
    console.error('Error caught by boundary:', error, errorInfo);
    
    this.setState({
      error,
      errorInfo,
    });
  }

  handleReset = (): void => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
    });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      // Custom fallback UI
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Default fallback UI
      return (
        <Box
          display="flex"
          justifyContent="center"
          alignItems="center"
          minHeight="100vh"
          p={3}
          role="alert"
          aria-live="assertive"
          aria-atomic="true"
        >
          <Paper elevation={3} sx={{ p: 4, maxWidth: 600 }}>
            <Typography 
              variant="h4" 
              component="h1" 
              gutterBottom 
              color="error"
              id="error-title"
            >
              Oops! Something went wrong
            </Typography>
            
            <Typography variant="body1" paragraph id="error-description">
              We're sorry for the inconvenience. An unexpected error has occurred.
            </Typography>

            {process.env.NODE_ENV === 'development' && this.state.error && (
              <Box sx={{ mt: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
                <Typography 
                  variant="body2" 
                  component="pre" 
                  sx={{ overflow: 'auto' }}
                  aria-label="Error details for developers"
                >
                  {this.state.error.toString()}
                  {this.state.errorInfo?.componentStack}
                </Typography>
              </Box>
            )}

            <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
              <Button
                variant="contained"
                color="primary"
                onClick={this.handleReset}
                aria-describedby="error-description"
              >
                Try Again
              </Button>
              
              <Button
                variant="outlined"
                onClick={() => window.location.href = '/'}
                aria-label="Navigate to home page"
              >
                Go to Home
              </Button>
            </Box>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;

