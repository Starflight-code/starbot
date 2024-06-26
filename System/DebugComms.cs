using System.Runtime.InteropServices;
using Discord;
using Discord.WebSocket;
using StarBot;

namespace Debug;

class DebugComms {
    bool verbose = false;
    private string position = "";
    public void UpdatePosition(string newPosition) {
        position = newPosition;
        if (verbose) {
            Console.WriteLine($"{DateTime.Now.ToString()}: \nReached Position: {position}");
        }
    }
    public void setVerbosity(bool verbose) {
        this.verbose = verbose;
    }
    public async void PrintState(DiscordSocketClient client, Exception e) {
        if (client.ConnectionState == ConnectionState.Connected) {
            var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;
            await channel.SendMessageAsync($"{DateTime.Now.ToString()}: {e.Message}\n```{e.StackTrace}```\nPosition: {position}");
        } else {
            Console.WriteLine($"{DateTime.Now.ToString()}: {e.Message}\n```{e.StackTrace}```\nPosition: {position}");
        }
    }
}