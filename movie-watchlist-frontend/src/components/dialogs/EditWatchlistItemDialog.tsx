import React, { useState, useEffect, useRef } from 'react';
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
  Box,
  Chip,
  FormControlLabel,
  Switch
} from '@mui/material';
import StarBorderIcon from '@mui/icons-material/StarBorder';
import StarIcon from '@mui/icons-material/Star';
import FavoriteIcon from '@mui/icons-material/Favorite';
import { WatchlistStatus } from '../../types/watchlist.types';
import { WatchlistItem } from '../../types/watchlist.types';
import { trapFocus } from '../../utils/accessibility';

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
  const dialogRef = useRef<HTMLDivElement>(null);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [isFavorite, setIsFavorite] = useState(false);
  const [userRating, setUserRating] = useState<number | undefined>(undefined);
  const [hoveredRating, setHoveredRating] = useState<number | null>(null);

  useEffect(() => {
    if (open && dialogRef.current) {
      const cleanup = trapFocus(dialogRef.current);
      return cleanup;
    }
  }, [open]);

  useEffect(() => {
    if (item && open) {
      setStatus(item.status);
      setNotes(item.notes || '');
      setIsFavorite(item.isFavorite || false);
      
      if (item.userRating) {
        let ratingValue: number;
        if (typeof item.userRating === 'object' && item.userRating !== null && 'value' in item.userRating) {
          ratingValue = (item.userRating as { value: number }).value;
        } else if (typeof item.userRating === 'number') {
          ratingValue = item.userRating;
        } else {
          ratingValue = item.userRating as any;
        }
        setUserRating(ratingValue);
      } else {
        setUserRating(undefined);
      }
      
      setHoveredRating(null);
    }
  }, [item, open]);

  const handleSave = async () => {
    if (!item) return;
    
    const updatedItem: any = {
      status,
      notes: notes || undefined,
      isFavorite,
    };
    
    if (userRating !== undefined && userRating !== null) {
      updatedItem.userRating = userRating;
    }

    await onSave(updatedItem);
    onClose();
  };

  if (!item) return null;

  return (
    <Dialog 
      open={open} 
      onClose={onClose} 
      maxWidth="sm" 
      fullWidth
      aria-labelledby="edit-watchlist-item-dialog-title"
      aria-describedby="edit-watchlist-item-dialog-description"
    >
      <div ref={dialogRef}>
      <DialogTitle id="edit-watchlist-item-dialog-title">
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Typography variant="h6">Edit Watchlist Item</Typography>
          {item.movie && (
            <Typography variant="body2" color="text.secondary">
              {item.movie.title}
            </Typography>
          )}
        </Box>
      </DialogTitle>
      <DialogContent id="edit-watchlist-item-dialog-description">
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, pt: 2 }}>
          
          <Box>
            <Typography variant="subtitle1" sx={{ mb: 1.5, fontWeight: 600 }}>
              Rating
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box 
                sx={{ 
                  display: 'flex', 
                  gap: 0.5,
                  cursor: 'pointer'
                }}
                onMouseLeave={() => setHoveredRating(null)}
              >
                {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((star) => {
                  const displayRating = hoveredRating ?? userRating;
                  const isFilled = star <= (displayRating ?? 0);
                  return (
                    <Box
                      key={star}
                      onClick={() => setUserRating(star)}
                      onMouseEnter={() => setHoveredRating(star)}
                      sx={{ 
                        transition: 'transform 0.1s',
                        '&:hover': {
                          transform: 'scale(1.2)'
                        }
                      }}
                    >
                      {isFilled ? (
                        <StarIcon sx={{ color: '#FFD700', fontSize: '2rem' }} />
                      ) : (
                        <StarBorderIcon sx={{ color: '#ccc', fontSize: '2rem' }} />
                      )}
                    </Box>
                  );
                })}
              </Box>
              <Typography variant="h6" sx={{ minWidth: 120 }}>
                {hoveredRating 
                  ? `${hoveredRating}/10` 
                  : userRating 
                    ? `${userRating}/10` 
                    : 'Rate Now'}
              </Typography>
            </Box>
          </Box>

          <Box>
            <Typography variant="subtitle1" sx={{ mb: 1.5, fontWeight: 600 }}>
              Status
            </Typography>
            <FormControl fullWidth>
              <InputLabel id="status-label">Status</InputLabel>
              <Select
                labelId="status-label"
                value={status}
                label="Status"
                onChange={(e) => setStatus(e.target.value as WatchlistStatus)}
              >
                <MenuItem value={WatchlistStatus.Planned}>
                  <Chip label="Planned" color="info" size="small" />
                </MenuItem>
                <MenuItem value={WatchlistStatus.Watching}>
                  <Chip label="Watching" color="primary" size="small" />
                </MenuItem>
                <MenuItem value={WatchlistStatus.Watched}>
                  <Chip label="Watched" color="success" size="small" />
                </MenuItem>
                <MenuItem value={WatchlistStatus.Dropped}>
                  <Chip label="Dropped" color="error" size="small" />
                </MenuItem>
              </Select>
            </FormControl>
          </Box>

          <Box>
            <Typography variant="subtitle1" sx={{ mb: 1.5, fontWeight: 600 }}>
              Notes
            </Typography>
            <TextField
              placeholder="Add your thoughts about this movie..."
              multiline
              rows={4}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              fullWidth
              variant="outlined"
            />
          </Box>

          <Box>
            <FormControlLabel
              control={
                <Switch
                  checked={isFavorite}
                  onChange={(e) => setIsFavorite(e.target.checked)}
                  color="error"
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FavoriteIcon color={isFavorite ? "error" : "action"} />
                  <Typography>Favorite</Typography>
                </Box>
              }
            />
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button onClick={handleSave} variant="contained" color="primary" disabled={loading}>
          Save Changes
        </Button>
      </DialogActions>
      </div>
    </Dialog>
  );
};

export default EditWatchlistItemDialog;

