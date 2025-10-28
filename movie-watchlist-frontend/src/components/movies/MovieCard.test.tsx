/**
 * Tests for MovieCard component
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter } from 'react-router-dom';
import MovieCard from './MovieCard';
import { mockMovie, mockMovieWithoutPoster } from '../../__tests__/fixtures/movieFixtures';
import { useWatchlist } from '../../contexts/WatchlistContext';
import { useNavigate } from 'react-router-dom';
import { appTheme } from '../../theme';

// Mock contexts and navigation
jest.mock('../../contexts/WatchlistContext');
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: jest.fn(),
}));

const mockedUseWatchlist = useWatchlist as jest.MockedFunction<typeof useWatchlist>;
const mockedUseNavigate = useNavigate as jest.MockedFunction<typeof useNavigate>;

describe('MovieCard', () => {
  const mockNavigate = jest.fn();
  const mockIsInWatchlist = jest.fn();

  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <ThemeProvider theme={appTheme}>
        <BrowserRouter>
          {ui}
        </BrowserRouter>
      </ThemeProvider>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseNavigate.mockReturnValue(mockNavigate);
    mockedUseWatchlist.mockReturnValue({
      watchlistMovieIds: [],
      isInWatchlist: mockIsInWatchlist,
    });
    mockIsInWatchlist.mockReturnValue(false);
  });

  it('should render movie information correctly', () => {
    renderWithProviders(<MovieCard movie={mockMovie} />);

    expect(screen.getByText(mockMovie.title)).toBeInTheDocument();
    expect(screen.getByText(mockMovie.voteAverage.toFixed(1))).toBeInTheDocument();
    expect(screen.getByText('/10')).toBeInTheDocument();
  });

  it('should display vote count when available', () => {
    renderWithProviders(<MovieCard movie={mockMovie} />);

    // Vote count should be formatted (e.g., "26.3K")
    expect(screen.getByText(/K|M/)).toBeInTheDocument();
  });

  it('should show release year', () => {
    renderWithProviders(<MovieCard movie={mockMovie} />);

    expect(screen.getByText('1999')).toBeInTheDocument();
  });

  it('should show "In Watchlist" chip when movie is in watchlist', () => {
    mockIsInWatchlist.mockReturnValue(true);
    
    renderWithProviders(<MovieCard movie={mockMovie} />);

    expect(screen.getByText('In Watchlist')).toBeInTheDocument();
  });

  it('should not show "In Watchlist" chip when movie is not in watchlist', () => {
    mockIsInWatchlist.mockReturnValue(false);
    
    renderWithProviders(<MovieCard movie={mockMovie} />);

    expect(screen.queryByText('In Watchlist')).not.toBeInTheDocument();
  });

  it('should navigate to movie details when card is clicked', () => {
    renderWithProviders(<MovieCard movie={mockMovie} />);

    const image = screen.getByRole('img', { name: mockMovie.title });
    const card = image.closest('.MuiCard-root');
    if (card) {
      fireEvent.click(card);
    }

    expect(mockNavigate).toHaveBeenCalledWith(`/movies/${mockMovie.tmdbId}`);
  });

  it('should display placeholder when poster is missing', () => {
    renderWithProviders(<MovieCard movie={mockMovieWithoutPoster} />);

    const image = screen.getByRole('img');
    expect(image).toHaveAttribute('src', expect.stringContaining('placeholder'));
  });

  it('should display movie overview', () => {
    renderWithProviders(<MovieCard movie={mockMovie} />);

    // Overview might be truncated, so just check if it starts correctly
    const overviewStart = mockMovie.overview.substring(0, 20);
    expect(screen.getByText(new RegExp(overviewStart))).toBeInTheDocument();
  });
});


