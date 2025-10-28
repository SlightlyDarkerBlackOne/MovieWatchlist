import React, { useRef, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Card,
  CardMedia,
  CardContent,
  Avatar,
} from '@mui/material';
import { CastMember } from '../../types/movie.types';
import { colors } from '../../theme/colors';
import * as movieService from '../../services/movieService';

interface TopCastCrewProps {
  topCast: CastMember[];
}

const TopCastCrew: React.FC<TopCastCrewProps> = ({ topCast }) => {
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const scrollContainer = scrollRef.current;
    if (!scrollContainer) return;

    const handleWheel = (e: WheelEvent) => {
      e.preventDefault();
      e.stopPropagation();
      scrollContainer.scrollLeft += e.deltaY;
    };

    scrollContainer.addEventListener('wheel', handleWheel, { passive: false });

    return () => {
      scrollContainer.removeEventListener('wheel', handleWheel);
    };
  }, []);

  if (topCast.length === 0) {
    return null;
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" sx={{ fontWeight: 700, mb: 3 }}>
        Top Cast
      </Typography>
      <Box
        ref={scrollRef}
        sx={{
          display: 'flex',
          gap: 2,
          overflowX: 'auto',
          pb: 2,
          '&::-webkit-scrollbar': { height: 8 },
          '&::-webkit-scrollbar-track': { bgcolor: colors.neutral.gray100 },
          '&::-webkit-scrollbar-thumb': { bgcolor: colors.neutral.gray600, borderRadius: 4 },
        }}
      >
        {topCast.map((cast) => {
          const profileUrl = movieService.getProfileUrl(cast.profilePath, 'small');          
          return (
            <Card
              key={`${cast.id}-${cast.castId}`}
              sx={{
                minWidth: 150,
                maxWidth: 150,
                cursor: 'pointer',
                transition: 'transform 0.2s',
                '&:hover': { transform: 'translateY(-4px)' },
              }}
            >
              <Box sx={{ position: 'relative', height: 200 }}>
                {profileUrl ? (
                  <CardMedia
                    component="img"
                    image={profileUrl}
                    alt={cast.name}
                    sx={{
                      height: '100%',
                      objectFit: 'cover',
                    }}
                  />
                ) : (
                  <Box
                    sx={{
                      height: '100%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      bgcolor: colors.neutral.gray200,
                    }}
                  >
                    <Avatar sx={{ width: 60, height: 60, bgcolor: colors.neutral.gray400 }}>
                      {cast.name[0]}
                    </Avatar>
                  </Box>
                )}
              </Box>
              <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
                <Typography
                  variant="subtitle2"
                  sx={{
                    fontWeight: 600,
                    mb: 0.5,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {cast.name}
                </Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{
                    display: 'block',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {cast.character}
                </Typography>
              </CardContent>
            </Card>
          );
        })}
      </Box>
    </Container>
  );
};

export default TopCastCrew;
