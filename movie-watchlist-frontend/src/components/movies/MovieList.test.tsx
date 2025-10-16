/**
 * Tests for MovieList component
 */

import React from 'react';
import { screen, fireEvent } from '@testing-library/react';
import { render } from '../../utils/test-utils';
import MovieList from './MovieList';
import { mockMovies } from '../../__tests__/fixtures/movieFixtures';

// Mock MovieCard since we're testing MovieList in isolation
jest.mock('./MovieCard', () => {
  return function MockMovieCard({ movie }: { movie: { title: string } }) {
    return <div data-testid="movie-card">{movie.title}</div>;
  };
});

describe('MovieList', () => {
  it('should render grid of movies', () => {
    render(<MovieList movies={mockMovies} />);

    const movieCards = screen.getAllByTestId('movie-card');
    expect(movieCards).toHaveLength(mockMovies.length);
  });

  it('should display loading spinner when loading', () => {
    render(<MovieList movies={[]} loading={true} />);

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
    expect(screen.queryByTestId('movie-card')).not.toBeInTheDocument();
  });

  it('should show "No movies found" when empty', () => {
    render(<MovieList movies={[]} loading={false} />);

    expect(screen.getByText('No movies found')).toBeInTheDocument();
    expect(screen.getByText(/try searching for something else/i)).toBeInTheDocument();
  });

  it('should render pagination when totalPages > 1', () => {
    const mockOnPageChange = jest.fn();
    
    render(
      <MovieList 
        movies={mockMovies} 
        currentPage={1}
        totalPages={5}
        onPageChange={mockOnPageChange}
      />
    );

    // MUI Pagination should be rendered
    expect(screen.getByRole('navigation')).toBeInTheDocument();
  });

  it('should not render pagination when totalPages = 1', () => {
    render(
      <MovieList 
        movies={mockMovies} 
        currentPage={1}
        totalPages={1}
        onPageChange={jest.fn()}
      />
    );

    expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
  });

  it('should call onPageChange when page is clicked', () => {
    const mockOnPageChange = jest.fn();
    
    render(
      <MovieList 
        movies={mockMovies} 
        currentPage={1}
        totalPages={3}
        onPageChange={mockOnPageChange}
      />
    );

    // Find and click page 2 button
    const page2Button = screen.getByRole('button', { name: 'Go to page 2' });
    fireEvent.click(page2Button);

    expect(mockOnPageChange).toHaveBeenCalledWith(2);
  });

  it('should handle page change with no onPageChange callback', () => {
    render(
      <MovieList 
        movies={mockMovies} 
        currentPage={1}
        totalPages={3}
      />
    );

    // Should not render pagination without callback
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
  });
});


