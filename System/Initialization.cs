using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using StarBot;

public static class Initialization {
    public static async Task CreateSlashCommandsAsync(DiscordSocketClient client, SocketGuild guild, Database data/*, Watcher watcher*/) {
        var dbkeymodify = new SlashCommandBuilder();
        var dbkeyremove = new SlashCommandBuilder();
        var dbkeylist = new SlashCommandBuilder();
        var setupChannels = new SlashCommandBuilder();

        dbkeymodify.WithName("key-modify");
        dbkeymodify.WithDescription("Modify a key value pair in the database");
        dbkeymodify.AddOption("key", ApplicationCommandOptionType.String, "The key you would like to modify", isRequired: true);
        dbkeymodify.AddOption("value", ApplicationCommandOptionType.String, "The value you would like to map the key to", isRequired: true);

        dbkeyremove.WithName("key-remove");
        dbkeyremove.WithDescription("Remove a key value pair in the database");
        dbkeyremove.AddOption("key", ApplicationCommandOptionType.String, "The key you would like to remove", isRequired: true);

        dbkeylist.WithName("keys-list");
        dbkeylist.WithDescription("List all key-value pairs for this guild");

        setupChannels.WithName("setup-channel");
        setupChannels.WithDescription("Associate systems with log channels to make them work.");
        setupChannels.AddOption(new SlashCommandOptionBuilder()
            .WithName("assign-channel")
            .WithDescription("Which function do you want to assign to the current channel?")
            .AddChoice("Report Log", 0)
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Integer));

        try {
            await guild.DeleteApplicationCommandsAsync(); // disabled global watcher system, pending full removal (too complex for current project)
            /*watcher.RegisterCommand((*/
            await guild.CreateApplicationCommandAsync(dbkeymodify.Build())/*).Id, guild.Id, data, dbkeymodify.Build())*/;
            /*watcher.RegisterCommand((*/
            await guild.CreateApplicationCommandAsync(dbkeyremove.Build())/*).Id, guild.Id, data, dbkeyremove.Build())*/;
            await guild.CreateApplicationCommandAsync(dbkeylist.Build());
            /*watcher.RegisterCommand((*/
            await guild.CreateApplicationCommandAsync(setupChannels.Build())/*).Id, guild.Id, data, setupChannels.Build())*/;


        } catch (HttpException exception) {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;
            await channel.SendMessageAsync($"Slash Command Initialization Error: \n{json}");
        }
    }
}