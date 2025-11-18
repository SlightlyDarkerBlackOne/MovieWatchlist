import React from 'react';
import { Container, Box, Skeleton, Typography } from '@mui/material';

const MovieDetailsSkeleton: React.FC = () => {
  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', gap: 4, mb: 4 }}>
        <Skeleton variant="rectangular" width={300} height={450} />
        <Box sx={{ flexGrow: 1 }}>
          <Skeleton variant="text" height={48} width="80%" sx={{ mb: 2 }} />
          <Box sx={{ mb: 2 }}>
            <Skeleton variant="text" height={32} width="40%" sx={{ mb: 1 }} />
            <Skeleton variant="text" height={24} width="60%" />
          </Box>
          <Box sx={{ mb: 2 }}>
            <Skeleton variant="rectangular" width="100%" height={150} />
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <Skeleton variant="rectangular" width={120} height={36} />
            <Skeleton variant="rectangular" width={120} height={36} />
          </Box>
        </Box>
      </Box>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Genres
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Skeleton variant="rectangular" width={100} height={32} />
          <Skeleton variant="rectangular" width={100} height={32} />
          <Skeleton variant="rectangular" width={100} height={32} />
        </Box>
      </Box>
      <Box>
        <Typography variant="h6" gutterBottom>
          Top Cast
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          {[1, 2, 3, 4, 5].map((i) => (
            <Box key={i} sx={{ width: 120 }}>
              <Skeleton variant="circular" width={120} height={120} sx={{ mb: 1 }} />
              <Skeleton variant="text" width="100%" />
              <Skeleton variant="text" width="80%" />
            </Box>
          ))}
        </Box>
      </Box>
    </Container>
  );
};

export default MovieDetailsSkeleton;


