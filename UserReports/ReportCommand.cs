using Discord.WebSocket;

namespace StarBot.UserReports;
class ReportCommand {
    public static async Task<bool> initialChecks(SocketMessageCommand command, Database data) {
        if (command.IsDMInteraction || command.GuildId == null) { // in DM channel or erroneous state (not sure why else GuildId would be null)
            await command.FollowupAsync("This command can not be executed in the current enviroment. (In DM Channel or GuildID is null)", ephemeral: true);
            return false;
        }
        ulong reportChannel;
        try {
            reportChannel = ulong.Parse(data.fetchValue("Report Channel", (ulong)command.GuildId));
        } catch {
            await command.FollowupAsync("This function is not set up yet. Consider contacting server administration.");
            return false;
        }
        return true;
    }
}