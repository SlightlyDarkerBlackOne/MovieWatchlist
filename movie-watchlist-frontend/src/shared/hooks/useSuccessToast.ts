import { useState, useEffect, useCallback } from 'react';

export const useSuccessToast = (autoHideDuration = 3000) => {
  const [message, setMessage] = useState<string | null>(null);

  const showMessage = useCallback((msg: string) => {
    setMessage(msg);
  }, []);

  const hideMessage = useCallback(() => {
    setMessage(null);
  }, []);

  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => setMessage(null), autoHideDuration);
      return () => clearTimeout(timer);
    }
  }, [message, autoHideDuration]);

  return {
    message,
    showMessage,
    hideMessage
  };
};

