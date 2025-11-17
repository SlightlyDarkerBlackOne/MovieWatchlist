import React, { cloneElement, ReactElement } from 'react';
import { Box } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import Header from '../common/Header';
import { ROUTES } from '../../constants/routeConstants';
import { colors } from '../../theme';

interface MainLayoutProps {
  children: React.ReactNode;
  showSearch?: boolean;
}

const MainLayout: React.FC<MainLayoutProps> = ({ 
  children, 
  showSearch = false
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleSearch = (query: string) => {
    if (location.pathname === ROUTES.MOVIES) {
      navigate(`${ROUTES.MOVIES}?search=${encodeURIComponent(query)}`);
    } else {
      navigate(`${ROUTES.MOVIES}?search=${encodeURIComponent(query)}`);
    }
  };

  // Clone children and pass the search handler if it's a valid React element
  const childrenWithProps = React.isValidElement(children) && showSearch
    ? cloneElement(children as ReactElement, { onHeaderSearch: handleSearch } as any)
    : children;

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: colors.imdb.backgroundLight }}>
      <Header 
        showAuth={true}
        onSearch={showSearch ? handleSearch : undefined} 
        showSearch={showSearch} 
      />
      {childrenWithProps}
    </Box>
  );
};

export default MainLayout;

