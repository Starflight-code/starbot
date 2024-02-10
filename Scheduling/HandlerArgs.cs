using System.Reflection;
using Discord.WebSocket;

namespace StarBot.Scheduling;

struct HandlerArgs {
    public string iteratorKey;
    public string channelKey;
    public DiscordSocketClient client;
    public Database data;
    public string resourceLocation;
    public HandlerArgs(string iteratorKey, string channelKey, DiscordSocketClient client, Database data, string resourceLocation) {
        this.iteratorKey = iteratorKey;
        this.channelKey = channelKey;
        this.client = client;
        this.data = data;
        this.resourceLocation = resourceLocation;
    }
}