using System.Collections.Concurrent;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using StarBot;

namespace StarBot.Caching;
public class MemoryCache {
    struct CachedObject<T> {
        DateTime validUntil;
        public readonly T objectToStore;
        public CachedObject(DateTime validUntil, T objectToStore) {
            this.validUntil = validUntil;
            this.objectToStore = objectToStore;
        }

        public readonly bool IsExpired() {
            return validUntil.CompareTo(DateTime.Now) < 0;
        }
    };

    Dictionary<string, CachedObject<dynamic>> cachedObjects = new();

    static string PreProcessValue(string input) {
        return input.Trim().ToLower();
    }

    public T? RequestFromCache<T>(string key) {
        if (!cachedObjects.TryGetValue(PreProcessValue(key), out CachedObject<dynamic> fetchedObject)) {
            return default;
        }

        if (fetchedObject.IsExpired()) {
            return default;
        }

        return fetchedObject.objectToStore;
    }

    public void AddToCache<T>(string key, DateTime validUntil, T data) {
        if (data == null) { return; }
        if (!Config.MEMORY_CACHE) { return; }
        CachedObject<dynamic> cacheEntry = new(validUntil, data);
        cachedObjects.Remove(key);
        cachedObjects.Add(key, cacheEntry);
    }
}