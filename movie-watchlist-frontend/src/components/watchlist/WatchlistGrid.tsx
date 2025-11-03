import React from 'react';
import {
  Box,
  Typography,
  CircularProgress,
  Grid2
} from '@mui/material';
import WatchlistItemCard from './WatchlistItemCard';
import { WatchlistItem } from '../../types/watchlist.types';

interface WatchlistGridProps {
  items: WatchlistItem[];
  loading?: boolean;
  onUpdate?: (item: WatchlistItem) => void;
  onDelete?: (itemId: number) => void;
  onEdit?: (item: WatchlistItem) => void;
}

const WatchlistGrid: React.FC<WatchlistGridProps> = ({
  items,
  loading = false,
  onUpdate,
  onDelete,
  onEdit
}) => {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (items.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary">
          Your watchlist is empty
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Start adding movies to track what you want to watch!
        </Typography>
      </Box>
    );
  }

  return (
    <Grid2 container spacing={3}>
      {items.map((item) => (
        <Grid2 key={item.id} size={{ xs: 12, sm: 6, md: 4, lg: 3, xl: 2 }}>
          <WatchlistItemCard
            item={item}
            onUpdate={onUpdate}
            onDelete={onDelete}
            onEdit={onEdit}
          />
        </Grid2>
      ))}
    </Grid2>
  );
};

export default WatchlistGrid;

