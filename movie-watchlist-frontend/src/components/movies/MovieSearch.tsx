import React, { useState } from 'react';
import {
  TextField,
  IconButton,
  InputAdornment,
  Paper
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';

interface MovieSearchProps {
  onSearch: (query: string) => void;
  placeholder?: string;
}

const MovieSearch: React.FC<MovieSearchProps> = ({ 
  onSearch, 
  placeholder = 'Search for movies...' 
}) => {
  const [searchQuery, setSearchQuery] = useState('');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      onSearch(searchQuery.trim());
    }
  };

  const handleClear = () => {
    setSearchQuery('');
    onSearch('');
  };

  return (
    <Paper 
      component="form" 
      onSubmit={handleSearch}
      sx={{
        p: '2px 4px',
        display: 'flex',
        alignItems: 'center',
        width: '100%',
        maxWidth: 600,
        mx: 'auto'
      }}
      elevation={2}
    >
      <TextField
        fullWidth
        placeholder={placeholder}
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        variant="standard"
        slotProps={{
          input: {
            disableUnderline: true,
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" />
              </InputAdornment>
            ),
            endAdornment: searchQuery && (
              <InputAdornment position="end">
                <IconButton
                  size="small"
                  onClick={handleClear}
                  aria-label="clear search"
                >
                  <ClearIcon />
                </IconButton>
              </InputAdornment>
            ),
          }
        }}
        sx={{ ml: 1, flex: 1 }}
      />
      <IconButton 
        type="submit" 
        sx={{ p: '10px' }} 
        aria-label="search"
        disabled={!searchQuery.trim()}
      >
        <SearchIcon />
      </IconButton>
    </Paper>
  );
};

export default MovieSearch;

