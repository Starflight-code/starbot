using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace StarBot.UserReports;

class Report {
    public readonly SocketMessageCommand command;
    public readonly string content;
    public readonly List<Attachment> attached;
    public readonly List<Embed> embeds;
    public readonly ReportEmbed embed;
    public struct MessageAttachment {
        public readonly bool isEmbed;
        public readonly Embed? embed;
        public readonly string? URL;
        public MessageAttachment(Embed embed) {
            isEmbed = true;
            this.embed = embed;
            URL = default;
        }
        public MessageAttachment(string URL) {
            isEmbed = false;
            this.URL = URL;
            embed = default;
        }
    }

    public Report(SocketMessageCommand command) {
        SocketMessage reportedMessage = command.Data.Message;
        this.command = command;
        content = reportedMessage.CleanContent.Trim() == "" ? "<No Content>" : reportedMessage.CleanContent.Trim();
        attached = reportedMessage.Attachments.ToList();
        embeds = reportedMessage.Embeds.ToList();
        embed = new ReportEmbed(this);
    }

    public string getAttachmentSummary() {
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
        return attachmentSummary;
    }

    public List<MessageAttachment> getSendList(RestUserMessage reportEmbedMessage) {
        List<MessageAttachment> attachmentSendList = new();
        if (attached.Count != 0 || embeds.Count != 0) {
            for (int i = 0; i < embeds.Count; i++) { // echos report message embeds in the log channel
                if (embeds[i].Url != null && embeds[i].Url.Contains("tenor")) { // tenor URLs require special handling (not valid embeds but attached as embed)
                    attachmentSendList.Add(new(embeds[i].Video.ToString()));
                } else {
                    attachmentSendList.Add(new(embed.modifyEmbedForReport(this, reportEmbedMessage, embeds[i])));
                }
                //for (int i = 0; i < attached.Count; i++) { // echos report message attachments in the log channel
                if (0 < attached.Count) {
                    attachmentSendList.Add(new(reportEmbedMessage.GetJumpUrl() + " - " + embed.attachedURLs));
                }
            }
        }
        return attachmentSendList;
    }
}