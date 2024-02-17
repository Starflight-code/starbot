using System.Reflection;
using Discord.WebSocket;
using StarBot.Caching;

namespace StarBot.Scheduling;

struct HandlerArgs {
    public string iteratorKey;
    public string channelKey;
    public string cacheKey;
    public DiscordSocketClient client;
    public Database data;
    public string resourceLocation;
    public ulong guildID;
    public MemoryCacheManager cache;
    public HandlerArgs(string iteratorKey, string channelKey, string cacheKey, DiscordSocketClient client, Database data, string resourceLocation, ulong guildID, MemoryCacheManager cache) {
        this.iteratorKey = iteratorKey;
        this.channelKey = channelKey;
        this.cacheKey = cacheKey;
        this.client = client;
        this.data = data;
        this.resourceLocation = resourceLocation;
        this.guildID = guildID;
        this.cache = cache;
    }
}