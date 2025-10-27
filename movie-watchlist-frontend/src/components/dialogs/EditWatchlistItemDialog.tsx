import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Typography,
  Box
} from '@mui/material';
import { WatchlistStatus } from '../../types/watchlist.types';
import { WatchlistItem } from '../../types/watchlist.types';

interface EditWatchlistItemDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (updatedItem: Partial<WatchlistItem>) => Promise<void>;
  item: WatchlistItem | null;
  loading?: boolean;
}

const EditWatchlistItemDialog: React.FC<EditWatchlistItemDialogProps> = ({
  open,
  onClose,
  onSave,
  item,
  loading = false
}) => {
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [isFavorite, setIsFavorite] = useState(false);
  const [userRating, setUserRating] = useState<number | undefined>(undefined);

  useEffect(() => {
    if (item) {
      setStatus(item.status);
      setNotes(item.notes || '');
      setIsFavorite(item.isFavorite || false);
      setUserRating(item.userRating);
    }
  }, [item]);

  const handleSave = async () => {
    if (!item) return;
    
    const updatedItem = {
      status,
      notes,
      isFavorite,
      userRating
    };

    await onSave(updatedItem);
    onClose();
  };

  if (!item) return null;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        Edit Watchlist Item
        {item.movie && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {item.movie.title}
          </Typography>
        )}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <FormControl fullWidth>
            <InputLabel id="status-label">Status</InputLabel>
            <Select
              labelId="status-label"
              value={status}
              label="Status"
              onChange={(e) => setStatus(e.target.value as WatchlistStatus)}
            >
              <MenuItem value={WatchlistStatus.Planned}>Planned</MenuItem>
              <MenuItem value={WatchlistStatus.Watching}>Watching</MenuItem>
              <MenuItem value={WatchlistStatus.Watched}>Watched</MenuItem>
              <MenuItem value={WatchlistStatus.Dropped}>Dropped</MenuItem>
            </Select>
          </FormControl>

          <TextField
            label="Notes (optional)"
            multiline
            rows={3}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Add your thoughts about this movie..."
            fullWidth
          />

          <TextField
            label="User Rating (optional)"
            type="number"
            inputProps={{ min: 1, max: 10, step: 0.5 }}
            value={userRating || ''}
            onChange={(e) => setUserRating(e.target.value ? parseFloat(e.target.value) : undefined)}
            fullWidth
            helperText="Rate the movie from 1 to 10"
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button onClick={handleSave} variant="contained" color="primary" disabled={loading}>
          Save Changes
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default EditWatchlistItemDialog;

