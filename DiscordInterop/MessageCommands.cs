using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;

namespace StarBot.DiscordInterop;

internal class MessageCommands {
    public static async Task UserReport(SocketMessageCommand command, DiscordSocketClient? client, Database data) {
        if (command.IsDMInteraction || command.GuildId == null) { // in DM channel or erroneous enviroment (not sure why else GuildId would be null)
            await command.RespondAsync("This command can not be executed in the current enviroment. (In DM Channel or GuildID is null)", ephemeral: true);
            return;
        }
        await command.DeferAsync(ephemeral: true);

        var reportedMessage = command.Data.Message;
        var content = reportedMessage.CleanContent.Trim() == "" ? "<No Content>" : reportedMessage.CleanContent.Trim();

        List<Attachment> attached = reportedMessage.Attachments.ToList();
        List<Embed> embeds = reportedMessage.Embeds.ToList();

        string attachmentSummary;

        if (attached.Count == 0 && embeds.Count == 0) { // creates a summary of attached files/embeds for the log
            attachmentSummary = "None";
        } else {
            attachmentSummary = "";
            for (int i = 0; i < attached.Count; i++) {
                if (i != 0) { attachmentSummary += "\n"; }
                attachmentSummary += "- " + attached[i].Filename;
            }
            if (0 < attached.Count && 0 < embeds.Count) {
                attachmentSummary += "\n- " + embeds.Count + " embed(s)";
            } else if (attached.Count == 0 && 0 < embeds.Count) {
                attachmentSummary += "- " + embeds.Count + " embed(s)";
            }
        }

        string attachedURLs = "";
        for (int i = 0; i < attached.Count; i++) { // adds the addToEmbed field
            attachedURLs += attached[i].ProxyUrl + " ";
        }

        // ** Report Log Embed Fields **

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

        var reportedMessageLink = new EmbedFieldBuilder {
            Name = "Message Link",
            Value = reportedMessage.GetJumpUrl(),
            IsInline = false
        };

        var reportedMessageID = new EmbedFieldBuilder {
            Name = "Message ID",
            Value = reportedMessage.Id,
            IsInline = true
        };

        var messageSentAt = new EmbedFieldBuilder {
            Name = "Message Sent",
            Value = TimestampTag.FromDateTimeOffset(reportedMessage.CreatedAt, style: TimestampTagStyles.ShortDateTime).ToString(),
            IsInline = true
        };

        var reportReceivedAt = new EmbedFieldBuilder {
            Name = "Report Received",
            Value = TimestampTag.FromDateTimeOffset(command.CreatedAt, style: TimestampTagStyles.ShortDateTime).ToString(),
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
            Description = content,
            ThumbnailUrl = reportedMessage.Author.GetAvatarUrl(),
            Fields = new List<EmbedFieldBuilder> { reported, reporter, reportedMessageLink, reportedMessageID, messageSentAt, reportReceivedAt, attachments }
        };

        var message = await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync(embed: reportEmbed.Build());

        var addToEmbed = new EmbedFieldBuilder { // embed field added to reported message attachment/embed echo
            Name = "Attached to Report",
            Value = message.GetJumpUrl(),
            IsInline = false
        };

        if (attached.Count != 0 || embeds.Count != 0) {
            for (int i = 0; i < embeds.Count; i++) { // echos report message embeds in the log channel
                await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync(embed: embeds[i]);
            }
            //for (int i = 0; i < attached.Count; i++) { // echos report message attachments in the log channel
            await (client.GetGuild((ulong)command.GuildId).GetChannel(Config.REPORT_LOG_CHANNEL) as SocketTextChannel).SendMessageAsync(message.GetJumpUrl() + " - " + attachedURLs);
            //}
        }



        await command.FollowupAsync(ephemeral: true, embed: new EmbedBuilder {
            Color = Color.Blue,
            Title = "Report Received",
            Description = "We've received your report, and we're looking into it on our side. This report contains a bit of information, such as: " +
            "\n- Your Account Identity" +
            "\n- The Reported User's Account Identity" +
            "\n- Message Content" +
            "\n- Message Attachments/Embeds" +
            "\n- Associated Timestamps" +
            "\n**Thanks for making " + client.GetGuild((ulong)command.GuildId).Name + " a safer place!**"
        }.Build());
    }
}