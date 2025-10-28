import React, { useState, useMemo } from 'react';
import {
  Container,
  Box,
  Typography,
  Alert,
  Tabs,
  Tab,
  Button,
  Snackbar,
  SelectChangeEvent
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { WatchlistGrid } from '../components/watchlist';
import { WatchlistFilters, WatchlistStats } from '../components/pages';
import { EditWatchlistItemDialog } from '../components/dialogs';
import { WatchlistItem, WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from '../contexts/AuthContext';
import { useWatchlist } from '../contexts/WatchlistContext';
import { 
  useWatchlistQuery, 
  useUpdateWatchlistItemOperation, 
  useRemoveFromWatchlistOperation,
  useWatchlistStatistics
} from '../hooks/useWatchlistOperations';
import { watchlistApi } from '../store/api/watchlistApi';
import { useDispatch } from 'react-redux';
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
  const { watchlistMovieIds } = useWatchlist();
  const navigate = useNavigate();
  
  const dispatch = useDispatch();
  const { data: watchlist = [], isLoading: loading, error } = useWatchlistQuery(user?.id);
  const { data: stats } = useWatchlistStatistics(user?.id);
  const { updateItem } = useUpdateWatchlistItemOperation();
  const { removeItem } = useRemoveFromWatchlistOperation();
  
  const [activeTab, setActiveTab] = useState(0);
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<WatchlistItem | null>(null);

  const filteredItems = useMemo(() => {
    let filtered = [...watchlist];

    if (activeTab === 1) {
      filtered = filtered.filter(item => item.isFavorite);
    } else if (activeTab === 2) {
      filtered = filtered.filter(item => item.status === WatchlistStatus.Watched);
    }

    if (statusFilter !== 'all') {
      filtered = filtered.filter(item => item.status === statusFilter);
    }

    return filtered;
  }, [watchlist, activeTab, statusFilter]);

  const handleEditItem = (item: WatchlistItem) => {
    setSelectedItem(item);
    setEditDialogOpen(true);
  };

  const handleQuickUpdate = async (item: WatchlistItem) => {
    if (!user?.id) return;

    try {
      const updatePayload: any = {};
      
      if (item.isFavorite !== undefined) {
        updatePayload.isFavorite = item.isFavorite;
      }
      
      await updateItem(user.id, item.id, updatePayload);
      dispatch(watchlistApi.util.invalidateTags(['Watchlist', 'WatchlistStats']));
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || 'Failed to update item');
    }
  };

  const handleUpdateItem = async (updatedFields: Partial<WatchlistItem>) => {
    if (!user?.id || !selectedItem) return;

    try {
      const updatePayload: any = {};
      
      if (updatedFields.isFavorite !== undefined) {
        updatePayload.isFavorite = updatedFields.isFavorite;
      }
      if (updatedFields.status !== undefined) {
        updatePayload.status = updatedFields.status;
      }
      if (updatedFields.userRating !== undefined && updatedFields.userRating !== null) {
        updatePayload.userRating = updatedFields.userRating;
      }
      if (updatedFields.notes !== undefined && updatedFields.notes !== null && updatedFields.notes !== '') {
        updatePayload.notes = updatedFields.notes;
      }
      
      await updateItem(user.id, selectedItem.id, updatePayload);
      setEditDialogOpen(false);
      setSelectedItem(null);
      dispatch(watchlistApi.util.invalidateTags(['Watchlist', 'WatchlistStats']));
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || 'Failed to update item');
    }
  };

  const handleDeleteItem = async (itemId: number) => {
    if (!user?.id) return;

    try {
      await removeItem(user.id, itemId);
      dispatch(watchlistApi.util.invalidateTags(['Watchlist', 'WatchlistStats']));
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || 'Failed to remove item');
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
    <>
      {/* Error Toast */}
      <Snackbar
        open={!!error || !!errorMessage}
        autoHideDuration={5000}
        onClose={() => setErrorMessage(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }} onClose={() => setErrorMessage(null)}>
          {errorMessage || (error ? String(error) : 'An error occurred')}
        </Alert>
      </Snackbar>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
            My Watchlist
          </Typography>
          <Typography variant="body1" color="text.secondary" gutterBottom>
            Manage your movie collection
          </Typography>
        </Box>

        <WatchlistStats userId={user?.id} />

        <WatchlistFilters
          statusFilter={statusFilter}
          onStatusFilterChange={handleStatusFilterChange}
          itemCount={filteredItems.length}
        />

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
          onUpdate={handleQuickUpdate}
          onDelete={handleDeleteItem}
          onEdit={handleEditItem}
        />
      </TabPanel>

      {/* Favorites Tab */}
      <TabPanel value={activeTab} index={1}>
        <WatchlistGrid
          items={filteredItems}
          loading={loading}
          onUpdate={handleQuickUpdate}
          onDelete={handleDeleteItem}
          onEdit={handleEditItem}
        />
      </TabPanel>

      {/* Completed Tab */}
      <TabPanel value={activeTab} index={2}>
        <WatchlistGrid
          items={filteredItems}
          loading={loading}
          onUpdate={handleQuickUpdate}
          onDelete={handleDeleteItem}
          onEdit={handleEditItem}
        />
      </TabPanel>

      {/* Edit Dialog */}
      <EditWatchlistItemDialog
        open={editDialogOpen}
        onClose={() => {
          setEditDialogOpen(false);
          setSelectedItem(null);
        }}
        onSave={handleUpdateItem}
        item={selectedItem}
      />

    </Container>
    </>
  );
};

export default WatchlistPage;
