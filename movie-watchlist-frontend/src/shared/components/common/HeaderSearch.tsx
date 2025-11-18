import React from 'react';
import { Box } from '@mui/material';
import SearchDropdown from './SearchDropdown';

interface HeaderSearchProps {
  onSearch: (query: string) => void;
}

/**
 * Header Search Component
 * 
 * Wraps the search dropdown with consistent styling for the header.
 */
const HeaderSearch: React.FC<HeaderSearchProps> = ({ onSearch }) => {
  return (
    <Box sx={{ width: '100%', maxWidth: 600, mr: 3, ml: { xs: 2, sm: 4, md: 8 } }}>
      <SearchDropdown onFullSearch={onSearch} />
    </Box>
  );
};

export default HeaderSearch;

