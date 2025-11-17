/**
 * Tests for HeaderSearch component
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import HeaderSearch from './HeaderSearch';

// Mock SearchDropdown component
jest.mock('./SearchDropdown', () => {
  return function MockSearchDropdown({ onFullSearch }: { onFullSearch: (query: string) => void }) {
    return (
      <div data-testid="search-dropdown">
        <button onClick={() => onFullSearch('test query')}>Search</button>
      </div>
    );
  };
});

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('HeaderSearch', () => {
  it('should render SearchDropdown', () => {
    const mockOnSearch = jest.fn();
    renderWithRouter(<HeaderSearch onSearch={mockOnSearch} />);
    
    expect(screen.getByTestId('search-dropdown')).toBeInTheDocument();
  });

  it('should call onSearch callback when SearchDropdown triggers search', () => {
    const mockOnSearch = jest.fn();
    renderWithRouter(<HeaderSearch onSearch={mockOnSearch} />);
    
    const searchButton = screen.getByRole('button', { name: /search/i });
    searchButton.click();
    
    expect(mockOnSearch).toHaveBeenCalledWith('test query');
  });

  it('should have proper max width styling', () => {
    const mockOnSearch = jest.fn();
    const { container } = renderWithRouter(<HeaderSearch onSearch={mockOnSearch} />);
    
    const wrapper = container.firstChild;
    expect(wrapper).toHaveStyle({ maxWidth: '600px' });
  });
});

