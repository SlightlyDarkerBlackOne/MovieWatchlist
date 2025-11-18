/**
 * Tests for useAddToWatchlistDialog hook
 */

import { renderHook, act } from '@testing-library/react';
import { useAddToWatchlistDialog } from './useAddToWatchlistDialog';
import { WatchlistStatus } from '../model/watchlist.types';
import { mockMovie } from '../../../__tests__/fixtures/movieFixtures';

describe('useAddToWatchlistDialog', () => {
  it('should initialize with closed state', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());

    expect(result.current.isOpen).toBe(false);
    expect(result.current.selectedMovie).toBeNull();
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should open dialog with movie', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());

    act(() => {
      result.current.openDialog(mockMovie);
    });

    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(mockMovie);
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should close dialog and reset state', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());

    act(() => {
      result.current.openDialog(mockMovie);
    });

    act(() => {
      result.current.closeDialog();
    });

    expect(result.current.isOpen).toBe(false);
    expect(result.current.selectedMovie).toBeNull();
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should update status', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());

    act(() => {
      result.current.openDialog(mockMovie);
      result.current.setStatus(WatchlistStatus.Watched);
    });

    expect(result.current.status).toBe(WatchlistStatus.Watched);
    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(mockMovie);
  });

  it('should update notes', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());
    const testNotes = 'Great movie!';

    act(() => {
      result.current.openDialog(mockMovie);
      result.current.setNotes(testNotes);
    });

    expect(result.current.notes).toBe(testNotes);
    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(mockMovie);
  });

  it('should update both status and notes', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());
    const testNotes = 'Must watch again';

    act(() => {
      result.current.openDialog(mockMovie);
      result.current.setStatus(WatchlistStatus.Watched);
      result.current.setNotes(testNotes);
    });

    expect(result.current.status).toBe(WatchlistStatus.Watched);
    expect(result.current.notes).toBe(testNotes);
  });

  it('should reset status and notes when closing dialog', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());

    act(() => {
      result.current.openDialog(mockMovie);
      result.current.setStatus(WatchlistStatus.Watched);
      result.current.setNotes('Test notes');
    });

    act(() => {
      result.current.closeDialog();
    });

    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should handle multiple open/close cycles', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog());
    const secondMovie = { ...mockMovie, tmdbId: 999, title: 'Another Movie' };

    act(() => {
      result.current.openDialog(mockMovie);
    });

    act(() => {
      result.current.closeDialog();
    });

    act(() => {
      result.current.openDialog(secondMovie);
    });

    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(secondMovie);
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });
});

