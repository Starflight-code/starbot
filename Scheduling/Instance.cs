using Discord.WebSocket;

namespace StarBot.Scheduling;

internal class Instance {
    readonly Func<HandlerArgs, bool> resourceHandler;
    readonly string? iteratorKey;
    readonly string? channelKey;

    public Instance(Func<HandlerArgs, bool> resourceHandler, string? iteratorKey) {
        this.resourceHandler = resourceHandler;
        this.iteratorKey = iteratorKey;
    }
}