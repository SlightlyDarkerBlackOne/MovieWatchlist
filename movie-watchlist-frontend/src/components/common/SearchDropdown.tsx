import React, { useState, useEffect, useMemo } from 'react';
import {
  Autocomplete,
  TextField,
  Box,
  Typography,
  Avatar,
  CircularProgress,
  Paper,
  InputAdornment,
  styled,
  alpha
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import StarIcon from '@mui/icons-material/Star';
import { useNavigate } from 'react-router-dom';
import { debounce } from '@mui/material/utils';
import * as movieService from '../../services/movieService';
import { Movie } from '../../types/movie.types';
import { ROUTES } from '../../constants/routeConstants';
import { formatVoteCount } from '../../utils/formatters';

interface SearchDropdownProps {
  onFullSearch?: (query: string) => void;
}

const StyledAutocomplete = styled(Autocomplete<Movie | string, false, false, true>)(({ theme }) => ({
  '& .MuiAutocomplete-inputRoot': {
    color: 'inherit',
    backgroundColor: alpha(theme.palette.common.white, 0.15),
    '&:hover': {
      backgroundColor: alpha(theme.palette.common.white, 0.25),
    },
    borderRadius: theme.shape.borderRadius,
    padding: '2px 8px',
  },
  '& .MuiAutocomplete-input': {
    padding: `${theme.spacing(1)} ${theme.spacing(1)} ${theme.spacing(1)} 0`,
    paddingLeft: `calc(1em + ${theme.spacing(4)})`,
    transition: theme.transitions.create('width'),
    width: '100%',
    [theme.breakpoints.up('md')]: {
      width: '40ch',
      '&:focus': {
        width: '50ch',
      },
    },
  },
}));

const SearchDropdown: React.FC<SearchDropdownProps> = ({ onFullSearch }) => {
  const navigate = useNavigate();
  const [inputValue, setInputValue] = useState('');
  const [options, setOptions] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(false);

  // Debounced search function
  const debouncedSearch = useMemo(
    () =>
      debounce(async (searchQuery: string) => {
        if (searchQuery.length < 2) {
          setOptions([]);
          setLoading(false);
          return;
        }

        setLoading(true);
        try {
          const result = await movieService.searchMovies(searchQuery, 1);
          // Show only top 8 results in dropdown
          setOptions(result.movies.slice(0, 8));
        } catch (error) {
          console.error('Search error:', error);
          setOptions([]);
        } finally {
          setLoading(false);
        }
      }, 500), // 500ms debounce delay
    []
  );

  useEffect(() => {
    if (inputValue) {
      setLoading(true);
      debouncedSearch(inputValue);
    } else {
      setOptions([]);
    }
  }, [inputValue, debouncedSearch]);

  const handleSelectMovie = (movie: Movie | null) => {
    if (movie) {
      navigate(ROUTES.MOVIE_DETAILS(movie.tmdbId));
      setInputValue(''); // Clear search after selection
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' && inputValue.trim()) {
      event.preventDefault();
      if (onFullSearch) {
        onFullSearch(inputValue.trim());
        setInputValue(''); // Clear search after full search
      }
    }
  };

  return (
    <StyledAutocomplete
      freeSolo
      options={options}
      loading={loading}
      inputValue={inputValue}
      onInputChange={(event, newInputValue) => {
        setInputValue(newInputValue);
      }}
      onChange={(event, value) => {
        if (value && typeof value !== 'string') {
          handleSelectMovie(value as Movie);
        }
      }}
      getOptionLabel={(option: Movie | string) => {
        if (typeof option === 'string') {
          return option;
        }
        return option.title;
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder="Search movies…"
          variant="standard"
          onKeyDown={handleKeyDown}
          InputProps={{
            ...params.InputProps,
            disableUnderline: true,
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: 'inherit', ml: 1 }} />
              </InputAdornment>
            ),
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      renderOption={(props, option: Movie | string) => {
        // Type guard to ensure option is a Movie
        if (typeof option === 'string') {
          return null;
        }

        const movie = option;
        const { key, ...otherProps } = props as any;
        const posterUrl = movieService.getPosterUrl(movie.posterPath, 'small');
        const year = movie.releaseDate 
          ? new Date(movie.releaseDate).getFullYear() 
          : 'N/A';

        return (
          <li key={movie.tmdbId} {...otherProps}>
            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%', py: 1 }}>
              <Avatar
                src={posterUrl || undefined}
                variant="rounded"
                sx={{ width: 50, height: 75, mr: 2 }}
              >
                {movie.title[0]}
              </Avatar>
              <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                <Typography variant="body1" noWrap>
                  {movie.title}
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    {year}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <StarIcon sx={{ fontSize: 16, color: '#f5c518', mr: 0.5 }} />
                    <Typography variant="body2" color="text.secondary">
                      {movie.voteAverage.toFixed(1)}
                    </Typography>
                    {movie.voteCount > 0 && (
                      <Typography variant="body2" color="text.secondary" sx={{ ml: 0.5 }}>
                        ({formatVoteCount(movie.voteCount)})
                      </Typography>
                    )}
                  </Box>
                </Box>
              </Box>
            </Box>
          </li>
        );
      }}
      PaperComponent={({ children }) => (
        <Paper elevation={8} sx={{ mt: 1 }}>
          {children}
        </Paper>
      )}
      noOptionsText={
        inputValue.length < 2 
          ? "Type at least 2 characters to search" 
          : "No movies found"
      }
      sx={{ width: '100%', maxWidth: 600 }}
    />
  );
};

export default SearchDropdown;

