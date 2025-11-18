import React, { useEffect, useRef } from 'react';
import { Container, Box } from '@mui/material';
import { MovieVideo } from '../model/movie.types';
import { getYouTubeEmbedUrl } from '../lib/tmdbUtils';
import { TRAILER_CONSTANTS } from '../../../shared/constants/appConstants';

interface TrailerSectionProps {
  trailer: MovieVideo | null;
  show: boolean;
}

const TrailerSection: React.FC<TrailerSectionProps> = ({ trailer, show }) => {
  const trailerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (show && trailer && trailerRef.current) {
      requestAnimationFrame(() => {
        const el = trailerRef.current as unknown as { scrollIntoView?: (opts?: any) => void } | null;
        if (el && typeof el.scrollIntoView === 'function') {
          el.scrollIntoView({ 
            behavior: TRAILER_CONSTANTS.SCROLL_BEHAVIOR, 
            block: TRAILER_CONSTANTS.SCROLL_BLOCK 
          });
        }
      });
    }
  }, [show, trailer]);

  if (!show || !trailer) return null;

  return (
    <Container ref={trailerRef} maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ 
        position: 'relative', 
        width: '100%', 
        maxHeight: TRAILER_CONSTANTS.MAX_HEIGHT,
        aspectRatio: TRAILER_CONSTANTS.ASPECT_RATIO,
        overflow: 'hidden',
        margin: '0 auto'
      }}>
        <iframe
          src={getYouTubeEmbedUrl(trailer.key)}
          title={trailer.name}
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
          style={{
            width: '100%',
            height: '100%',
            border: 'none',
          }}
        />
      </Box>
    </Container>
  );
};

export default TrailerSection;

