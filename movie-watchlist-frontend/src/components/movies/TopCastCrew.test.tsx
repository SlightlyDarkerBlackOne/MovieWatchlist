/**
 * Tests for TopCastCrew component
 */

import React from 'react';
import { screen } from '@testing-library/react';
import { render } from '../../utils/test-utils';
import TopCastCrew from './TopCastCrew';
import { mockCastMember } from '../../__tests__/fixtures/movieFixtures';

describe('TopCastCrew', () => {
  const mockCast = [
    mockCastMember,
    { ...mockCastMember, id: 288, name: 'Edward Norton', character: 'The Narrator', castId: 5 },
    { ...mockCastMember, id: 289, name: 'Helena Bonham Carter', character: 'Marla Singer', castId: 6 },
  ];

  it('should render cast members', () => {
    render(<TopCastCrew topCast={mockCast} />);

    expect(screen.getByText('Top Cast')).toBeInTheDocument();
    expect(screen.getByText('Brad Pitt')).toBeInTheDocument();
    expect(screen.getByText('Tyler Durden')).toBeInTheDocument();
    expect(screen.getByText('Edward Norton')).toBeInTheDocument();
    expect(screen.getByText('Helena Bonham Carter')).toBeInTheDocument();
  });

  it('should show actor names and characters', () => {
    render(<TopCastCrew topCast={[mockCastMember]} />);

    expect(screen.getByText(mockCastMember.name)).toBeInTheDocument();
    expect(screen.getByText(mockCastMember.character)).toBeInTheDocument();
  });

  it('should display profile images when available', () => {
    render(<TopCastCrew topCast={[mockCastMember]} />);

    const image = screen.getByAltText(mockCastMember.name);
    expect(image).toBeInTheDocument();
    expect(image).toHaveAttribute('src', expect.stringContaining(mockCastMember.profilePath!));
  });

  it('should display fallback avatar when profile image is missing', () => {
    const castWithoutPhoto = { ...mockCastMember, profilePath: null };
    
    render(<TopCastCrew topCast={[castWithoutPhoto]} />);

    // Should show avatar with first letter of name
    expect(screen.getByText('B')).toBeInTheDocument(); // "B" from "Brad Pitt"
  });

  it('should return null when empty', () => {
    const { container } = render(<TopCastCrew topCast={[]} />);

    expect(container.firstChild).toBeNull();
  });

  it('should render scrollable container for many cast members', () => {
    const manyCast = Array.from({ length: 15 }, (_, i) => ({
      ...mockCastMember,
      id: i,
      castId: i,
      name: `Actor ${i}`,
      character: `Character ${i}`,
    }));

    render(<TopCastCrew topCast={manyCast} />);

    expect(screen.getByText('Top Cast')).toBeInTheDocument();
    expect(screen.getAllByRole('img')).toHaveLength(15);
  });
});


