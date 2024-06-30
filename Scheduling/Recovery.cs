using Debug;
using Discord.WebSocket;
using static StarBot.Scheduler;

class Recovery {
    public static bool attemptRecovery(List<SocketGuild> guildsAffected, scheduledTask lambda, Discord.WebSocket.DiscordSocketClient client, SqlDatabase data, StarBot.Caching.MemoryCacheManager cache, DebugComms debug) {
        const int RECOVERY_ATTEMPTS = 3;
        int failCount = 0;
        int progressIndex = 0;
        while (failCount <= RECOVERY_ATTEMPTS) {
            try {
                for (int i = progressIndex; i < guildsAffected.Count(); i++) {
                    lambda.lambda(client, data, guildsAffected[i].Id, cache, debug);
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