import React from 'react';
import { Box, Typography } from '@mui/material';
import { InboxOutlined } from '@mui/icons-material';

interface EmptyStateProps {
  message: string;
  icon?: React.ReactNode;
}

const EmptyState: React.FC<EmptyStateProps> = ({ message, icon }) => {
  return (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      py={8}
      px={2}
    >
      {icon || <InboxOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />}
      <Typography variant="h6" color="text.secondary" align="center">
        {message}
      </Typography>
    </Box>
  );
};

export default EmptyState;


