using Discord;
using Discord.WebSocket;
using StarBot.UserReports;

namespace StarBot.DiscordInterop;

internal class MessageCommands
{
    public static async Task UserReport(SocketMessageCommand command, DiscordSocketClient? client, Database data)
    {

        await command.DeferAsync(ephemeral: true);

        if (!await ReportCommand.initialChecks(command, data))
        {
            return;
        }
        ulong reportChannel = ulong.Parse(data.fetchValue("Report Channel", (ulong)command.GuildId));

        Report report = new Report(command);

        var message = await (client.GetGuild((ulong)command.GuildId).GetChannel(reportChannel) as SocketTextChannel).SendMessageAsync(embed: report.embed.generateEmbed(report));

        List<Report.MessageAttachment> toSend = report.getSendList(message);

        for (int i = 0; i < toSend.Count; i++)
        {
            if (toSend[i].isEmbed)
            {
                await (client.GetGuild((ulong)command.GuildId).GetChannel(reportChannel) as SocketTextChannel).SendMessageAsync(embed: toSend[i].embed);
            }
            else
            {
                await (client.GetGuild((ulong)command.GuildId).GetChannel(reportChannel) as SocketTextChannel).SendMessageAsync(toSend[i].URL);
            }
        }



        await command.FollowupAsync(ephemeral: true, embed: new EmbedBuilder
        {
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