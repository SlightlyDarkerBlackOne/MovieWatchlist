import React from 'react';
import { renderHook, act } from '@testing-library/react';
import { useAddToWatchlistDialog } from '../useAddToWatchlistDialog';
import { AllProviders } from '../../utils/test-utils';
import { mockMovies } from '../../__tests__/fixtures/movieFixtures';
import { WatchlistStatus } from '../../types/watchlist.types';

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <AllProviders>{children}</AllProviders>
);

describe('useAddToWatchlistDialog', () => {
  it('should initialize with closed dialog', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });

    expect(result.current.isOpen).toBe(false);
    expect(result.current.selectedMovie).toBeNull();
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should open dialog with selected movie', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });
    const movie = mockMovies[0];

    act(() => {
      result.current.openDialog(movie);
    });

    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(movie);
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should close dialog', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });
    const movie = mockMovies[0];

    act(() => {
      result.current.openDialog(movie);
    });

    expect(result.current.isOpen).toBe(true);

    act(() => {
      result.current.closeDialog();
    });

    expect(result.current.isOpen).toBe(false);
    expect(result.current.selectedMovie).toBeNull();
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });

  it('should update status', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });

    act(() => {
      result.current.setStatus(WatchlistStatus.Watched);
    });

    expect(result.current.status).toBe(WatchlistStatus.Watched);
  });

  it('should update notes', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });
    const notes = 'Must watch this weekend!';

    act(() => {
      result.current.setNotes(notes);
    });

    expect(result.current.notes).toBe(notes);
  });

  it('should reset state when opening new movie after closing', () => {
    const { result } = renderHook(() => useAddToWatchlistDialog(), { wrapper });
    const movie1 = mockMovies[0];
    const movie2 = mockMovies[1];

    act(() => {
      result.current.openDialog(movie1);
      result.current.setStatus(WatchlistStatus.Watching);
      result.current.setNotes('First movie notes');
      result.current.closeDialog();
      result.current.openDialog(movie2);
    });

    expect(result.current.isOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(movie2);
    expect(result.current.status).toBe(WatchlistStatus.Planned);
    expect(result.current.notes).toBe('');
  });
});

