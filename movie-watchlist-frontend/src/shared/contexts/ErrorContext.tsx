import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { Snackbar, Alert } from '@mui/material';

interface ErrorContextType {
  showError: (message: string, severity?: 'error' | 'warning') => void;
}

const ErrorContext = createContext<ErrorContextType | undefined>(undefined);

interface ErrorProviderProps {
  children: ReactNode;
}

export const ErrorProvider: React.FC<ErrorProviderProps> = ({ children }) => {
  const [error, setError] = useState<{ message: string; severity: 'error' | 'warning' } | null>(null);

  const showError = useCallback((message: string, severity: 'error' | 'warning' = 'error') => {
    setError({ message, severity });
  }, []);

  const handleClose = useCallback(() => {
    setError(null);
  }, []);

  return (
    <ErrorContext.Provider value={{ showError }}>
      {children}
      {error && (
        <Snackbar
          open={!!error}
          autoHideDuration={6000}
          onClose={handleClose}
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        >
          <Alert 
            severity={error.severity} 
            variant="filled" 
            sx={{ width: '100%' }}
            onClose={handleClose}
          >
            {error.message}
          </Alert>
        </Snackbar>
      )}
    </ErrorContext.Provider>
  );
};

export const useError = (): ErrorContextType => {
  const context = useContext(ErrorContext);
  if (!context) {
    throw new Error('useError must be used within ErrorProvider');
  }
  return context;
};

