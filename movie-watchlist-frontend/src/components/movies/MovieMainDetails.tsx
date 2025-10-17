import React from 'react';
import {
  Container,
  Grid,
  Box,
  Typography,
  Card,
  CardMedia,
  IconButton,
  Button,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import StarIcon from '@mui/icons-material/Star';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import AddIcon from '@mui/icons-material/Add';
import { useNavigate } from 'react-router-dom';
import { MovieDetails, MovieVideo, MovieCredits, CrewMember } from '../../types/movie.types';
import { WatchlistStatus } from '../../types/watchlist.types';
import { colors } from '../../theme/colors';
import { formatVoteCount, formatRuntime } from '../../utils/formatters';
import movieService from '../../services/movieService';

interface MovieMainDetailsProps {
  movieDetails: MovieDetails;
  videos: MovieVideo[];
  credits: MovieCredits | null;
  showTrailer: boolean;
  onToggleTrailer: () => void;
  onAddToWatchlist: () => void;
  onRemoveFromWatchlist: () => void;
  isInWatchlist: boolean;
}

const MovieMainDetails: React.FC<MovieMainDetailsProps> = ({
  movieDetails,
  videos,
  credits,
  showTrailer,
  onToggleTrailer,
  onAddToWatchlist,
  onRemoveFromWatchlist,
  isInWatchlist,
}) => {
  const navigate = useNavigate();
  const [isHoveringWatchlistBtn, setIsHoveringWatchlistBtn] = React.useState(false);
  const posterUrl = movieService.getPosterUrl(movieDetails.posterPath, 'large');
  const mainTrailer = movieService.findMainTrailer(videos);
  const director = credits?.crew.find((c: CrewMember) => c.job === 'Director');
  const writers = credits?.crew.filter((c: CrewMember) => 
    c.department === 'Writing' && (c.job === 'Writer' || c.job === 'Screenplay')
  ).slice(0, 3);

  return (
    <Box
      sx={{
        position: 'relative',
        minHeight: '60vh',
        backgroundImage: movieDetails.backdropPath 
          ? `url(${movieService.getBackdropUrl(movieDetails.backdropPath, 'original')})`
          : 'none',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'linear-gradient(to right, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.5) 50%, rgba(0,0,0,0.9) 100%)',
        },
      }}
    >
      <Container maxWidth="xl" sx={{ position: 'relative', zIndex: 1, py: 4 }}>
        <IconButton
          onClick={() => navigate(-1)}
          sx={{
            color: 'white',
            mb: 2,
            bgcolor: 'rgba(0,0,0,0.5)',
            '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
          }}
        >
          <ArrowBackIcon />
        </IconButton>

        <Grid container spacing={4}>
          {/* Left: Poster */}
          <Grid item xs={12} md={3}>
            <Card elevation={8}>
              {posterUrl ? (
                <CardMedia
                  component="img"
                  image={posterUrl}
                  alt={movieDetails.title}
                  sx={{ width: '100%', height: 'auto' }}
                />
              ) : (
                <Box sx={{ height: 450, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: colors.neutral.gray300 }}>
                  <Typography variant="h6" color="text.secondary">No Poster</Typography>
                </Box>
              )}
            </Card>
          </Grid>

          {/* Right: Movie Info */}
          <Grid item xs={12} md={9}>
            <Box sx={{ color: 'white' }}>
              {/* Title and Year */}
              <Typography variant="h3" component="h1" sx={{ fontWeight: 700, mb: 1 }}>
                {movieDetails.title}
                {movieDetails.releaseDate && (
                  <Typography component="span" variant="h4" sx={{ color: '#ccc', ml: 2 }}>
                    ({new Date(movieDetails.releaseDate).getFullYear()})
                  </Typography>
                )}
              </Typography>

              {/* Tagline */}
              {movieDetails.tagline && (
                <Typography variant="h6" sx={{ fontStyle: 'italic', color: '#ddd', mb: 2 }}>
                  "{movieDetails.tagline}"
                </Typography>
              )}

              {/* Meta Info */}
              <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap', alignItems: 'flex-start' }}>
                {/* Rating */}
                {movieDetails.voteAverage > 0 && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <StarIcon sx={{ color: colors.imdb.yellow, fontSize: '1.8rem' }} />
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0 }}>
                      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5 }}>
                        <Typography variant="h5" sx={{ fontWeight: 600 }}>
                          {movieDetails.voteAverage.toFixed(1)}
                        </Typography>
                        <Typography variant="h6" sx={{ color: '#ccc' }}>
                          /10
                        </Typography>
                      </Box>
                      {movieDetails.voteCount > 0 && (
                        <Typography variant="h6" sx={{ color: '#ccc', fontSize: '0.875rem' }}>
                          ({formatVoteCount(movieDetails.voteCount)})
                        </Typography>
                      )}
                    </Box>
                  </Box>
                )}

                {/* Runtime */}
                {movieDetails.runtime && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <AccessTimeIcon />
                    <Typography variant="body1">
                      {formatRuntime(movieDetails.runtime)}
                    </Typography>
                  </Box>
                )}
              </Box>

              {/* Action Buttons */}
              <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
                {mainTrailer && (
                  <Button
                    variant="contained"
                    startIcon={<PlayArrowIcon />}
                    onClick={onToggleTrailer}
                    sx={{
                      bgcolor: colors.imdb.yellow,
                      color: colors.neutral.black,
                      fontWeight: 600,
                      px: 3,
                      '&:hover': { bgcolor: colors.imdb.yellowDark },
                    }}
                  >
                    {showTrailer ? 'Hide Trailer' : 'Play Trailer'}
                  </Button>
                )}
                <Button
                  variant="outlined"
                  startIcon={<AddIcon />}
                  onClick={isInWatchlist ? onRemoveFromWatchlist : onAddToWatchlist}
                  onMouseEnter={() => setIsHoveringWatchlistBtn(true)}
                  onMouseLeave={() => setIsHoveringWatchlistBtn(false)}
                  sx={{
                    borderColor: 'white',
                    color: 'white',
                    fontWeight: 600,
                    px: 3,
                    opacity: isInWatchlist ? 0.7 : 1,
                    transition: 'all 0.3s ease',
                    '&:hover': {
                      borderColor: isInWatchlist ? '#f44336' : colors.imdb.yellow,
                      bgcolor: isInWatchlist ? 'rgba(244, 67, 54, 0.8)' : colors.overlay.light,
                      opacity: 1,
                      color: 'white',
                    },
                  }}
                >
                  {isInWatchlist 
                    ? (isHoveringWatchlistBtn ? 'Remove from Watchlist' : 'In Watchlist')
                    : 'Add to Watchlist'
                  }
                </Button>
              </Box>

              {/* Overview */}
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                Overview
              </Typography>
              <Typography variant="body1" sx={{ lineHeight: 1.7, mb: 3 }}>
                {movieDetails.overview || 'No overview available.'}
              </Typography>

              {/* Director and Writers */}
              <Grid container spacing={2}>
                {director && (
                  <Grid item xs={12} sm={6} md={4}>
                    <Typography variant="caption" sx={{ color: '#ccc' }}>
                      Director
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 600 }}>
                      {director.name}
                    </Typography>
                  </Grid>
                )}
                {writers && writers.length > 0 && (
                  <Grid item xs={12} sm={6} md={4}>
                    <Typography variant="caption" sx={{ color: '#ccc' }}>
                      Writers
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 600 }}>
                      {writers.map(w => w.name).join(', ')}
                    </Typography>
                  </Grid>
                )}
              </Grid>
            </Box>
          </Grid>
        </Grid>
      </Container>
    </Box>
  );
};

export default MovieMainDetails;
