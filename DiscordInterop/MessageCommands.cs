using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;

namespace StarBot.DiscordInterop;

internal class MessageCommands {
    public static async Task UserReport(SocketMessageCommand command, DiscordSocketClient? client, Database data) {
        if (command.IsDMInteraction || command.GuildId == null) {
            await command.RespondAsync("This command can not be executed in the current enviroment. (In DM Channel or GuildID is null)", ephemeral: true);
            return;
        }

        var reportedMessage = command.Data.Message;
        var content = reportedMessage.CleanContent.Trim() == "" ? "<No Content>" : reportedMessage.CleanContent.Trim();

        List<Attachment> attached = reportedMessage.Attachments.ToList();
        List<Embed> embeds = reportedMessage.Embeds.ToList();

        string attachmentSummary;

        if (attached.Count == 0 && embeds.Count == 0) {
            attachmentSummary = "None";
        } else {
            attachmentSummary = "";
            for (int i = 0; i < attached.Count; i++) {
                if (i != 0) { attachmentSummary += "\n"; }
                attachmentSummary += " - " + attached[i].Filename;
            }
            if (0 < attached.Count && 0 < embeds.Count) {
                attachmentSummary += "\n - " + embeds.Count + " embed(s)";
            } else if (attached.Count == 0 && 0 < embeds.Count) {
                attachmentSummary += " - " + embeds.Count + " embed(s)";
            }
        }

        var reported = new EmbedFieldBuilder {
            Name = "Reported",
            Value = reportedMessage.Author.Mention + " - " + reportedMessage.Author.Username,
            IsInline = true
        };

        var reporter = new EmbedFieldBuilder {
            Name = "Reporter",
            Value = command.User.Mention + " - " + command.User.Username,
            IsInline = true
        };

        var attachments = new EmbedFieldBuilder {
            Name = "Attachments",
            Value = attachmentSummary,
            IsInline = false
        };

        var reportEmbed = new EmbedBuilder {
            Title = "Message Report",
            Color = Color.DarkRed,
            Description = content + $"\n{reportedMessage.GetJumpUrl()} | ID: {reportedMessage.Id}",
            ThumbnailUrl = reportedMessage.Author.GetAvatarUrl(),
            Fields = new List<EmbedFieldBuilder> { reported, reporter, attachments }
        };

        var message = await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync("", embed: reportEmbed.Build());
        var addToEmbed = new EmbedFieldBuilder {
            Name = "Attached to Report",
            Value = message.GetJumpUrl(),
            IsInline = false
        };
        for (int i = 0; i < embeds.Count; i++) {
            embeds[i] = embeds[i].ToEmbedBuilder().AddField(addToEmbed).Build();
        }
        if (attached.Count != 0 || embeds.Count != 0) {
            for (int i = 0; i < attached.Count; i++) {
                await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync(attached[i].ProxyUrl);
            }
            for (int i = 0; i < embeds.Count; i++) {
                await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync(embed: embeds[i]);
            }
        }
        await command.RespondAsync("We've recieved your report, and it will be reviewed by our moderation team.", ephemeral: true);
    }
}