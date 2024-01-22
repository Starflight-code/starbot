using Discord;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using StarBot;

class WebManager {

    public static dynamic FetchJSON(string URL) {
        var site = new Url(URL);
        // headers and user agent spoofing are required to avoid a 403 'unauthorized' http error code
        System.Threading.Tasks.Task<string> output = site.WithHeaders(new { Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8", User_Agent = "Mozilla/5.0" }).GetStringAsync();
        output.Wait();
        string outString = output.Result;
        try {
            return JObject.Parse(outString);

        } catch (Newtonsoft.Json.JsonReaderException) {
            return JArray.Parse(outString);
        }
    }

    public static string AddNewPostID(string postIDString, string newPostID) {
        List<string> postIDs;
        if (postIDString != "") {
            postIDs = postIDString.Split(",").ToList();
        } else {
            postIDs = new List<string>();
        }
        while (postIDs.Count >= Config.POSTS_TO_CACHE) { // reduces list count to POSTS_TO_CACHE - 1, oldest entries first
            postIDs.RemoveAt(0);
        }
        postIDs.Add(newPostID);
        return string.Join(",", postIDs);
    }

    public static JToken? SelectRandomRedditPost(string url, StarBot.Caching.MemoryCacheManager cacheManager, bool containsImage = true) {
        JObject json = cacheManager.FetchJSONFromCache(url);

        Random rand = new();
        int i = 0;
        int randomValue;
        while (true) {
            i++;
            randomValue = rand.Next(100); // 0-99
            try {
                if (!containsImage || Validation.IsLinkToImage(json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString())) {
                    break;
                }
            } catch (NullReferenceException) {
                // ignores null refrences
            }
            if (i >= 150) {
                json = FetchJSON(url);
            }
        }
        return json["data"]["children"][randomValue]["data"];
    }

    public static JToken? SelectRandomRedditPost(string url, string lastIDCache, StarBot.Caching.MemoryCacheManager cacheManager, bool containsImage = true) {
        /*bool useCache = true;
        if (cache == null || default(JObject) == cache.RequestFromCache<JObject>(url)) {
            useCache = false;
        }
        JObject? json;
        switch (useCache) {
            case true:
                json = cache.RequestFromCache<JObject>(url);
                break;
            case false:
                json = FetchJSON(url);
                break;
        }*/
        JObject json = cacheManager.FetchJSONFromCache(url);

        Random rand = new();
        int i = 0;
        int randomValue;
        while (true) {
            i++;
            randomValue = rand.Next(100); // 0-99
            JToken? token = json["data"]["children"][randomValue]["data"];
            if (!containsImage || Validation.DuplicateAndImageCheck(token, lastIDCache)) {
                break;
            }
            if (i >= 150) {
                json = FetchJSON(url);
            }
        }
        /*if (!useCache) {
            cache.AddToCache(url, DateTime.Now.AddHours(Config.HOURS_TO_CACHE), json);*/
        return json["data"]["children"][randomValue]["data"];
    }
};