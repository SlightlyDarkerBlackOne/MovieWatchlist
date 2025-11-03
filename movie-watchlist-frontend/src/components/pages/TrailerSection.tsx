import React, { useEffect, useRef } from 'react';
import { Container, Box } from '@mui/material';
import { MovieVideo } from '../../types/movie.types';
import { getYouTubeEmbedUrl } from '../../services/movieService';

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
          el.scrollIntoView({ behavior: 'smooth', block: 'center' });
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
        maxHeight: 'calc(100vh - 20px)',
        aspectRatio: '16 / 9',
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

