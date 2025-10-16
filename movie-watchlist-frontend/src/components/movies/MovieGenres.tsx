import React from 'react';
import { Container, Box, Chip, Typography } from '@mui/material';

interface MovieGenresProps {
  genres: string[];
}

const MovieGenres: React.FC<MovieGenresProps> = ({ genres }) => {
  return (
    <Container maxWidth="xl" sx={{ pt: 4, pb: 0.2 }}>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', justifyContent: 'flex-start' }}>
        {genres && genres.length > 0 ? (
          genres.map((genre, index) => (
            <Chip
              key={`${genre}-${index}`}
              label={genre}
              sx={{
                bgcolor: 'transparent',
                color: '#424242',
                fontWeight: 500,
                borderRadius: '20px',
                fontSize: '1rem',
                height: '40px',
                px: 2,
                py: 1,
                border: '1px solid #666666',
                transition: 'all 0.3s ease',
                '&:hover': {
                  bgcolor: '#e0e0e0',
                  color: '#000000',
                  transform: 'translateX(-4px)',
                  boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
                },
              }}
            />
          ))
        ) : (
          <Typography variant="body2" color="text.secondary">
            No genres available
          </Typography>
        )}
      </Box>
    </Container>
  );
};

export default MovieGenres;
