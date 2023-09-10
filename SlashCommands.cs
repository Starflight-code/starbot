using Discord;
using Discord.WebSocket;

namespace StarBot
{
    internal class SlashCommands
    {
        public static async Task keySet(SocketSlashCommand command, DiscordSocketClient? client, Database data)
        {

            if (Statics.userHasRole(client, command.GuildId, command.User.Id, 696818216080769025))
            {
                var commandArgs = command.Data.Options.ToArray();
                string? key = commandArgs[0].Value.ToString();
                string? value = commandArgs[1].Value.ToString();
                if (key == null || value == null)
                {
                    await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
                    return;
                }
                await data.setValue(key, value);
                await command.RespondAsync("Changes applied: " + key + " -> " + value + "\nKey Value pair mapping completed. Changes will sync immediately.", ephemeral: true);
                await data.updateDB();
                await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB(); });
            }
        }
        public static async Task keyRemove(SocketSlashCommand command, DiscordSocketClient? client, Database data)
        {
            if (Statics.userHasRole(client, command.GuildId, command.User.Id, 696818216080769025))
            {
                var commandArgs = command.Data.Options.ToArray();
                string? key = commandArgs[0].Value.ToString();
                if (key == null)
                {
                    await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
                    return;
                }
                data.removeValue(key);
                await command.RespondAsync("Changes applied: \"" + key + "\" - REMOVED" + "\nChanges will sync immediately.", ephemeral: true);
                await data.updateDB();
                await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB(); });
            }
        }
        public static async Task starbotInterest(SocketSlashCommand command, DiscordSocketClient? client)
        {

            var commandArgs = command.Data.Options.ToArray();
            int interested = (int)commandArgs[0].Value;
            if (command.GuildId == null)
            {
                await command.RespondAsync("Execution Failed, invalid arguments were provided.", ephemeral: true);
                return;
            }
            string uiStatus = "";
            if (interested == 1)
            {
                uiStatus = "APPROVED";
                await client.GetGuild((ulong)command.GuildId).GetUser(command.User.Id).AddRoleAsync(1143808465194713108);
            }
            else
            {
                uiStatus = "REMOVED";
                await client.GetGuild((ulong)command.GuildId).GetUser(command.User.Id).RemoveRoleAsync(1143808465194713108);
            }
            await command.RespondAsync($"Your interest in StarBot is appreciated. Your access to backend channels was **{uiStatus}**.\nThis action has been logged. Access to these channels may be revoked at any time for any reason.", ephemeral: true);
            await client.GetGuild((ulong)command.GuildId).GetUser(command.User.Id).AddRoleAsync(1143808465194713108);
            await (client.GetChannel(1143815199816699944) as SocketTextChannel).SendMessageAsync(embed: new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithImageUrl(command.User.GetAvatarUrl())
                .WithTitle($"Access to StarBot Interest Program {uiStatus}")
                .WithDescription($"Username: {command.User.Username}\nStatus: {uiStatus}\nScope:\n- #devlog\n- #autosync-backend")
                .Build());

        }
    }
}