import React from 'react';
import { Box, Tabs, Tab } from '@mui/material';
import { WatchlistStatus, WatchlistItem } from '../../types/watchlist.types';

interface WatchlistStatsProps {
  activeTab: number;
  onTabChange: (event: React.SyntheticEvent, newValue: number) => void;
  watchlist: WatchlistItem[];
}

const WatchlistStats: React.FC<WatchlistStatsProps> = ({
  activeTab,
  onTabChange,
  watchlist
}) => {
  const favoriteCount = watchlist.filter(i => i.isFavorite).length;
  const watchedCount = watchlist.filter(i => i.status === WatchlistStatus.Watched).length;

  return (
    <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
      <Tabs value={activeTab} onChange={onTabChange} aria-label="watchlist tabs">
        <Tab label={`All (${watchlist.length})`} id="watchlist-tab-0" />
        <Tab label={`Favorites (${favoriteCount})`} id="watchlist-tab-1" />
        <Tab label={`Watched (${watchedCount})`} id="watchlist-tab-2" />
      </Tabs>
    </Box>
  );
};

export default WatchlistStats;

