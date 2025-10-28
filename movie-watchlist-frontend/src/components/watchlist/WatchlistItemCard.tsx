import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
  Chip,
  IconButton,
  Rating,
  Menu,
  MenuItem,
  Tooltip,
  Button
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import StarIcon from '@mui/icons-material/Star';
import { WatchlistItem, WatchlistStatus } from '../../types/watchlist.types';
import { ROUTES } from '../../constants/routeConstants';
import * as movieService from '../../services/movieService';
import * as watchlistService from '../../services/watchlistService';
import { colors } from '../../theme/colors';
import { formatVoteCount } from '../../utils/formatters';

interface WatchlistItemCardProps {
  item: WatchlistItem;
  onUpdate?: (item: WatchlistItem) => void;
  onDelete?: (itemId: number) => void;
  onEdit?: (item: WatchlistItem) => void;
}

const WatchlistItemCard: React.FC<WatchlistItemCardProps> = ({ 
  item, 
  onUpdate, 
  onDelete,
  onEdit
}) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleCardClick = () => {
    if (item.movie?.tmdbId) {
      navigate(ROUTES.MOVIE_DETAILS(item.movie.tmdbId));
    }
  };

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    event.stopPropagation(); // Prevent card click
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleToggleFavorite = async (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onUpdate) {
      const updatedItem = { ...item, isFavorite: !item.isFavorite };
      await onUpdate(updatedItem);
    }
  };

  const handleEdit = (event?: React.MouseEvent) => {
    event?.stopPropagation();
    handleMenuClose();
    if (onEdit) {
      onEdit(item);
    }
  };

  const handleDelete = () => {
    handleMenuClose();
    if (onDelete) {
      onDelete(item.id);
    }
  };

  const posterUrl = movieService.getPosterUrl(item.movie?.posterPath || null, 'medium');
  const releaseYear = item.movie?.releaseDate 
    ? new Date(item.movie.releaseDate).getFullYear() 
    : 'N/A';
  
  const getUserRating = (): number | undefined => {
    if (!item.userRating) return undefined;
    if (typeof item.userRating === 'object' && item.userRating !== null && 'value' in item.userRating) {
      return (item.userRating as { value: number }).value;
    }
    return item.userRating as number;
  };
  
  const userRatingValue = getUserRating();

  return (
    <Card 
      onClick={handleCardClick}
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
          alt={item.movie?.title || 'Movie'}
          sx={{ 
            objectFit: 'cover',
            transition: 'transform 0.3s ease-in-out'
          }}
        />
      </Box>

      {/* Status Chip */}
      <Box sx={{ position: 'absolute', top: 8, left: 8 }}>
        <Chip 
          label={watchlistService.getStatusLabel(item.status)} 
          color={watchlistService.getStatusColor(item.status)}
          size="small"
          sx={{ fontWeight: 'bold' }}
        />
      </Box>

      {/* Actions Menu */}
      <Box sx={{ position: 'absolute', top: 8, right: 8 }}>
        <IconButton
          onClick={handleMenuClick}
          sx={{
            backgroundColor: 'rgba(0, 0, 0, 0.6)',
            color: 'white',
            '&:hover': {
              backgroundColor: 'rgba(0, 0, 0, 0.8)',
            }
          }}
          size="small"
        >
          <MoreVertIcon />
        </IconButton>
        <Menu
          anchorEl={anchorEl}
          open={open}
          onClose={handleMenuClose}
          onClick={(e) => e.stopPropagation()}
          slotProps={{
            backdrop: {
              onClick: (e) => {
                e.stopPropagation();
              }
            }
          }}
        >
          <MenuItem onClick={handleEdit}>
            <EditIcon sx={{ mr: 1 }} fontSize="small" />
            Edit
          </MenuItem>
          <MenuItem onClick={handleDelete}>
            <DeleteIcon sx={{ mr: 1 }} fontSize="small" />
            Remove
          </MenuItem>
        </Menu>
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
          {item.movie?.title || 'Unknown Title'}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <StarIcon sx={{ color: colors.imdb.yellow, fontSize: '1.2rem' }} />
            <Typography variant="body1" sx={{ fontWeight: 600 }}>
              {item.movie?.voteAverage?.toFixed(1) || 'N/A'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              /10
            </Typography>
            {item.movie?.voteCount && item.movie.voteCount > 0 && (
              <Typography variant="caption" color="text.secondary" sx={{ ml: 0.5 }}>
                ({formatVoteCount(item.movie.voteCount)})
              </Typography>
            )}
          </Box>
          
          <Tooltip title={item.isFavorite ? "Remove from favorites" : "Add to favorites"}>
            <IconButton 
              onClick={handleToggleFavorite}
              size="small"
              color={item.isFavorite ? "error" : "default"}
            >
              {item.isFavorite ? <FavoriteIcon /> : <FavoriteBorderIcon />}
            </IconButton>
          </Tooltip>
        </Box>
        
        <Box sx={{ mb: 1 }}>
          {userRatingValue ? (
            <Button
              variant="outlined"
              size="small"
              onClick={(e) => handleEdit(e)}
              sx={{
                textTransform: 'none',
                borderColor: 'primary.main',
                color: 'primary.main'
              }}
            >
              Your Rating: {userRatingValue}/10
            </Button>
          ) : (
            <Button
              variant="outlined"
              size="small"
              onClick={(e) => handleEdit(e)}
              sx={{
                textTransform: 'none',
                borderColor: 'text.secondary',
                color: 'text.secondary'
              }}
            >
              Rate Now
            </Button>
          )}
        </Box>

        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {releaseYear}
        </Typography>

        {item.notes && (
          <Typography 
            variant="body2" 
            color="text.secondary"
            sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              fontStyle: 'italic'
            }}
          >
            "{item.notes}"
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default WatchlistItemCard;

