using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using StarBot;

public static class Initialization {
    public static async Task CreateSlashCommandsAsync(DiscordSocketClient client, SocketGuild guild) {
        var dbkeymodify = new SlashCommandBuilder();
        var dbkeyremove = new SlashCommandBuilder();
        var starbotInterest = new SlashCommandBuilder();

        dbkeymodify.WithName("key-modify");
        dbkeymodify.WithDescription("Modify a key value pair in the database");
        dbkeymodify.AddOption("key", ApplicationCommandOptionType.String, "The key you would like to modify", isRequired: true);
        dbkeymodify.AddOption("value", ApplicationCommandOptionType.String, "The value you would like to map the key to", isRequired: true);

        dbkeyremove.WithName("key-remove");
        dbkeyremove.WithDescription("Remove a key value pair in the database");
        dbkeyremove.AddOption("key", ApplicationCommandOptionType.String, "The key you would like to remove", isRequired: true);

        starbotInterest.WithName("starbot-interest");
        starbotInterest.WithDescription("Are you interesting in seeing notes from our developer and some inner workings of StarBot?");
        starbotInterest.AddOption(new SlashCommandOptionBuilder()
            .WithName("interested")
            .WithDescription("See the inner workings and dev notes for StarBot?")
            .AddChoice("Yes", 1)
            .AddChoice("No", 0)
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Integer));

        try {
            // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
            await guild.DeleteApplicationCommandsAsync();
            await guild.CreateApplicationCommandAsync(dbkeymodify.Build());
            await guild.CreateApplicationCommandAsync(dbkeyremove.Build());
            await guild.CreateApplicationCommandAsync(starbotInterest.Build());

        } catch (HttpException exception) {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;
            await channel.SendMessageAsync($"Slash Command Initialization Error: \n{json}");
        }
    }
}