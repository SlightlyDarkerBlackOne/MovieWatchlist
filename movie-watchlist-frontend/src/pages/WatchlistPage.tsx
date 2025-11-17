import React, { useState } from 'react';
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
import { WatchlistGrid } from '../features/watchlist/components';
import { WatchlistFilters, WatchlistStats } from '../features/watchlist/components';
import { EditWatchlistItemDialog } from '../features/watchlist/components';
import { WatchlistItem, UpdateWatchlistRequest } from '../features/watchlist/model/watchlist.types';
import { useAuth } from '../features/auth/contexts/AuthContext';
import { 
  useGetWatchlistQuery, 
  useUpdateWatchlistItemMutation, 
  useRemoveFromWatchlistMutation,
} from '../features/watchlist/api/watchlistApi';
import { useWatchlistFilters } from '../features/watchlist/hooks/useWatchlistFilters';
import { ROUTES } from '../shared/constants/routeConstants';
import {
  ERROR_MESSAGES,
  WATCHLIST_PAGE_TEXT,
  WATCHLIST_TAB_LABELS,
  WATCHLIST_FILTER_VALUES,
  UI_CONSTANTS,
} from '../shared/constants/appConstants';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role={UI_CONSTANTS.ARIA_ROLES.TABPANEL}
      hidden={value !== index}
      id={`${UI_CONSTANTS.ID_PREFIXES.WATCHLIST_TABPANEL}${index}`}
      aria-labelledby={`${UI_CONSTANTS.ID_PREFIXES.WATCHLIST_TAB}${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const WatchlistPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  
  const { data: watchlist = [], isLoading: loading, error } = useGetWatchlistQuery(undefined, { skip: !user });
  const [updateItem] = useUpdateWatchlistItemMutation();
  const [removeItem] = useRemoveFromWatchlistMutation();
  
  const [activeTab, setActiveTab] = useState(0);
  const [statusFilter, setStatusFilter] = useState<number | 'all'>(WATCHLIST_FILTER_VALUES.ALL);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<WatchlistItem | null>(null);

  const { filteredItems, allCount, favoritesCount, watchedCount } = useWatchlistFilters({
    watchlist,
    activeTab,
    statusFilter,
  });

  const handleEditItem = (item: WatchlistItem) => {
    setSelectedItem(item);
    setEditDialogOpen(true);
  };

  const handleQuickUpdate = async (item: WatchlistItem) => {
    if (!user) return;

    try {
      const updatePayload: UpdateWatchlistRequest = {
        watchlistItemId: item.id,
        isFavorite: item.isFavorite,
      };
      
      await updateItem(updatePayload).unwrap();
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || ERROR_MESSAGES.FAILED_TO_UPDATE_ITEM);
    }
  };

  const handleUpdateItem = async (updatedFields: Partial<WatchlistItem>) => {
    if (!user || !selectedItem) return;

    try {
      const updatePayload: UpdateWatchlistRequest = {
        watchlistItemId: selectedItem.id,
      };
      
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
      
      await updateItem(updatePayload).unwrap();
      setEditDialogOpen(false);
      setSelectedItem(null);
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || ERROR_MESSAGES.FAILED_TO_UPDATE_ITEM);
    }
  };

  const handleDeleteItem = async (itemId: number) => {
    if (!user) return;

    try {
      await removeItem(itemId).unwrap();
    } catch (err) {
      const error = err as Error;
      setErrorMessage(error.message || ERROR_MESSAGES.FAILED_TO_REMOVE_ITEM);
    }
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleStatusFilterChange = (event: SelectChangeEvent<number | 'all'>) => {
    setStatusFilter(event.target.value as number | typeof WATCHLIST_FILTER_VALUES.ALL);
  };

  if (!user) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity={UI_CONSTANTS.ALERT_SEVERITY.WARNING} sx={{ mb: 3 }}>
          {WATCHLIST_PAGE_TEXT.LOGIN_REQUIRED}
        </Alert>
        <Button 
          variant="contained" 
          color="primary"
          onClick={() => navigate(ROUTES.LOGIN)}
        >
          {WATCHLIST_PAGE_TEXT.GO_TO_LOGIN}
        </Button>
      </Container>
    );
  }

  return (
    <>
      {/* Error Toast */}
      <Snackbar
        open={!!error || !!errorMessage}
        autoHideDuration={UI_CONSTANTS.SNACKBAR.AUTO_HIDE_DURATION}
        onClose={() => setErrorMessage(null)}
        anchorOrigin={{ 
          vertical: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.VERTICAL_TOP, 
          horizontal: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.HORIZONTAL_CENTER 
        }}
      >
        <Alert severity={UI_CONSTANTS.ALERT_SEVERITY.ERROR} variant="filled" sx={{ width: '100%' }} onClose={() => setErrorMessage(null)}>
          {errorMessage || (error ? String(error) : ERROR_MESSAGES.AN_ERROR_OCCURRED)}
        </Alert>
      </Snackbar>

      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
            {WATCHLIST_PAGE_TEXT.TITLE}
          </Typography>
          <Typography variant="body1" color="text.secondary" gutterBottom>
            {WATCHLIST_PAGE_TEXT.SUBTITLE}
          </Typography>
        </Box>

        <WatchlistStats />

        <WatchlistFilters
          statusFilter={statusFilter}
          onStatusFilterChange={handleStatusFilterChange}
          filteredCount={filteredItems.length}
        />

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label={UI_CONSTANTS.ARIA_LABELS.WATCHLIST_TABS}>
          <Tab label={`${WATCHLIST_TAB_LABELS.ALL} (${allCount})`} id={`${UI_CONSTANTS.ID_PREFIXES.WATCHLIST_TAB}0`} />
          <Tab 
            label={`${WATCHLIST_TAB_LABELS.FAVORITES} (${favoritesCount})`} 
            id={`${UI_CONSTANTS.ID_PREFIXES.WATCHLIST_TAB}1`} 
          />
          <Tab 
            label={`${WATCHLIST_TAB_LABELS.WATCHED} (${watchedCount})`} 
            id={`${UI_CONSTANTS.ID_PREFIXES.WATCHLIST_TAB}2`} 
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
