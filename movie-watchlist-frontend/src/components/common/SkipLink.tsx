import React from 'react';
import { Link, Box } from '@mui/material';

const SkipLink: React.FC = () => {
  return (
    <Box
      component={Link}
      href="#main-content"
      sx={{
        position: 'absolute',
        top: '-40px',
        left: '50%',
        transform: 'translateX(-50%)',
        backgroundColor: 'primary.main',
        color: 'white',
        padding: '8px 16px',
        textDecoration: 'none',
        borderRadius: '4px',
        zIndex: 9999,
        fontSize: '14px',
        fontWeight: 500,
        '&:focus': {
          top: '8px',
          outline: '2px solid',
          outlineColor: 'white',
          outlineOffset: '2px'
        }
      }}
    >
      Skip to main content
    </Box>
  );
};

export default SkipLink;

