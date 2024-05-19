using Newtonsoft.Json.Linq;

namespace StarBot.Caching;
public class MemoryCacheManager
{
    private MemoryCache? cache;

    public MemoryCacheManager()
    {
        if (Config.MEMORY_CACHE)
        {
            cache = new();
        }
        else
        {
            cache = null;
        }
    }
    public MemoryCacheManager(MemoryCache? cache)
    {
        if (Config.MEMORY_CACHE)
        {
            this.cache = cache;
        }
        else
        {
            cache = null;
        }
    }
    public JObject FetchJSONFromCache(string url)
    {
        switch (Config.MEMORY_CACHE)
        {
            case false: // cache disabled
                return WebManager.FetchJSON(url);
                break;
            case true: // cache enabled
                JObject? result = cache.RequestFromCache<JObject>(url);
                if (result == default)
                {
                    JObject json = WebManager.FetchJSON(url);
                    cache.AddToCache<JObject>(url, DateTime.Now.AddHours(Config.HOURS_TO_CACHE), json);
                    return json;
                }
                else
                {
                    return result;
                }
                break;
        }
    }
}