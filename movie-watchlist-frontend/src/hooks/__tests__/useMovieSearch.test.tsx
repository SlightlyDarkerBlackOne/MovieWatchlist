import React from 'react';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useSearchParams } from 'react-router-dom';
import { useMovieSearch } from '../useMovieSearch';
import { useSearchMoviesQuery } from '../../store/api/moviesApi';
import { AllProviders } from '../../utils/test-utils';
import { TestConstants } from '../../__tests__/TestConstants';

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useSearchParams: jest.fn(),
}));

jest.mock('../../store/api/moviesApi', () => ({
  ...jest.requireActual('../../store/api/moviesApi'),
  useSearchMoviesQuery: jest.fn(),
}));

const mockUseSearchParams = useSearchParams as jest.MockedFunction<typeof useSearchParams>;
const mockUseSearchMoviesQuery = useSearchMoviesQuery as jest.MockedFunction<typeof useSearchMoviesQuery>;

describe('useMovieSearch', () => {
  const mockSetSearchParams = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    mockUseSearchParams.mockReturnValue([
      new URLSearchParams(),
      mockSetSearchParams,
    ] as any);
  });

  it('should initialize with empty search query', () => {
    mockUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useMovieSearch(), { wrapper });

    expect(result.current.searchQuery).toBe('');
    expect(result.current.searchResults).toBeUndefined();
    expect(result.current.searchLoading).toBe(false);
  });

  it('should sync with URL search params', () => {
    const searchParams = new URLSearchParams(`?search=${TestConstants.SearchQueries.Matrix}`);
    mockUseSearchParams.mockReturnValue([searchParams, mockSetSearchParams] as any);
    
    mockUseSearchMoviesQuery.mockReturnValue({
      data: { movies: [], totalResults: 0 },
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useMovieSearch(), { wrapper });

    expect(result.current.searchQuery).toBe(TestConstants.SearchQueries.Matrix);
  });

  it('should handle search errors', () => {
    const mockError = { message: TestConstants.ErrorMessages.SearchFailed };
    mockUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useMovieSearch(), { wrapper });

    expect(result.current.searchError).toEqual(mockError);
  });
});

