using System.Security.Cryptography.X509Certificates;
using Discord.WebSocket;
using StarBot.Caching;

namespace StarBot.DiscordInterop;
internal class SlashCommands {
    /*public static async Task KeySet(SocketSlashCommand command, DiscordSocketClient? client, Database data) {
        if (command.GuildId == null) { return; }

        if (UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            var commandArgs = command.Data.Options.ToArray();
            string? key = commandArgs[0].Value.ToString();
            string? value = commandArgs[1].Value.ToString();
            if (key == null || value == null) {
                await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
                return;
            }
            await data.setValue(key, value, (ulong)command.GuildId);
            await command.RespondAsync("Changes applied: " + key + " -> " + value + "\nKey Value pair mapping completed. Changes will sync immediately.", ephemeral: true);
            await data.updateDB((ulong)command.GuildId);
            await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB((ulong)command.GuildId); });
        }
    }
    public static async Task KeyRemove(SocketSlashCommand command, DiscordSocketClient? client, Database data) {
        if (command.GuildId == null) { return; }
        if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            return;
        } // permission check

        var commandArgs = command.Data.Options.ToArray();
        string? key = commandArgs[0].Value.ToString();
        if (key == null) { // argument check
            await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
            return;
        }

        data.removeValue(key, (ulong)command.GuildId);
        await command.RespondAsync("Changes applied: \"" + key + "\" - REMOVED" + "\nChanges will sync immediately.", ephemeral: true);
        await data.updateDB((ulong)command.GuildId);
    }*/
    /*public static async Task KeyList(SocketSlashCommand command, DiscordSocketClient? client, Database data) {
        if (command.GuildId == null) { return; }
        if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            return;
        } // permission check

        string[]? keys = data.getKeys((ulong)command.GuildId);
        string output = "";
        for (int i = 0; i < keys.Length; i++) {
            string? value = data.fetchValue(keys[i], (ulong)command.GuildId);
            if (i != 0) { output += "\n- "; }
            output += $"{keys[i]} - {value}";
        }

        await command.RespondAsync("Keys associated with the current guild: \n" + output, ephemeral: true);
    }*/
    public static async Task ExecuteTask(SocketSlashCommand command, Scheduler scheduler, DiscordSocketClient client, SqlDatabase data, MemoryCacheManager cache) {
        if (command.GuildId == null) { return; }
        await command.DeferAsync(ephemeral: true);
        if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            await command.FollowupAsync("You do not have the required permissions to execute this command.", ephemeral: true);
            return;
        }
        var commandArgs = command.Data.Options.ToArray();
        int taskIndex = unchecked((int)(Int64)commandArgs[0].Value);

        await scheduler.invokeTask(taskIndex, client, data, cache, (ulong)command.GuildId);
        await command.FollowupAsync($"Task \"{scheduler.getTaskName(taskIndex)}\" executed.", ephemeral: true);
    }

    public static async Task SetUpTask(SocketSlashCommand command, Scheduler scheduler, DiscordSocketClient client, SqlDatabase data, MemoryCacheManager cache) {
        if (command.GuildId == null || command.ChannelId == null) { return; }
        await command.DeferAsync(ephemeral: true);
        if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            await command.FollowupAsync("You do not have the required permissions to execute this command.", ephemeral: true);
            return;
        }
        var commandArgs = command.Data.Options.ToArray();
        int taskIndex = unchecked((int)(Int64)commandArgs[0].Value);

        data.writeToDB<ulong>(Config.TASK_NAMES[taskIndex] + "Channel", (ulong)command.ChannelId, (ulong)command.GuildId);
        await command.FollowupAsync($"Current channel linked with \"{scheduler.getTaskName(taskIndex)}\".", ephemeral: true);
    }

    public static async Task SetupChannels(SocketSlashCommand command, DiscordSocketClient client, SqlDatabase data) {
        await command.DeferAsync(ephemeral: true);
        var commandArgs = command.Data.Options.ToArray();
        int argument = unchecked((int)(Int64)commandArgs[0].Value);

        switch (argument) {
            case 0: // Report Channel
                if (command.ChannelId == null || command.GuildId == null) {
                    return;
                }
                data.writeToDB<ulong>("reportchannel", (ulong)command.ChannelId, (ulong)command.GuildId);
                command.FollowupAsync("Associated current channel with report system.");
                break;
        }
    }
}