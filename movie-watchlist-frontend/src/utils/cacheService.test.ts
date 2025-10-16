/**
 * Tests for cache service
 */

import cacheService from './cacheService';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};

  const mock = {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value.toString();
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
    get length() {
      return Object.keys(store).length;
    },
    key: (index: number) => {
      const keys = Object.keys(store);
      return keys[index] || null;
    },
  };

  // Make Object.keys() work on the mock by copying store keys
  return new Proxy(mock, {
    ownKeys: () => Object.keys(store),
    getOwnPropertyDescriptor: (target, prop) => {
      if (prop in store) {
        return {
          enumerable: true,
          configurable: true,
        };
      }
      return Object.getOwnPropertyDescriptor(target, prop);
    },
  });
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

describe('CacheService', () => {
  beforeEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  describe('set and get', () => {
    it('should store and retrieve data', () => {
      const testData = { name: 'Test Movie', rating: 8.5 };
      cacheService.set('test-key', testData, 1);
      
      const retrieved = cacheService.get<typeof testData>('test-key');
      expect(retrieved).toEqual(testData);
    });

    it('should return null for non-existent key', () => {
      const result = cacheService.get('non-existent-key');
      expect(result).toBeNull();
    });

    it('should handle primitive values', () => {
      cacheService.set('string-key', 'test string', 1);
      cacheService.set('number-key', 42, 1);
      cacheService.set('boolean-key', true, 1);

      expect(cacheService.get('string-key')).toBe('test string');
      expect(cacheService.get('number-key')).toBe(42);
      expect(cacheService.get('boolean-key')).toBe(true);
    });

    it('should handle arrays', () => {
      const testArray = [1, 2, 3, 4, 5];
      cacheService.set('array-key', testArray, 1);
      
      expect(cacheService.get('array-key')).toEqual(testArray);
    });
  });

  describe('TTL (Time To Live)', () => {
    it('should expire data after TTL', () => {
      const testData = { value: 'test' };
      const originalNow = Date.now;
      const mockNow = jest.fn();
      Date.now = mockNow;

      // Set data at time 0
      mockNow.mockReturnValue(0);
      cacheService.set('ttl-test', testData, 1); // 1 hour TTL

      // Check immediately - should exist
      mockNow.mockReturnValue(0);
      expect(cacheService.get('ttl-test')).toEqual(testData);

      // Check after 30 minutes - should still exist
      mockNow.mockReturnValue(30 * 60 * 1000);
      expect(cacheService.get('ttl-test')).toEqual(testData);

      // Check after 61 minutes - should be expired
      mockNow.mockReturnValue(61 * 60 * 1000);
      expect(cacheService.get('ttl-test')).toBeNull();

      Date.now = originalNow;
    });

    it('should not expire data before TTL', () => {
      const testData = { value: 'test' };
      cacheService.set('no-expire', testData, 10); // 10 hours
      
      const retrieved = cacheService.get('no-expire');
      expect(retrieved).toEqual(testData);
    });
  });

  describe('clear', () => {
    it('should clear specific key', () => {
      cacheService.set('key1', 'value1', 1);
      cacheService.set('key2', 'value2', 1);

      cacheService.clear('key1');

      expect(cacheService.get('key1')).toBeNull();
      expect(cacheService.get('key2')).toBe('value2');
    });

    it('should handle clearing non-existent key', () => {
      expect(() => cacheService.clear('non-existent')).not.toThrow();
    });
  });

  describe('getStats', () => {
    it('should return cache statistics', () => {
      cacheService.set('key1', 'value1', 1);
      cacheService.set('key2', { data: 'value2' }, 1);

      const stats = cacheService.getStats();

      expect(stats.totalEntries).toBe(2);
      expect(stats.totalSize).toBeGreaterThan(0);
      expect(stats.version).toBeDefined();
    });

    it('should return zero entries for empty cache', () => {
      const stats = cacheService.getStats();

      expect(stats.totalEntries).toBe(0);
      expect(stats.totalSize).toBe(0);
    });
  });

  describe('clearExpired', () => {
    it('should remove only expired entries', () => {
      const originalNow = Date.now;
      const mockNow = jest.fn();
      Date.now = mockNow;

      mockNow.mockReturnValue(0);
      cacheService.set('fresh', 'value1', 10); // 10 hours
      cacheService.set('expired', 'value2', 1); // 1 hour

      // Move time forward by 2 hours
      mockNow.mockReturnValue(2 * 60 * 60 * 1000);
      cacheService.clearExpired();

      expect(cacheService.get('fresh')).toBe('value1');
      expect(cacheService.get('expired')).toBeNull();

      Date.now = originalNow;
    });
  });

  describe('version handling', () => {
    it('should include version in cache entries', () => {
      cacheService.set('versioned', 'test', 1);
      
      const stats = cacheService.getStats();
      expect(stats.version).toBe('1.0');
    });
  });
});


