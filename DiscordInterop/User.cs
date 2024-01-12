using Discord.WebSocket;

namespace StarBot.DiscordInterop;
internal class UserManager {
    public static bool userHasRole(DiscordSocketClient? client, ulong? guildID, ulong userID, ulong roleID) {
        if (guildID == null) {
            return false;
        }
        var userRoles = client.GetGuild((ulong)guildID).GetUser(userID).Roles;
        foreach (SocketRole role in userRoles) {
            if (role.Id == roleID) {
                return true;
            }
        }
        return false;
    }
}