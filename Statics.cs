using Discord.WebSocket;
using System.Runtime.InteropServices;

namespace StarBot
{
    internal static class Statics
    {
        public static string preProcessValue(string value)
        {
            return value.ToLower().Trim();
        }
        public static string buildPath(string windowsPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return windowsPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return windowsPath.Replace('\\', '/');
            }
            else
            {
                return windowsPath; // not a supported OS for buildPath, we do not need total support
                                    // due to the current deployment plan (closed source, internal use only)
            }
        }
        public static bool userHasRole(DiscordSocketClient? client, ulong? guildID, ulong userID, ulong roleID)
        {
            if (guildID == null)
            {
                return false;
            }
            var userRoles = client.GetGuild((ulong)guildID).GetUser(userID).Roles;
            foreach (SocketRole role in userRoles)
            {
                if (role.Id == roleID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
