using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;

namespace StarBot.DiscordInterop;

internal class MessageCommands {
    public static async Task UserReport(SocketMessageCommand command, DiscordSocketClient? client, Database data) {
        var reportedMessage = command.Data.Message;
        var content = reportedMessage.CleanContent.Trim() == "" ? "<No Content>" : reportedMessage.CleanContent.Trim();
        if (command.GuildId == null) { return; }

        var reportEmbed = new EmbedBuilder {
            Title = "Report by " + command.User.Username,
            Color = Color.DarkRed,
            Description = content + $"\n{reportedMessage.GetJumpUrl()} | ID: {reportedMessage.Id}",
            ThumbnailUrl = reportedMessage.Author.GetAvatarUrl()
        };

        await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync("", embed: reportEmbed.Build());
        await command.RespondAsync("We've recieved your report, and it will be reviewed by our moderation team.", ephemeral: true);
    }
}