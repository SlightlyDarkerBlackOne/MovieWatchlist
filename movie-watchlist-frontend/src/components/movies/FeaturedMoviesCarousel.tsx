import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, IconButton, Typography, Chip, Tooltip } from '@mui/material';
import ArrowBackIosNewIcon from '@mui/icons-material/ArrowBackIosNew';
import ArrowForwardIosIcon from '@mui/icons-material/ArrowForwardIos';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import AddIcon from '@mui/icons-material/Add';
import CheckIcon from '@mui/icons-material/Check';
import StarIcon from '@mui/icons-material/Star';
import { Movie } from '../../types/movie.types';
import * as movieService from '../../services/movieService';
import { colors } from '../../theme';
import { useWatchlist } from '../../contexts/WatchlistContext';

interface FeaturedMoviesCarouselProps {
  movies: Movie[];
  onAddToWatchlist: (movie: Movie) => void;
  autoRotate?: boolean;
  rotateInterval?: number;
}

const FeaturedMoviesCarousel: React.FC<FeaturedMoviesCarouselProps> = ({
  movies,
  onAddToWatchlist,
  autoRotate = true,
  rotateInterval = 5000,
}) => {
  const navigate = useNavigate();
  const { isInWatchlist } = useWatchlist();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isHovered, setIsHovered] = useState(false);

  const currentMovie = movies[currentIndex];
  const isCurrentMovieInWatchlist = currentMovie ? isInWatchlist(currentMovie.tmdbId) : false;

  const handleBackgroundClick = () => {
    if (currentMovie) {
      navigate(`/movies/${currentMovie.tmdbId}`);
    }
  };

  // Auto-rotation
  useEffect(() => {
    if (!autoRotate || isHovered || movies.length <= 1) return;

    const timer = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % movies.length);
    }, rotateInterval);

    return () => clearInterval(timer);
  }, [autoRotate, isHovered, movies.length, rotateInterval]);

  const handlePrevious = () => {
    setCurrentIndex((prev) => (prev - 1 + movies.length) % movies.length);
  };

  const handleNext = () => {
    setCurrentIndex((prev) => (prev + 1) % movies.length);
  };

  if (!currentMovie || movies.length === 0) {
    return null;
  }

  // Use backdrop if available, fallback to poster
  const backdropUrl = currentMovie.backdropPath 
    ? movieService.getBackdropUrl(currentMovie.backdropPath, 'original')
    : currentMovie.posterPath 
    ? movieService.getPosterUrl(currentMovie.posterPath, 'original')
    : null;

  return (
    <Box
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      sx={{
        position: 'relative',
        width: '100%',
        height: { xs: '70vh', sm: '80vh', md: '90vh' },
        overflow: 'hidden',
        bgcolor: 'black',
        mb: 4,
      }}
    >
      {/* Background Image with Gradient Overlay */}
      <Box
        onClick={handleBackgroundClick}
        sx={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundImage: backdropUrl ? `url(${backdropUrl})` : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          cursor: 'pointer',
          transition: 'transform 0.3s ease',
          '&:hover': {
            transform: 'scale(1.02)',
          },
          '&::after': {
            content: '""',
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'linear-gradient(to right, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.3) 50%, rgba(0,0,0,0.9) 100%), linear-gradient(to top, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0) 50%)',
          },
        }}
      />

      {/* Navigation Arrows */}
      {movies.length > 1 && (
        <>
          <IconButton
            onClick={handlePrevious}
            sx={{
              position: 'absolute',
              left: { xs: 8, md: 24 },
              top: '50%',
              transform: 'translateY(-50%)',
              zIndex: 2,
              bgcolor: 'rgba(0,0,0,0.5)',
              color: 'white',
              width: { xs: 40, md: 56 },
              height: { xs: 40, md: 56 },
              '&:hover': {
                bgcolor: 'rgba(0,0,0,0.8)',
              },
            }}
          >
            <ArrowBackIosNewIcon />
          </IconButton>

          <IconButton
            onClick={handleNext}
            sx={{
              position: 'absolute',
              right: { xs: 8, md: 24 },
              top: '50%',
              transform: 'translateY(-50%)',
              zIndex: 2,
              bgcolor: 'rgba(0,0,0,0.5)',
              color: 'white',
              width: { xs: 40, md: 56 },
              height: { xs: 40, md: 56 },
              '&:hover': {
                bgcolor: 'rgba(0,0,0,0.8)',
              },
            }}
          >
            <ArrowForwardIosIcon />
          </IconButton>
        </>
      )}

      {/* Movie Details Overlay */}
      <Box
        sx={{
          position: 'absolute',
          bottom: 0,
          left: 0,
          right: 0,
          zIndex: 1,
          p: { xs: 2, md: 4 },
          color: 'white',
        }}
      >
        <Box
          onClick={(e) => e.stopPropagation()}
          sx={{ maxWidth: '800px' }}>
          {/* Movie Title */}
          <Typography
            variant="h2"
            component="h1"
            sx={{
              fontSize: { xs: '2rem', sm: '3rem', md: '4rem' },
              fontWeight: 700,
              mb: 2,
              textShadow: '2px 2px 4px rgba(0,0,0,0.8)',
            }}
          >
            {currentMovie.title}
          </Typography>

          {/* Rating and Year */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            {currentMovie.voteAverage > 0 && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <StarIcon sx={{ color: colors.imdb.yellow, fontSize: '1.5rem' }} />
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  {currentMovie.voteAverage.toFixed(1)}
                </Typography>
              </Box>
            )}
            {currentMovie.releaseDate && (
              <Typography variant="h6" sx={{ color: 'rgba(255,255,255,0.8)' }}>
                {new Date(currentMovie.releaseDate).getFullYear()}
              </Typography>
            )}
          </Box>

          {/* Overview */}
          <Typography
            variant="body1"
            sx={{
              fontSize: { xs: '0.9rem', md: '1.1rem' },
              mb: 3,
              maxWidth: '600px',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: { xs: 3, md: 4 },
              WebkitBoxOrient: 'vertical',
              textShadow: '1px 1px 2px rgba(0,0,0,0.8)',
              lineHeight: 1.6,
            }}
          >
            {currentMovie.overview}
          </Typography>

          {/* Action Buttons */}
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Tooltip title={isCurrentMovieInWatchlist ? 'Movie already in watchlist' : 'Add to watchlist'}>
              <span>
                <IconButton
                  onClick={() => onAddToWatchlist(currentMovie)}
                  disabled={isCurrentMovieInWatchlist}
                  sx={{
                    bgcolor: isCurrentMovieInWatchlist ? 'rgba(76, 175, 80, 0.9)' : 'rgba(255,255,255,0.9)',
                    color: isCurrentMovieInWatchlist ? 'white' : 'black',
                    px: 3,
                    py: 1,
                    borderRadius: 1,
                    '&:hover': {
                      bgcolor: isCurrentMovieInWatchlist ? 'rgba(76, 175, 80, 1)' : 'white',
                    },
                    '&.Mui-disabled': {
                      bgcolor: 'rgba(76, 175, 80, 0.9)',
                      color: 'white',
                    },
                    display: 'flex',
                    gap: 1,
                  }}
                >
                  {isCurrentMovieInWatchlist ? <CheckIcon /> : <AddIcon />}
                  <Typography variant="button" sx={{ fontWeight: 600 }}>
                    {isCurrentMovieInWatchlist ? 'In Watchlist' : 'Add to Watchlist'}
                  </Typography>
                </IconButton>
              </span>
            </Tooltip>
          </Box>

          {/* Carousel Indicators */}
          {movies.length > 1 && (
            <Box
              sx={{
                display: 'flex',
                gap: 1,
                mt: 3,
                justifyContent: { xs: 'center', md: 'flex-start' },
              }}
            >
              {movies.map((_, index) => (
                <Box
                  key={index}
                  onClick={() => setCurrentIndex(index)}
                  sx={{
                    width: 40,
                    height: 4,
                    bgcolor: index === currentIndex ? 'white' : 'rgba(255,255,255,0.3)',
                    cursor: 'pointer',
                    transition: 'all 0.3s',
                    '&:hover': {
                      bgcolor: index === currentIndex ? 'white' : 'rgba(255,255,255,0.5)',
                    },
                  }}
                />
              ))}
            </Box>
          )}
        </Box>
      </Box>
    </Box>
  );
};

export default FeaturedMoviesCarousel;

