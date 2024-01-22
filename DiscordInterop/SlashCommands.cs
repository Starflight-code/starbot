using Discord;
using Discord.WebSocket;
using StarBot.Caching;

namespace StarBot.DiscordInterop;
internal class SlashCommands {
    public static async Task keySet(SocketSlashCommand command, DiscordSocketClient? client, Database data) {
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
    public static async Task keyRemove(SocketSlashCommand command, DiscordSocketClient? client, Database data) {
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
        await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB((ulong)command.GuildId); });
    }
    /*public static async Task starbotInterest(SocketSlashCommand command, DiscordSocketClient? client) {
        if (command.GuildId == null) { return; }
        var commandArgs = command.Data.Options.ToArray();
        long interested = (long)commandArgs[0].Value;
        if (command.GuildId == null) {
            await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
            return;
        }
        string uiStatus = "";
        if (interested == 1) {
            if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
                uiStatus = "APPROVED";
                await client.GetGuild((ulong)command.GuildId).GetUser(command.User.Id).AddRoleAsync(Config.STARBOT_INTEREST_ROLE_ID);
                await command.RespondAsync($"Your interest in StarBot is appreciated. Your access to backend channels was **{uiStatus}**.\nThis action has been logged. Access to these channels may be revoked at any time for any reason.", ephemeral: true);
            } else {
                await command.RespondAsync("You are already enrolled in this program.", ephemeral: true);
                return;
            }
        } else {
            if (UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
                uiStatus = "REMOVED";
                await client.GetGuild((ulong)command.GuildId).GetUser(command.User.Id).RemoveRoleAsync(Config.STARBOT_INTEREST_ROLE_ID);
                await command.RespondAsync($"Your access to backend channels was **{uiStatus}**.\nThis action has been logged.", ephemeral: true);
            } else {
                await command.RespondAsync("You are not enrolled in this program.", ephemeral: true);
                return;
            }
        }
        await (client.GetChannel(1143815199816699944) as SocketTextChannel).SendMessageAsync(embed: new EmbedBuilder()
            .WithCurrentTimestamp()
            .WithImageUrl(command.User.GetAvatarUrl())
            .WithTitle($"Access {uiStatus} to StarBot Interest Program")
            .WithDescription($"Username: {command.User.Username}\nStatus: {uiStatus}\nScope:\n- #devlog\n- #autosync-backend")
            .Build());

    }*/
    public static async Task executeTask(SocketSlashCommand command, Scheduler scheduler, DiscordSocketClient client, Database data, MemoryCacheManager cache) {
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

    public static async Task setUpTask(SocketSlashCommand command, Scheduler scheduler, DiscordSocketClient client, Database data, MemoryCacheManager cache) {
        if (command.GuildId == null || command.ChannelId == null) { return; }
        await command.DeferAsync(ephemeral: true);
        if (!UserManager.userHasManageServer(client, command.GuildId, command.User.Id)) {
            await command.FollowupAsync("You do not have the required permissions to execute this command.", ephemeral: true);
            return;
        }
        var commandArgs = command.Data.Options.ToArray();
        int taskIndex = unchecked((int)(Int64)commandArgs[0].Value);

        await data.setValue(Config.TASK_NAMES[taskIndex] + " Channel", command.ChannelId.ToString(), (ulong)command.GuildId, true);
        await command.FollowupAsync($"Current channel linked with \"{scheduler.getTaskName(taskIndex)}\".", ephemeral: true);
    }

    public static async Task setupChannels(SocketSlashCommand command, DiscordSocketClient client, Database data) {
        await command.DeferAsync(ephemeral: true);
        var commandArgs = command.Data.Options.ToArray();
        int argument = unchecked((int)(Int64)commandArgs[0].Value);

        switch (argument) {
            case 0: // Report Channel
                await data.setValue("Report Channel", command.ChannelId.ToString(), command.GuildId, true);
                command.FollowupAsync("Associated current channel with report system.");
                break;
        }
    }
}