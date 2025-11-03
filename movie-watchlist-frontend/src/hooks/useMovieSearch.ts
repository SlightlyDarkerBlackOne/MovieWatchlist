import { useState, useEffect, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useSearchMoviesQuery } from '../store/api/moviesApi';

export const useMovieSearch = () => {
  const [searchParams] = useSearchParams();
  const [searchQuery, setSearchQuery] = useState('');
  const searchResultsRef = useRef<HTMLDivElement>(null);

  const { 
    data: searchResults, 
    isLoading: searchLoading, 
    error: searchError
  } = useSearchMoviesQuery(
    { query: searchQuery, page: 1 },
    { skip: !searchQuery.trim() }
  );

  useEffect(() => {
    const urlSearchQuery = searchParams.get('search');
    if (urlSearchQuery && urlSearchQuery !== searchQuery) {
      setSearchQuery(urlSearchQuery);
    }
  }, [searchParams, searchQuery]);

  useEffect(() => {
    if (searchResults && searchQuery) {
      setTimeout(() => {
        searchResultsRef.current?.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'start' 
        });
      }, 100);
    }
  }, [searchResults, searchQuery]);

  return {
    searchQuery,
    setSearchQuery,
    searchResults,
    searchLoading,
    searchError,
    searchResultsRef
  };
};

