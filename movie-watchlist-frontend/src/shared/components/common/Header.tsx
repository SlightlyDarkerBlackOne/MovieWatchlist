import React from 'react';
import { AppBar, Toolbar, Box } from '@mui/material';
import HeaderLogo from './HeaderLogo';
import HeaderSearch from './HeaderSearch';
import HeaderAuthButtons from './HeaderAuthButtons';

interface HeaderProps {
  showAuth?: boolean;
  onSearch?: (query: string) => void;
  showSearch?: boolean;
}

const Header: React.FC<HeaderProps> = ({ 
  showAuth = false, 
  onSearch, 
  showSearch = false 
}) => {
  const handleSearch = (query: string) => {
    if (onSearch) {
      onSearch(query);
    }
  };

  return (
    <AppBar position="static" elevation={2}>
      <Toolbar>
        <HeaderLogo />
        {showSearch && <HeaderSearch onSearch={handleSearch} />}
        <Box sx={{ flexGrow: 1 }} />
        {showAuth && <HeaderAuthButtons />}
      </Toolbar>
    </AppBar>
  );
};

export default Header;

