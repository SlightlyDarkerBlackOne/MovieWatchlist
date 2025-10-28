import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
  IconButton,
  Tooltip
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import AddIcon from '@mui/icons-material/Add';
import CheckIcon from '@mui/icons-material/Check';
import { Movie } from '../../types/movie.types';
import { getPosterUrl } from '../../services/movieService';
import { ROUTES } from '../../constants/routeConstants';
import { colors } from '../../theme';
import { formatVoteCount, getReleaseYear } from '../../utils/formatters';
import { useWatchlist } from '../../contexts/WatchlistContext';
import { getMovieCardAriaLabel, handleEnterKey } from '../../utils/accessibility';

interface MovieCardProps {
  movie: Movie;
  onAddToWatchlist?: (movie: Movie) => void;
}

const MovieCard: React.FC<MovieCardProps> = ({ movie, onAddToWatchlist }) => {
  const navigate = useNavigate();
  const { isInWatchlist: checkIsInWatchlist } = useWatchlist();
  const isInWatchlist = checkIsInWatchlist(movie.tmdbId);
  const posterUrl = getPosterUrl(movie.posterPath, 'medium');
  const releaseYear = getReleaseYear(movie.releaseDate);

  const handleCardClick = () => {
    navigate(ROUTES.MOVIE_DETAILS(movie.tmdbId));
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    handleEnterKey(event, handleCardClick);
  };

  const handleAddClick = (e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent card navigation
    if (onAddToWatchlist) {
      onAddToWatchlist(movie);
    }
  };

  return (
    <Card 
      onClick={handleCardClick}
      onKeyDown={handleKeyDown}
      role="button"
      tabIndex={0}
      aria-label={getMovieCardAriaLabel({ 
        title: movie.title, 
        voteAverage: movie.voteAverage, 
        releaseDate: movie.releaseDate 
      })}
      sx={{ 
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column',
        position: 'relative',
        cursor: 'pointer',
        transition: 'box-shadow 0.3s ease-in-out',
        '&:hover': {
          boxShadow: 6,
          '& img': {
            transform: 'scale(1.1)'
          }
        },
        '&:focus': {
          outline: '2px solid',
          outlineColor: 'primary.main',
          outlineOffset: '2px'
        }
      }}
    >
      {/* Movie Poster */}
      <Box sx={{ 
        overflow: 'hidden',
        position: 'relative',
        height: 300
      }}>
        <CardMedia
          component="img"
          height="300"
          image={posterUrl || '/placeholder-movie.png'}
          alt={movie.title}
          sx={{ 
            objectFit: 'cover',
            transition: 'transform 0.3s ease-in-out'
          }}
        />
      </Box>


      {/* Add to Watchlist Button */}
      <Box sx={{ position: 'absolute', top: 8, right: 8, zIndex: 1 }}>
        <Tooltip title={isInWatchlist ? 'Movie already in watchlist' : 'Add to watchlist'}>
          <span>
            <IconButton
              onClick={handleAddClick}
              disabled={isInWatchlist}
              aria-label={isInWatchlist ? 'Movie in watchlist' : 'Add to watchlist'}
              sx={{
                bgcolor: isInWatchlist ? 'success.main' : 'rgba(0, 0, 0, 0.6)',
                color: 'white',
                '&:hover': {
                  bgcolor: isInWatchlist ? 'success.main' : 'rgba(0, 0, 0, 0.8)',
                },
                '&.Mui-disabled': {
                  bgcolor: 'success.main',
                  color: 'white',
                  opacity: 0.8
                },
                transition: 'background-color 0.2s',
              }}
            >
              {isInWatchlist ? <CheckIcon /> : <AddIcon />}
            </IconButton>
          </span>
        </Tooltip>
      </Box>

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

