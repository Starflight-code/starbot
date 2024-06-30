using Discord;
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
    public static bool userHasManageServer(DiscordSocketClient? client, ulong? guildID, ulong userID) {
        if (guildID == null) {
            return false;
        }
        var userPermissions = client.GetGuild((ulong)guildID).GetUser(userID).GuildPermissions;
        return userPermissions.Has(Discord.GuildPermission.ManageGuild);
    }

    public static bool isStaff(DiscordSocketClient? client, ulong? guildID, ulong userID) {
        if (guildID == null) {
            return false;
        }
        GuildPermissions userPermissions = client.GetGuild((ulong)guildID).GetUser(userID).GuildPermissions;
        return userPermissions.Has(Discord.GuildPermission.ModerateMembers);
    }

    public static bool isBot(DiscordSocketClient? client, ulong userID) {
        return client.GetUser(userID).IsBot;
    }
    public static async Task<bool> userBlackisted(SqlDatabase data, ulong? guildID, ulong userID, string command) {
        if (guildID == null) {
            return false;
        }
        string? outputArray = await data.readFromDB<string>($"{command} blacklist", (ulong)guildID);
        string[] outArray = outputArray.Split(',');
        HashSet<string> arraySet = outArray.ToHashSet();

        return arraySet.Contains(userID.ToString());
    }
}