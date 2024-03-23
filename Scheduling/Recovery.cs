using Discord.WebSocket;
using static StarBot.Scheduler;

class Recovery {
    public static bool attemptRecovery(List<SocketGuild> guildsAffected, scheduledTask lambda, Discord.WebSocket.DiscordSocketClient client, StarBot.Database data, StarBot.Caching.MemoryCacheManager cache) {
        const int RECOVERY_ATTEMPTS = 3;
        int failCount = 0;
        int progressIndex = 0;
        while (failCount <= RECOVERY_ATTEMPTS) {
            try {
                for (int i = progressIndex; i < guildsAffected.Count(); i++) {
                    lambda.lambda(client, data, guildsAffected[i].Id, cache);
                    progressIndex++;
                }
            } catch (Exception) {
                failCount++;
            }
        }
        if (failCount > RECOVERY_ATTEMPTS) {
            return false;
        } else {
            return true;
        }
    }
}