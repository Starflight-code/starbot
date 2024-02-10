namespace StarBot.Scheduling;

struct HandlerArgs {
    string resourceLocation;
    public HandlerArgs(string resourceLocation) {
        this.resourceLocation = resourceLocation;
    }
}