import React from 'react';
import { Card, CardContent, Skeleton, Box } from '@mui/material';

const MovieCardSkeleton: React.FC = () => {
  return (
    <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Skeleton variant="rectangular" height={300} />
      <CardContent sx={{ flexGrow: 1, p: 2 }}>
        <Skeleton variant="text" height={32} width="90%" sx={{ mb: 1 }} />
        <Skeleton variant="text" height={24} width="60%" sx={{ mb: 1 }} />
        <Skeleton variant="text" height={20} width="40%" />
        <Box sx={{ mt: 1 }}>
          <Skeleton variant="text" height={16} width="100%" />
          <Skeleton variant="text" height={16} width="100%" />
          <Skeleton variant="text" height={16} width="80%" />
        </Box>
      </CardContent>
    </Card>
  );
};

export default MovieCardSkeleton;


