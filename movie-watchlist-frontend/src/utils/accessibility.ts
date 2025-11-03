/**
 * Accessibility utilities for enhancing ARIA labels and keyboard navigation
 */

/**
 * Creates a descriptive ARIA label for movie cards
 */
export const getMovieCardAriaLabel = (movie: { title: string; voteAverage: number; releaseDate?: string }) => {
  const releaseYear = movie.releaseDate ? new Date(movie.releaseDate).getFullYear() : 'Unknown';
  return `${movie.title}, rating ${movie.voteAverage.toFixed(1)} out of 10, released ${releaseYear}`;
};

/**
 * Creates an ARIA label for watchlist status
 */
export const getWatchlistStatusAriaLabel = (isInWatchlist: boolean) => {
  return isInWatchlist ? 'Already in your watchlist' : 'Add to watchlist';
};

/**
 * Creates an ARIA live region message for screen readers
 */
export const getSuccessMessage = (itemName: string, action: string) => {
  return `${itemName} successfully ${action}`;
};

/**
 * Creates an ARIA live region message for errors
 */
export const getErrorMessage = (itemName: string, action: string) => {
  return `Failed to ${action} ${itemName}`;
};

/**
 * Focus management utilities
 */
export const focusElement = (elementId: string) => {
  const element = document.getElementById(elementId);
  if (element) {
    element.focus();
  }
};

export const announceToScreenReader = (message: string, priority: 'polite' | 'assertive' = 'polite') => {
  const announcement = document.createElement('div');
  announcement.setAttribute('role', 'status');
  announcement.setAttribute('aria-live', priority);
  announcement.setAttribute('aria-atomic', 'true');
  announcement.className = 'sr-only';
  announcement.textContent = message;
  document.body.appendChild(announcement);
  
  setTimeout(() => {
    document.body.removeChild(announcement);
  }, 1000);
};

/**
 * Keyboard event handlers
 */
export const handleEnterKey = (
  event: React.KeyboardEvent,
  callback: () => void
) => {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault();
    callback();
  }
};

export const handleEscapeKey = (
  event: React.KeyboardEvent,
  callback: () => void
) => {
  if (event.key === 'Escape') {
    callback();
  }
};

/**
 * Creates a keyboard navigation handler
 */
export const createKeyboardHandler = (callbacks: {
  onEnter?: () => void;
  onEscape?: () => void;
  onArrowLeft?: () => void;
  onArrowRight?: () => void;
  onHome?: () => void;
  onEnd?: () => void;
}) => {
  return (event: React.KeyboardEvent) => {
    switch (event.key) {
      case 'Enter':
      case ' ':
        callbacks.onEnter?.();
        break;
      case 'Escape':
        callbacks.onEscape?.();
        break;
      case 'ArrowLeft':
        callbacks.onArrowLeft?.();
        break;
      case 'ArrowRight':
        callbacks.onArrowRight?.();
        break;
      case 'Home':
        callbacks.onHome?.();
        break;
      case 'End':
        callbacks.onEnd?.();
        break;
    }
  };
};

/**
 * Traps focus within a container
 */
export const trapFocus = (container: HTMLElement) => {
  const focusableElements = container.querySelectorAll(
    'a[href], button:not([disabled]), textarea, input:not([disabled]), select'
  );
  
  const firstFocusableElement = focusableElements[0] as HTMLElement;
  const lastFocusableElement = focusableElements[focusableElements.length - 1] as HTMLElement;

  const handleTabKey = (e: KeyboardEvent) => {
    if (e.key !== 'Tab') return;

    if (e.shiftKey) {
      if (document.activeElement === firstFocusableElement) {
        lastFocusableElement.focus();
        e.preventDefault();
      }
    } else {
      if (document.activeElement === lastFocusableElement) {
        firstFocusableElement.focus();
        e.preventDefault();
      }
    }
  };

  container.addEventListener('keydown', handleTabKey);

  return () => {
    container.removeEventListener('keydown', handleTabKey);
  };
};

