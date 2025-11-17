import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import { useNavigate } from 'react-router-dom';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

class QueryErrorBoundaryClass extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
    };
  }

  static getDerivedStateFromError(error: Error): Partial<ErrorBoundaryState> {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    this.setState({ error });
  }

  handleReset = (): void => {
    this.setState({
      hasError: false,
      error: null,
    });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return <QueryErrorFallback error={this.state.error} onReset={this.handleReset} />;
    }

    return this.props.children;
  }
}

function QueryErrorFallback({ error, onReset }: { error: Error | null; onReset: () => void }) {
  const navigate = useNavigate();

  const handleGoHome = () => {
    onReset();
    navigate('/');
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      p={3}
    >
      <Paper elevation={3} sx={{ p: 4, maxWidth: 600 }}>
        <Typography variant="h4" component="h1" gutterBottom color="error">
          Oops! Something went wrong
        </Typography>
        
        <Typography variant="body1" paragraph>
          We're sorry for the inconvenience. An error occurred while loading the page.
        </Typography>

        {process.env.NODE_ENV === 'development' && error && (
          <Box sx={{ mt: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
            <Typography variant="body2" component="pre" sx={{ overflow: 'auto' }}>
              {error.toString()}
            </Typography>
          </Box>
        )}

        <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            color="primary"
            onClick={onReset}
          >
            Try Again
          </Button>
          
          <Button
            variant="outlined"
            onClick={handleGoHome}
          >
            Go to Home
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}

const QueryErrorBoundary: React.FC<ErrorBoundaryProps> = ({ children, fallback }) => {
  return <QueryErrorBoundaryClass fallback={fallback}>{children}</QueryErrorBoundaryClass>;
};

export default QueryErrorBoundary;


