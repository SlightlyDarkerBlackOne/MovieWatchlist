import { z } from 'zod';
import { WatchlistStatus } from './watchlist.types';
import {
  VALIDATION_CONSTRAINTS,
  VALIDATION_MESSAGES,
} from '../../../shared/constants/appConstants';

export const watchlistItemSchema = z.object({
  status: z.nativeEnum(WatchlistStatus),
  notes: z.string().max(VALIDATION_CONSTRAINTS.NOTES.MAX_LENGTH, VALIDATION_MESSAGES.NOTES_LENGTH_MAX).optional(),
});

export type WatchlistItemSchemaType = z.infer<typeof watchlistItemSchema>;
export type WatchlistItemSchema = WatchlistItemSchemaType;


