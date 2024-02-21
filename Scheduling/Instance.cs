using Discord.WebSocket;

namespace StarBot.Scheduling;

internal class Instance {
    readonly Func<HandlerArgs, bool> resourceHandler;

    public Instance(Func<HandlerArgs, bool> resourceHandler, string? iteratorKey) {
        this.resourceHandler = resourceHandler;
    }
}