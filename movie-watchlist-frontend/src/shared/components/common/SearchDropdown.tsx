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
import { useLazySearchMoviesQuery } from '../../../features/movies/api/moviesApi';
import { getPosterUrl } from '../../../features/movies/lib/tmdbUtils';
import { Movie } from '../../../features/movies/model/movie.types';
import { ROUTES } from '../../constants/routeConstants';
import { formatVoteCount } from '../../lib/formatters';
import {
  SEARCH_CONSTANTS,
  KEYBOARD_KEYS,
  YEAR_DISPLAY,
  POSTER_SIZES,
  UI_CONSTANTS,
} from '../../constants/appConstants';

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
  const [searchMovies, { isLoading }] = useLazySearchMoviesQuery();

  const debouncedSearch = useMemo(
    () =>
      debounce(async (searchQuery: string) => {
        if (searchQuery.length < SEARCH_CONSTANTS.MIN_QUERY_LENGTH) {
          setOptions([]);
          return;
        }

        try {
          const result = await searchMovies({ query: searchQuery, page: 1 }).unwrap();
          setOptions(result.movies.slice(0, SEARCH_CONSTANTS.MAX_DROPDOWN_RESULTS));
        } catch (error) {
          setOptions([]);
        }
      }, SEARCH_CONSTANTS.DEBOUNCE_DELAY_MS),
    [searchMovies]
  );

  useEffect(() => {
    if (inputValue) {
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
    if (event.key === KEYBOARD_KEYS.ENTER && inputValue.trim()) {
      event.preventDefault();
      if (onFullSearch) {
        onFullSearch(inputValue.trim());
        setInputValue('');
      }
    }
  };

  return (
    <StyledAutocomplete
      freeSolo
      options={options}
      loading={isLoading}
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
          placeholder={SEARCH_CONSTANTS.PLACEHOLDER}
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
                {isLoading ? <CircularProgress color="inherit" size={UI_CONSTANTS.CIRCULAR_PROGRESS.SMALL_SIZE} /> : null}
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
        const posterUrl = getPosterUrl(movie.posterPath, POSTER_SIZES.SMALL);
        const year = movie.releaseDate 
          ? new Date(movie.releaseDate).getFullYear() 
          : YEAR_DISPLAY.NOT_AVAILABLE;

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
        inputValue.length < SEARCH_CONSTANTS.MIN_QUERY_LENGTH 
          ? SEARCH_CONSTANTS.MIN_CHARACTERS_MESSAGE 
          : SEARCH_CONSTANTS.NO_RESULTS_MESSAGE
      }
      sx={{ width: '100%', maxWidth: 600 }}
    />
  );
};

export default SearchDropdown;

