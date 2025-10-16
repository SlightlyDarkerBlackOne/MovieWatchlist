/**
 * Tests for utility formatter functions
 */

import { formatVoteCount, formatRuntime, getReleaseYear } from './formatters';

describe('formatVoteCount', () => {
  it('should format vote count of 0', () => {
    expect(formatVoteCount(0)).toBe('0');
  });

  it('should format vote count less than 1000', () => {
    expect(formatVoteCount(500)).toBe('500');
    expect(formatVoteCount(999)).toBe('999');
  });

  it('should format vote count in thousands (K)', () => {
    expect(formatVoteCount(1000)).toBe('1.0K');
    expect(formatVoteCount(1500)).toBe('1.5K');
    expect(formatVoteCount(15000)).toBe('15.0K');
    expect(formatVoteCount(15400)).toBe('15.4K');
  });

  it('should format vote count in millions (M)', () => {
    expect(formatVoteCount(1000000)).toBe('1.0M');
    expect(formatVoteCount(1500000)).toBe('1.5M');
    expect(formatVoteCount(2340000)).toBe('2.3M');
  });

  it('should handle negative numbers', () => {
    expect(formatVoteCount(-100)).toBe('-100');
  });
});

describe('formatRuntime', () => {
  it('should format runtime of 0 minutes', () => {
    expect(formatRuntime(0)).toBe('N/A');
  });

  it('should format runtime less than 60 minutes', () => {
    expect(formatRuntime(45)).toBe('0h 45m');
  });

  it('should format runtime of exactly 60 minutes', () => {
    expect(formatRuntime(60)).toBe('1h 0m');
  });

  it('should format runtime of 90 minutes', () => {
    expect(formatRuntime(90)).toBe('1h 30m');
  });

  it('should format runtime of 120 minutes', () => {
    expect(formatRuntime(120)).toBe('2h 0m');
  });

  it('should format runtime of 150 minutes', () => {
    expect(formatRuntime(150)).toBe('2h 30m');
  });

  it('should format runtime of 180 minutes', () => {
    expect(formatRuntime(180)).toBe('3h 0m');
  });

  it('should handle edge case of 1 minute', () => {
    expect(formatRuntime(1)).toBe('0h 1m');
  });
});

describe('getReleaseYear', () => {
  it('should extract year from valid date string', () => {
    expect(getReleaseYear('1999-10-15')).toBe('1999');
    expect(getReleaseYear('2024-01-01')).toBe('2024');
  });

  it('should handle ISO date strings', () => {
    expect(getReleaseYear('1999-10-15T00:00:00Z')).toBe('1999');
  });

  it('should return N/A for invalid date', () => {
    expect(getReleaseYear('invalid-date')).toBe('NaN');
  });

  it('should return N/A for empty string', () => {
    expect(getReleaseYear('')).toBe('N/A');
  });

  it('should return N/A for null input', () => {
    expect(getReleaseYear(null)).toBe('N/A');
  });

  it('should return N/A for undefined input', () => {
    expect(getReleaseYear(undefined as unknown as string)).toBe('N/A');
  });
});


