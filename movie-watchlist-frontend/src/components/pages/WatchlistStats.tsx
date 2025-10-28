import React from 'react';
import { Box, Paper, Typography, Grid } from '@mui/material';
import { useWatchlistStatistics } from '../../hooks/useWatchlistOperations';

interface WatchlistStatsProps {
  userId: number | undefined;
}

const WatchlistStats: React.FC<WatchlistStatsProps> = ({ userId }) => {
  const { data: stats, isLoading } = useWatchlistStatistics(userId);

  if (isLoading || !stats) {
    return null;
  }

  return (
    <Box sx={{ mb: 4 }}>
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="h4" color="primary" fontWeight="bold">
              {stats.totalMovies || 0}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Total Movies
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="h4" color="success.main" fontWeight="bold">
              {stats.watchedMovies || 0}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Watched
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="h4" color="secondary.main" fontWeight="bold">
              {stats.plannedMovies || 0}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Planned
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, textAlign: 'center' }}>
            <Typography variant="h4" color="warning.main" fontWeight="bold">
              {stats.averageUserRating ? stats.averageUserRating.toFixed(1) : '0.0'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Your Avg Rating
            </Typography>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default WatchlistStats;

