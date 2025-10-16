import React, { useState, useEffect } from 'react';
import {
  Container,
  Box,
  Typography,
  Alert,
  Tabs,
  Tab,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  SelectChangeEvent,
  Button
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { WatchlistGrid } from '../components/watchlist';
import watchlistService from '../services/watchlistService';
import { WatchlistItem, WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from '../contexts/AuthContext';
import { useWatchlist } from '../contexts/WatchlistContext';
import { ROUTES } from '../constants/routeConstants';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`watchlist-tabpanel-${index}`}
      aria-labelledby={`watchlist-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const WatchlistPage: React.FC = () => {
  const { user } = useAuth();
  const { removeFromWatchlistIds } = useWatchlist();
  const navigate = useNavigate();
  const [watchlist, setWatchlist] = useState<WatchlistItem[]>([]);
  const [filteredItems, setFilteredItems] = useState<WatchlistItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all');

  useEffect(() => {
    if (user?.id) {
      loadWatchlist();
    }
  }, [user]);

  // Refresh watchlist when component mounts or becomes visible
  useEffect(() => {
    const handleFocus = () => {
      if (user?.id) {
        loadWatchlist();
      }
    };

    window.addEventListener('focus', handleFocus);
    return () => window.removeEventListener('focus', handleFocus);
  }, [user]);

  useEffect(() => {
    filterWatchlist();
  }, [watchlist, activeTab, statusFilter]);

  const loadWatchlist = async () => {
    if (!user?.id) return;
    
    setLoading(true);
    setError(null);
    try {
      const data = await watchlistService.getUserWatchlist(user.id);
      setWatchlist(data);
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to load watchlist');
    } finally {
      setLoading(false);
    }
  };

  const filterWatchlist = () => {
    let filtered = [...watchlist];

    // Filter by tab
    if (activeTab === 1) {
      // Favorites
      filtered = filtered.filter(item => item.isFavorite);
    } else if (activeTab === 2) {
      // Watched
      filtered = filtered.filter(item => item.status === WatchlistStatus.Watched);
    }

    // Filter by status dropdown
    if (statusFilter !== 'all') {
      filtered = filtered.filter(item => item.status === statusFilter);
    }

    setFilteredItems(filtered);
  };

  const handleUpdateItem = async (updatedItem: WatchlistItem) => {
    if (!user?.id) return;

    try {
      await watchlistService.updateWatchlistItem(user.id, updatedItem.id, {
        isFavorite: updatedItem.isFavorite,
        status: updatedItem.status,
        userRating: updatedItem.userRating,
        notes: updatedItem.notes
      });
      
      // Update local state
      setWatchlist(prev => 
        prev.map(item => item.id === updatedItem.id ? updatedItem : item)
      );
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to update item');
    }
  };

  const handleDeleteItem = async (itemId: number) => {
    if (!user?.id) return;

    if (!window.confirm('Are you sure you want to remove this from your watchlist?')) {
      return;
    }

    try {
      // Find the movie's TMDB ID before removing
      const itemToDelete = watchlist.find(item => item.id === itemId);
      const tmdbId = itemToDelete?.movie?.tmdbId;
      
      await watchlistService.removeFromWatchlist(user.id, itemId);
      setWatchlist(prev => prev.filter(item => item.id !== itemId));
      
      // Update context IDs to keep MovieCard indicators in sync
      if (tmdbId) {
        removeFromWatchlistIds(tmdbId);
      }
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to remove item');
    }
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleStatusFilterChange = (event: SelectChangeEvent<number | 'all'>) => {
    setStatusFilter(event.target.value as number | 'all');
  };

  if (!user) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="warning" sx={{ mb: 3 }}>
          Please log in to view your watchlist.
        </Alert>
        <Button 
          variant="contained" 
          color="primary"
          onClick={() => navigate(ROUTES.LOGIN)}
        >
          Go to Login
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
          My Watchlist
        </Typography>
        <Typography variant="body1" color="text.secondary" gutterBottom>
          Manage your movie collection
        </Typography>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Filters */}
      <Box sx={{ mb: 3, display: 'flex', gap: 2, alignItems: 'center' }}>
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel id="status-filter-label">Filter by Status</InputLabel>
          <Select
            labelId="status-filter-label"
            value={statusFilter}
            label="Filter by Status"
            onChange={handleStatusFilterChange}
          >
            <MenuItem value="all">All</MenuItem>
            <MenuItem value={WatchlistStatus.Planned}>Planned</MenuItem>
            <MenuItem value={WatchlistStatus.Watching}>Watching</MenuItem>
            <MenuItem value={WatchlistStatus.Watched}>Watched</MenuItem>
            <MenuItem value={WatchlistStatus.Dropped}>Dropped</MenuItem>
          </Select>
        </FormControl>

        <Typography variant="body2" color="text.secondary">
          {filteredItems.length} {filteredItems.length === 1 ? 'movie' : 'movies'}
        </Typography>
      </Box>

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label="watchlist tabs">
          <Tab label={`All (${watchlist.length})`} id="watchlist-tab-0" />
          <Tab 
            label={`Favorites (${watchlist.filter(i => i.isFavorite).length})`} 
            id="watchlist-tab-1" 
          />
          <Tab 
            label={`Watched (${watchlist.filter(i => i.status === WatchlistStatus.Watched).length})`} 
            id="watchlist-tab-2" 
          />
        </Tabs>
      </Box>

      {/* All Tab */}
      <TabPanel value={activeTab} index={0}>
        <WatchlistGrid
          items={filteredItems}
          loading={loading}
          onUpdate={handleUpdateItem}
          onDelete={handleDeleteItem}
        />
      </TabPanel>

      {/* Favorites Tab */}
      <TabPanel value={activeTab} index={1}>
        <WatchlistGrid
          items={filteredItems}
          loading={loading}
          onUpdate={handleUpdateItem}
          onDelete={handleDeleteItem}
        />
      </TabPanel>

      {/* Completed Tab */}
      <TabPanel value={activeTab} index={2}>
        <WatchlistGrid
          items={filteredItems}
          loading={loading}
          onUpdate={handleUpdateItem}
          onDelete={handleDeleteItem}
        />
      </TabPanel>
    </Container>
  );
};

export default WatchlistPage;

