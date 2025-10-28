import { WatchlistStatus } from '../types/watchlist.types';

export const STATUS_CONFIG = {
  [WatchlistStatus.Planned]: {
    label: 'Planned',
    color: 'info' as const,
  },
  [WatchlistStatus.Watching]: {
    label: 'Watching',
    color: 'primary' as const,
  },
  [WatchlistStatus.Watched]: {
    label: 'Watched',
    color: 'success' as const,
  },
  [WatchlistStatus.Dropped]: {
    label: 'Dropped',
    color: 'error' as const,
  },
} as const;

export function getStatusLabel(status: WatchlistStatus): string {
  return STATUS_CONFIG[status]?.label || 'Unknown';
}

export function getStatusColor(status: WatchlistStatus): 'default' | 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success' {
  return STATUS_CONFIG[status]?.color || 'default';
}

export function getAllStatuses() {
  return Object.entries(STATUS_CONFIG).map(([value, config]) => ({
    value: Number(value) as WatchlistStatus,
    label: config.label,
    color: config.color,
  }));
}
