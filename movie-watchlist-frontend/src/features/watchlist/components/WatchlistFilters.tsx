import React from 'react';
import { Box, FormControl, InputLabel, Select, MenuItem, Typography, SelectChangeEvent } from '@mui/material';
import { WatchlistStatus } from '../model/watchlist.types';

interface WatchlistFiltersProps {
  statusFilter: number | 'all';
  onStatusFilterChange: (event: SelectChangeEvent<number | 'all'>) => void;
  filteredCount: number;
}

const WatchlistFilters: React.FC<WatchlistFiltersProps> = ({
  statusFilter,
  onStatusFilterChange,
  filteredCount
}) => {
  return (
    <Box sx={{ mb: 3, display: 'flex', gap: 2, alignItems: 'center' }}>
      <FormControl size="small" sx={{ minWidth: 200 }}>
        <InputLabel id="status-filter-label">Filter by Status</InputLabel>
        <Select
          labelId="status-filter-label"
          value={statusFilter}
          label="Filter by Status"
          onChange={onStatusFilterChange}
        >
          <MenuItem value="all">All</MenuItem>
          <MenuItem value={WatchlistStatus.Planned}>Planned</MenuItem>
          <MenuItem value={WatchlistStatus.Watching}>Watching</MenuItem>
          <MenuItem value={WatchlistStatus.Watched}>Watched</MenuItem>
          <MenuItem value={WatchlistStatus.Dropped}>Dropped</MenuItem>
        </Select>
      </FormControl>

      <Typography variant="body2" color="text.secondary">
        {filteredCount} {filteredCount === 1 ? 'movie' : 'movies'}
      </Typography>
    </Box>
  );
};

export default WatchlistFilters;

