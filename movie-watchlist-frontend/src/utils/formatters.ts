/**
 * Utility functions for formatting various data types
 */

/**
 * Format vote count for display
 * @param count - Number of votes
 * @returns Formatted string (e.g., 1234 -> 1.2K, 1234567 -> 1.2M)
 */
export const formatVoteCount = (count: number): string => {
  if (count >= 1000000) {
    return `${(count / 1000000).toFixed(1)}M`;
  }
  if (count >= 1000) {
    return `${(count / 1000).toFixed(1)}K`;
  }
  return count.toString();
};

/**
 * Format runtime in minutes to hours and minutes
 * @param minutes - Runtime in minutes
 * @returns Formatted string (e.g., 142 -> "2h 22m")
 */
export const formatRuntime = (minutes: number | null): string => {
  if (!minutes) return 'N/A';
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m`;
};

/**
 * Format currency to readable format
 * @param amount - Amount in dollars
 * @returns Formatted string (e.g., 1000000 -> "$1.0M")
 */
export const formatCurrency = (amount: number): string => {
  if (amount >= 1000000000) {
    return `$${(amount / 1000000000).toFixed(1)}B`;
  }
  if (amount >= 1000000) {
    return `$${(amount / 1000000).toFixed(1)}M`;
  }
  if (amount >= 1000) {
    return `$${(amount / 1000).toFixed(1)}K`;
  }
  return `$${amount}`;
};

/**
 * Format date to readable format
 * @param dateString - ISO date string
 * @returns Formatted date string (e.g., "Jan 1, 2023")
 */
export const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric' 
  });
};

/**
 * Get release year from date string
 * @param dateString - ISO date string
 * @returns Year as string or 'N/A'
 */
export const getReleaseYear = (dateString: string | null): string => {
  if (!dateString) return 'N/A';
  return new Date(dateString).getFullYear().toString();
};