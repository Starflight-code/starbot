using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace StarBot.UserReports;

class ReportEmbed {
    public readonly List<EmbedFieldBuilder> fields;
    public readonly string attachedURLs;
    EmbedFieldBuilder? addToEmbed;
    public ReportEmbed(Report report) {
        fields = new();
        SocketMessage reportedMessage = report.command.Data.Message;
        fields.Add(new EmbedFieldBuilder { // message content
            Name = "Message Content",
            Value = report.content,
            IsInline = false
        });

        fields.Add(new EmbedFieldBuilder { // reported user
            Name = "Reported",
            Value = reportedMessage.Author.Mention + " - " + reportedMessage.Author.Username,
            IsInline = true
        });

        fields.Add(new EmbedFieldBuilder { // executor
            Name = "Reporter",
            Value = report.command.User.Mention + " - " + report.command.User.Username,
            IsInline = true
        });

        fields.Add(new EmbedFieldBuilder { // link to reported message
            Name = "Message Link",
            Value = reportedMessage.GetJumpUrl(),
            IsInline = false
        });

        fields.Add(new EmbedFieldBuilder { // ID of reported message
            Name = "Message ID",
            Value = reportedMessage.Id,
            IsInline = true
        });

        fields.Add(new EmbedFieldBuilder { // date when reported message was sent
            Name = "Message Sent",
            Value = TimestampTag.FromDateTimeOffset(reportedMessage.CreatedAt, style: TimestampTagStyles.Relative).ToString(),
            IsInline = true
        });

        fields.Add(new EmbedFieldBuilder { // date when command was executed
            Name = "Report Received",
            Value = TimestampTag.FromDateTimeOffset(report.command.CreatedAt, style: TimestampTagStyles.Relative).ToString(),
            IsInline = true
        });

        fields.Add(new EmbedFieldBuilder { // message attachment summary
            Name = "Attachments",
            Value = report.getAttachmentSummary(),
            IsInline = false
        });

        attachedURLs = "";
        for (int i = 0; i < report.attached.Count; i++) { // sends attachments after embed to report channel.
            attachedURLs += report.attached[i].ProxyUrl + " ";
        }
    }

    public Embed generateEmbed(Report report) {
        return new EmbedBuilder {
            Title = "Message Report",
            Color = Color.DarkRed,
            //Description = content,
            ThumbnailUrl = report.command.Data.Message.Author.GetAvatarUrl(),
            Fields = fields
        }.Build();
    }

    public void generateEmbedField(RestUserMessage reportEmbedMessage) {
        addToEmbed = new EmbedFieldBuilder { // embed field added to reported message attachment/embed echo
            Name = "Attached to Report",
            Value = reportEmbedMessage.GetJumpUrl(),
            IsInline = false
        };
    }

    public Embed modifyEmbedForReport(Report report, RestUserMessage reportEmbedMessage, Embed embed) {
        if (addToEmbed == null) {
            generateEmbedField(reportEmbedMessage);
        }
        return embed.ToEmbedBuilder().AddField(addToEmbed).Build();
    }
}