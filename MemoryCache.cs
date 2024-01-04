using System.Collections.Concurrent;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using StarBot;

class MemoryCache {
    struct cachedObject<T> {
        DateTime validUntil;
        public readonly T objectToStore;
        public cachedObject(DateTime validUntil, T objectToStore) {
            this.validUntil = validUntil;
            this.objectToStore = objectToStore;
        }

        public readonly bool IsExpired() {
            return validUntil.CompareTo(DateTime.Now) < 0;
        }
    };

    Dictionary<string, cachedObject<dynamic>> cachedObjects = new();

    string preProcessValue(string input) {
        return input.Trim().ToLower();
    }

    public T? RequestFromCache<T>(string key) {
        cachedObject<dynamic> fetchedObject;
        if (!cachedObjects.TryGetValue(preProcessValue(key), out fetchedObject)) {
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
        cachedObject<dynamic> cacheEntry = new(validUntil, data);
        cachedObjects.Remove(key);
        cachedObjects.Add(key, cacheEntry);
    }


}