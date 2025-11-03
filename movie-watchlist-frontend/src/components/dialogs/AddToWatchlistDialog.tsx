import React, { useEffect, useRef } from 'react';
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
import { trapFocus } from '../../utils/accessibility';

interface AddToWatchlistForm {
  status: WatchlistStatus;
  notes: string;
}

interface AddToWatchlistDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (form: AddToWatchlistForm) => void;
  form: AddToWatchlistForm;
  onChange: (form: AddToWatchlistForm) => void;
  movieTitle?: string;
  loading?: boolean;
}

const AddToWatchlistDialog: React.FC<AddToWatchlistDialogProps> = ({
  open,
  onClose,
  onConfirm,
  form,
  onChange,
  movieTitle,
  loading = false
}) => {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (open && dialogRef.current) {
      const cleanup = trapFocus(dialogRef.current);
      return cleanup;
    }
  }, [open]);

  const handleStatusChange = (status: WatchlistStatus) => {
    onChange({ ...form, status });
  };

  const handleNotesChange = (notes: string) => {
    onChange({ ...form, notes });
  };

  const handleConfirm = () => {
    onConfirm(form);
  };

  return (
    <Dialog 
      open={open} 
      onClose={onClose} 
      maxWidth="sm" 
      fullWidth
      aria-labelledby="add-to-watchlist-dialog-title"
      aria-describedby="add-to-watchlist-dialog-description"
    >
      <div ref={dialogRef}>
      <DialogTitle>
        Add to Watchlist
        {movieTitle && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {movieTitle}
          </Typography>
        )}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <FormControl fullWidth>
            <InputLabel id="status-label">Status</InputLabel>
            <Select
              labelId="status-label"
              value={form.status}
              label="Status"
              onChange={(e) => handleStatusChange(e.target.value as WatchlistStatus)}
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
            value={form.notes}
            onChange={(e) => handleNotesChange(e.target.value)}
            placeholder="Add your thoughts about this movie..."
            fullWidth
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button onClick={handleConfirm} variant="contained" color="primary" disabled={loading}>
          Add to Watchlist
        </Button>
      </DialogActions>
      </div>
    </Dialog>
  );
};

export default AddToWatchlistDialog;

