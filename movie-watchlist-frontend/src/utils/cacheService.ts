/**
 * Generic localStorage-based caching service with TTL support
 */

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresAt: number;
  version: string;
}

const CACHE_VERSION = '1.0';
const CACHE_KEY_PREFIX = 'movie_watchlist_cache_';

class CacheService {
  /**
   * Check if localStorage is available
   */
  private isLocalStorageAvailable(): boolean {
    try {
      const test = '__localStorage_test__';
      localStorage.setItem(test, test);
      localStorage.removeItem(test);
      return true;
    } catch (e) {
      return false;
    }
  }

  /**
   * Generate full cache key with prefix
   */
  private getFullKey(key: string): string {
    return `${CACHE_KEY_PREFIX}${key}`;
  }

  /**
   * Store data in cache with TTL
   * @param key - Cache key
   * @param data - Data to cache
   * @param ttlHours - Time to live in hours
   */
  set<T>(key: string, data: T, ttlHours: number = 6): boolean {
    if (!this.isLocalStorageAvailable()) {
      return false;
    }

    try {
      const now = Date.now();
      const entry: CacheEntry<T> = {
        data,
        timestamp: now,
        expiresAt: now + ttlHours * 60 * 60 * 1000,
        version: CACHE_VERSION,
      };

      const serialized = JSON.stringify(entry);
      const fullKey = this.getFullKey(key);
      
      try {
        localStorage.setItem(fullKey, serialized);
        return true;
      } catch (e) {
        const error = e as DOMException;
        // Handle quota exceeded error
        if (error.name === 'QuotaExceededError' || error.name === 'NS_ERROR_DOM_QUOTA_REACHED') {
          this.clearExpired();
          
          // Try one more time after cleanup
          try {
            localStorage.setItem(fullKey, serialized);
            return true;
          } catch (retryError) {
            return false;
          }
        }
        throw e;
      }
    } catch (error) {
      return false;
    }
  }

  /**
   * Retrieve data from cache if not expired
   * @param key - Cache key
   * @returns Cached data or null if expired/missing
   */
  get<T>(key: string): T | null {
    if (!this.isLocalStorageAvailable()) {
      return null;
    }

    try {
      const fullKey = this.getFullKey(key);
      const serialized = localStorage.getItem(fullKey);

      if (!serialized) {
        return null;
      }

      const entry: CacheEntry<T> = JSON.parse(serialized);

      if (entry.version !== CACHE_VERSION) {
        this.clear(key);
        return null;
      }

      const now = Date.now();
      if (now > entry.expiresAt) {
        this.clear(key);
        return null;
      }

      return entry.data;
    } catch (error) {
      this.clear(key);
      return null;
    }
  }

  /**
   * Clear specific cache entry
   * @param key - Cache key to clear
   */
  clear(key: string): void {
    if (!this.isLocalStorageAvailable()) {
      return;
    }

    try {
      const fullKey = this.getFullKey(key);
      localStorage.removeItem(fullKey);
    } catch (error) {
    }
  }

  /**
   * Clear all cache entries with our prefix
   */
  clearAll(): void {
    if (!this.isLocalStorageAvailable()) {
      return;
    }

    try {
      const keys = Object.keys(localStorage);
      keys.forEach((key) => {
        if (key.startsWith(CACHE_KEY_PREFIX)) {
          localStorage.removeItem(key);
        }
      });
    } catch (error) {
    }
  }

  /**
   * Clear only expired cache entries
   */
  clearExpired(): void {
    if (!this.isLocalStorageAvailable()) {
      return;
    }

    try {
      const now = Date.now();
      const keys = Object.keys(localStorage);
      let clearedCount = 0;

      keys.forEach((fullKey) => {
        if (fullKey.startsWith(CACHE_KEY_PREFIX)) {
          try {
            const serialized = localStorage.getItem(fullKey);
            if (serialized) {
              const entry = JSON.parse(serialized);
              if (now > entry.expiresAt || entry.version !== CACHE_VERSION) {
                localStorage.removeItem(fullKey);
                clearedCount++;
              }
            }
          } catch (e) {
            // Corrupted entry, remove it
            localStorage.removeItem(fullKey);
            clearedCount++;
          }
        }
      });

      if (clearedCount > 0) {
      }
    } catch (error) {
    }
  }

  /**
   * Get cache statistics
   */
  getStats(): { totalEntries: number; totalSize: number; version?: string } {
    if (!this.isLocalStorageAvailable()) {
      return { totalEntries: 0, totalSize: 0, version: CACHE_VERSION };
    }

    try {
      const keys = Object.keys(localStorage);
      let totalSize = 0;
      let totalEntries = 0;

      keys.forEach((key) => {
        if (key.startsWith(CACHE_KEY_PREFIX)) {
          const value = localStorage.getItem(key);
          if (value) {
            totalSize += value.length;
            totalEntries++;
          }
        }
      });

      return {
        totalEntries,
        totalSize, // Size in characters (roughly bytes for ASCII)
        version: CACHE_VERSION,
      };
    } catch (error) {
      return { totalEntries: 0, totalSize: 0, version: CACHE_VERSION };
    }
  }
}

const cacheService = new CacheService();
export default cacheService;






