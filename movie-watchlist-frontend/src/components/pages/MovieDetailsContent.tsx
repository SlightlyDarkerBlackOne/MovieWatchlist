import React from 'react';
import { MovieDetails, MovieCredits, MovieVideo } from '../../types/movie.types';
import MovieMainDetails from '../movies/MovieMainDetails';
import MovieGenres from '../movies/MovieGenres';
import TopCastCrew from '../movies/TopCastCrew';
import TrailerSection from './TrailerSection';
import { findMainTrailer } from '../../services/movieService';

interface MovieDetailsContentProps {
  movieDetails: MovieDetails;
  videos: MovieVideo[];
  credits: MovieCredits | null;
  showTrailer: boolean;
  onToggleTrailer: () => void;
  onAddToWatchlist: () => void;
  onRemoveFromWatchlist: () => void;
  isInWatchlist: boolean;
}

const MovieDetailsContent: React.FC<MovieDetailsContentProps> = ({
  movieDetails,
  videos,
  credits,
  showTrailer,
  onToggleTrailer,
  onAddToWatchlist,
  onRemoveFromWatchlist,
  isInWatchlist
}) => {
  const mainTrailer = findMainTrailer(videos);

  return (
    <>
      <MovieMainDetails
        movieDetails={movieDetails}
        videos={videos}
        credits={credits}
        showTrailer={showTrailer}
        onToggleTrailer={onToggleTrailer}
        onAddToWatchlist={onAddToWatchlist}
        onRemoveFromWatchlist={onRemoveFromWatchlist}
        isInWatchlist={isInWatchlist}
      />

      <MovieGenres genres={movieDetails.genres} />

      <TrailerSection trailer={mainTrailer} show={showTrailer} />

      <TopCastCrew topCast={credits?.cast.slice(0, 10) || []} />
    </>
  );
};

export default MovieDetailsContent;

