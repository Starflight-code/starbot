namespace StarBot.Scheduling;

internal class Instance {
    public enum siteToHandle {
        XKCD,
        Reddit
    }
    readonly Func<HandlerArgs, Task<Post>>? resourceHandler;

    public Instance(siteToHandle site) {
        switch(site) {
            case siteToHandle.Reddit:
                    this.resourceHandler = Handlers.redditHandler;
                    break;
                case siteToHandle.XKCD:
                    this.resourceHandler = Handlers.xkcdHandler;
                    break;
                default:
                    return;
        }
    }
}