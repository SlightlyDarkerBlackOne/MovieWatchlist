/**
 * Tests for MovieGenres component
 */

import React from 'react';
import { screen } from '@testing-library/react';
import { render } from '../../utils/test-utils';
import MovieGenres from './MovieGenres';

describe('MovieGenres', () => {
  it('should render genre chips', () => {
    const genres = ['Action', 'Adventure', 'Sci-Fi'];
    
    render(<MovieGenres genres={genres} />);

    expect(screen.getByText('Action')).toBeInTheDocument();
    expect(screen.getByText('Adventure')).toBeInTheDocument();
    expect(screen.getByText('Sci-Fi')).toBeInTheDocument();
  });

  it('should render nothing when empty', () => {
    const { container } = render(<MovieGenres genres={[]} />);
    
    expect(container.firstChild).toBeNull();
  });

  it('should handle single genre', () => {
    render(<MovieGenres genres={['Drama']} />);

    expect(screen.getByText('Drama')).toBeInTheDocument();
  });

  it('should render multiple genres with proper layout', () => {
    const genres = ['Action', 'Adventure', 'Comedy', 'Drama', 'Thriller'];
    
    render(<MovieGenres genres={genres} />);

    genres.forEach(genre => {
      expect(screen.getByText(genre)).toBeInTheDocument();
    });
  });
});


