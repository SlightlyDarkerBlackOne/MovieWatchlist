import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
  Chip,
  IconButton,
  Tooltip
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import StarIcon from '@mui/icons-material/Star';
import { Movie } from '../../types/movie.types';
import movieService from '../../services/movieService';
import { ROUTES } from '../../constants/routeConstants';
import { colors } from '../../theme';
import { formatVoteCount, getReleaseYear } from '../../utils/formatters';
import { useWatchlist } from '../../contexts/WatchlistContext';

interface MovieCardProps {
  movie: Movie;
}

const MovieCard: React.FC<MovieCardProps> = ({ movie }) => {
  const navigate = useNavigate();
  const { addToWatchlist, isInWatchlist: checkIsInWatchlist } = useWatchlist();
  const isInWatchlist = checkIsInWatchlist(movie.tmdbId);
  const posterUrl = movieService.getPosterUrl(movie.posterPath, 'medium');
  const releaseYear = getReleaseYear(movie.releaseDate);

  const handleCardClick = () => {
    navigate(ROUTES.MOVIE_DETAILS(movie.tmdbId));
  };

  const handleAddClick = (e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent card navigation
    addToWatchlist(movie);
  };

  return (
    <Card 
      onClick={handleCardClick}
      sx={{ 
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column',
        position: 'relative',
        cursor: 'pointer',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 6
        }
      }}
    >
      {/* Movie Poster */}
      <CardMedia
        component="img"
        height="300"
        image={posterUrl || '/placeholder-movie.png'}
        alt={movie.title}
        sx={{ objectFit: 'cover' }}
      />

      {/* Add to Watchlist Button */}
      {!isInWatchlist && (
        <Box sx={{ position: 'absolute', top: 8, right: 8 }}>
          <Tooltip title="Add to Watchlist">
            <IconButton
              onClick={handleAddClick}
              sx={{
                backgroundColor: 'rgba(0, 0, 0, 0.6)',
                color: 'white',
                '&:hover': {
                  backgroundColor: 'rgba(0, 0, 0, 0.8)',
                }
              }}
            >
              <AddIcon />
            </IconButton>
          </Tooltip>
        </Box>
      )}

      {/* Already in Watchlist Indicator */}
      {isInWatchlist && (
        <Box sx={{ position: 'absolute', top: 8, right: 8 }}>
          <Chip 
            label="In Watchlist" 
            color="success" 
            size="small"
            sx={{ fontWeight: 'bold' }}
          />
        </Box>
      )}

      {/* Movie Info */}
      <CardContent sx={{ flexGrow: 1, p: 2 }}>
        <Typography 
          variant="h6" 
          component="h3" 
          gutterBottom 
          sx={{ 
            fontWeight: 'bold',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            minHeight: '3.2em'
          }}
        >
          {movie.title}
        </Typography>

        {/* Rating with single star */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, gap: 0.5 }}>
          <StarIcon sx={{ color: colors.imdb.yellow, fontSize: '1.2rem' }} />
          <Typography variant="body1" sx={{ fontWeight: 600 }}>
            {movie.voteAverage.toFixed(1)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            /10
          </Typography>
          {movie.voteCount > 0 && (
            <Typography variant="caption" color="text.secondary" sx={{ ml: 0.5 }}>
              ({formatVoteCount(movie.voteCount)})
            </Typography>
          )}
        </Box>

        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {releaseYear}
        </Typography>

        <Typography 
          variant="body2" 
          color="text.secondary"
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 3,
            WebkitBoxOrient: 'vertical'
          }}
        >
          {movie.overview || 'No description available.'}
        </Typography>
      </CardContent>
    </Card>
  );
};

export default MovieCard;

