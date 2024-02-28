using System.Reflection;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using StarBot.Caching;

namespace StarBot.Scheduling;

struct HandlerArgs {
    public enum attributes {
        requiresIteration,
        onReddit,
        onXKCD

    }
    public string iteratorKey;
    public string channelKey;
    public string cacheKey;
    public DiscordSocketClient client;
    public Database data;
    public string resourceLocation;
    public ulong guildID;
    public MemoryCacheManager cache;
    public List<Func<JToken, bool>> filters;
    public List<attributes> attributeList;
    public string baseName;
    public HandlerArgs(string iteratorKey, string channelKey, string cacheKey, DiscordSocketClient client, Database data, string resourceLocation, ulong guildID, MemoryCacheManager cache, List<Func<JToken, bool>> filters, List<attributes> attributeList, string baseName) {
        this.iteratorKey = iteratorKey;
        this.channelKey = channelKey;
        this.cacheKey = cacheKey;
        this.client = client;
        this.data = data;
        this.resourceLocation = resourceLocation;
        this.guildID = guildID;
        this.cache = cache;
        this.filters = filters;
        this.attributeList = attributeList;
        this.baseName = baseName;
    }
}