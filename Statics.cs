using Discord.WebSocket;
using NCrontab;
using System.Runtime.InteropServices;

namespace StarBot {
    internal static class Statics {
        public static string preProcessValue(string value) {
            return value.ToLower().Trim();
        }
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
        public struct scheduledTask {
            public CrontabSchedule schedule;
            public Func<DiscordSocketClient, Database, Caching.MemoryCacheManager, Task> lambda;
            public string name;
            public scheduledTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, Caching.MemoryCacheManager, Task> lambda, string name) {
                this.schedule = schedule;
                this.lambda = lambda;
                this.name = name;
            }
        }
    }
}