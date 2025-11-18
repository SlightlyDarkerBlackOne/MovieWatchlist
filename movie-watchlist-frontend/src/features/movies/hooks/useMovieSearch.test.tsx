/**
 * Tests for useMovieSearch hook
 */

import { renderHook, act } from '@testing-library/react';
import { useSearchParams } from 'react-router-dom';
import { useMovieSearch } from './useMovieSearch';
import { useSearchMoviesQuery } from '../api/moviesApi';

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useSearchParams: jest.fn(),
}));

jest.mock('../api/moviesApi', () => ({
  useSearchMoviesQuery: jest.fn(),
}));

const mockedUseSearchParams = useSearchParams as jest.MockedFunction<typeof useSearchParams>;
const mockedUseSearchMoviesQuery = useSearchMoviesQuery as jest.MockedFunction<typeof useSearchMoviesQuery>;

describe('useMovieSearch', () => {
  let mockSearchParams: URLSearchParams;
  let mockSetSearchParams: jest.Mock;

  beforeEach(() => {
    jest.clearAllMocks();
    jest.useFakeTimers();

    mockSearchParams = new URLSearchParams();
    mockSetSearchParams = jest.fn();

    mockedUseSearchParams.mockReturnValue([mockSearchParams, mockSetSearchParams]);
    mockedUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    } as any);
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  it('should initialize with empty search query', () => {
    const { result } = renderHook(() => useMovieSearch());

    expect(result.current.searchQuery).toBe('');
    expect(result.current.searchResults).toBeUndefined();
    expect(result.current.searchLoading).toBe(false);
    expect(result.current.searchError).toBeUndefined();
  });

  it('should skip query when search query is empty', () => {
    renderHook(() => useMovieSearch());

    expect(mockedUseSearchMoviesQuery).toHaveBeenCalledWith(
      { query: '', page: 1 },
      { skip: true }
    );
  });

  it('should set search query from URL params', () => {
    mockSearchParams.set('search', 'inception');

    const { result } = renderHook(() => useMovieSearch());

    expect(result.current.searchQuery).toBe('inception');
  });

  it('should update search query when URL params change', () => {
    const newSearchParams = new URLSearchParams();
    newSearchParams.set('search', 'matrix');
    
    mockedUseSearchParams.mockReturnValueOnce([mockSearchParams, mockSetSearchParams])
      .mockReturnValueOnce([newSearchParams, mockSetSearchParams]);

    const { result, rerender } = renderHook(() => useMovieSearch());

    expect(result.current.searchQuery).toBe('');

    rerender();

    expect(result.current.searchQuery).toBe('matrix');
  });

  it('should not update if URL param matches current query', () => {
    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('inception');
    });

    mockSearchParams.set('search', 'inception');
    mockedUseSearchParams.mockReturnValue([mockSearchParams, mockSetSearchParams]);

    const { result: newResult } = renderHook(() => useMovieSearch());

    expect(newResult.current.searchQuery).toBe('inception');
  });

  it('should execute query when search query is set', () => {
    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('test');
    });

    expect(mockedUseSearchMoviesQuery).toHaveBeenCalledWith(
      { query: 'test', page: 1 },
      { skip: false }
    );
  });

  it('should skip query for whitespace-only search', () => {
    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('   ');
    });

    expect(mockedUseSearchMoviesQuery).toHaveBeenCalledWith(
      { query: '   ', page: 1 },
      { skip: true }
    );
  });

  it('should expose search results from query', () => {
    const mockResults = {
      movies: [],
      totalResults: 0,
      totalPages: 0,
      currentPage: 1,
    };

    mockedUseSearchMoviesQuery.mockReturnValue({
      data: mockResults,
      isLoading: false,
      error: undefined,
    } as any);

    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('test');
    });

    expect(result.current.searchResults).toEqual(mockResults);
  });

  it('should expose loading state', () => {
    mockedUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: undefined,
    } as any);

    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('test');
    });

    expect(result.current.searchLoading).toBe(true);
  });

  it('should expose error state', () => {
    const mockError = { status: 500, data: { message: 'Server error' } };

    mockedUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
    } as any);

    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('test');
    });

    expect(result.current.searchError).toEqual(mockError);
  });

  it('should provide searchResultsRef', () => {
    const { result } = renderHook(() => useMovieSearch());

    expect(result.current.searchResultsRef).toBeDefined();
    expect(result.current.searchResultsRef.current).toBeNull();
  });

  it('should handle empty search query reset', () => {
    const { result } = renderHook(() => useMovieSearch());

    act(() => {
      result.current.setSearchQuery('test');
    });

    act(() => {
      result.current.setSearchQuery('');
    });

    expect(mockedUseSearchMoviesQuery).toHaveBeenCalledWith(
      { query: '', page: 1 },
      { skip: true }
    );
  });
});

