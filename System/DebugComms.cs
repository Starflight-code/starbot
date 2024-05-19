using System.Runtime.InteropServices;
using Discord;
using Discord.WebSocket;
using StarBot;

namespace Debug;

class DebugComms {
    bool verbose = false;
    private string position = "";
    string[] subPositions = new string[10];
    public void UpdatePosition(string newPosition) {
        position = newPosition;
        if (verbose) {
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Reached Position: {position}");
        }
        for (int i = 0; i < subPositions.Length; i++) {
            subPositions[i] = "";
        }
    }
    public void SetSubPosition(string subPosition, uint index) {
        if (index >= subPosition.Length) {
            throw new ArgumentException("Sub-Position Index Out Of Range");
        }
        if (verbose) {
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Reached Sub-Position: {subPosition}");
        }
    }
    public void setVerbosity(bool verbose) {
        this.verbose = verbose;
    }
    public async void LogState(DiscordSocketClient client, Exception e) {
        var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;

        string subPositionConcat = string.Join("", subPositions);
        string message = $"{DateTime.Now}: {e.Message}\n```{e.StackTrace}```\nPosition: {position}\nSubPositions: {subPositionConcat}";

        await channel.SendMessageAsync(message);
        Console.WriteLine(message);
    }
}