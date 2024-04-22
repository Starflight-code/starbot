using Debug;
using Discord;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using StarBot;

class WebManager {
    public struct databaseLookupValues {
        public string channelKey;
        public ulong guildID;
        public string iteratorKey;
        public databaseLookupValues(string channelKey, string iteratorKey, ulong guildID) {
            this.channelKey = channelKey;
            this.iteratorKey = iteratorKey;
            this.guildID = guildID;
        }

    }

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
        postIDString ??= ""; // if null, set to ""
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

    public static async void sendPost(Post post, databaseLookupValues dbVals, Database data, string baseName, DiscordSocketClient client) {
        if (!post.isValid()) {
            throw new ArgumentNullException("\"post\" is not valid (title, body and/or iterative null), required values not provided upon object construction.");
        }
        EmbedBuilder newEmbed = new() {
            Title = (bool)post.iterative ? $"{baseName} Image #{data.fetchValue(dbVals.iteratorKey, dbVals.guildID)}" : $"{baseName}: {post.title}",
            Description = post.body + "\n" +
                               post.linkToPost,
            ImageUrl = post.multimediaURL
        };
        //newEmbed.WithFooter("powered by https://reddit.com/r/cats/");*/
        ulong channelID = ulong.Parse(data.fetchValue(dbVals.channelKey, dbVals.guildID));
        SocketTextChannel? channel = client.GetChannel(channelID) as SocketTextChannel;
        if (!Config.DEBUG_MODE) {
            await channel.SendMessageAsync("", false, newEmbed.Build());
        }
    }

    public static JToken? SelectRandomRedditPost(string url, string lastIDCache, StarBot.Caching.MemoryCacheManager cacheManager, DebugComms debug, bool containsImage = true) {
        debug.SetSubPosition("Cache Fetch", 0);
        JObject json = cacheManager.FetchJSONFromCache(url);
        Random rand = new();
        int i = 0;
        int randomValue;
        debug.SetSubPosition("SelectFromJSON", 0);
        while (true) {
            i++;
            randomValue = rand.Next(100); // 0-99
            JToken? token = json["data"]["children"][randomValue]["data"];
            if (!containsImage || Validation.DuplicateAndImageCheck(token, lastIDCache, debug)) {
                break;
            }
            if (i >= 150) {
                json = FetchJSON(url);
            }
        }
        debug.SetSubPosition("Send \"Data\" Token Back", 0);
        /*if (!useCache) {
            cache.AddToCache(url, DateTime.Now.AddHours(Config.HOURS_TO_CACHE), json);*/
        return json["data"]["children"][randomValue]["data"];
    }

    public static JToken? SelectRandomRedditPost(string url, string lastIDCache, StarBot.Caching.MemoryCacheManager cacheManager, Func<JToken, bool> validation, bool containsImage = true) {
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
            if (validation(token)) {
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

    public static JToken? SelectRandomRedditPost(string url, string lastIDCache, StarBot.Caching.MemoryCacheManager cacheManager, List<Func<JToken, bool>> validation, bool containsImage = true) {
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
            bool valid = true;
            for (int j = 0; j < validation.Count(); j++) {
                if (!validation[j](token)) {
                    valid = false;
                    break;
                }
            }
            if (valid) {
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