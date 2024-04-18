using Discord;
using Discord.WebSocket;
using StarBot.Caching;
using StarBot.DiscordInterop;

namespace StarBot {
    internal class Program {
        private Scheduler scheduler = new();
        private DiscordSocketClient? client;
        private Database? data;
        MemoryCacheManager cacheManager = new();
        Moderation moderation = new();
        //Watcher watcher = new();
        public static Task Main(string[] args) => new Program().MainAsync(args);
        private Task Log(Discord.LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        public async Task MainAsync(string[] args) {
            //await Task.Delay(-1);
            bool ready = false;
            var config = new DiscordSocketConfig { MessageCacheSize = 5 };
            client = new DiscordSocketClient(config);
            if (Config.DISCORD_NET_LOGGING) { // only handles the log event if logging is enabled
                client.Log += Log;
            }
            client.SlashCommandExecuted += SlashCommandHandler;
            client.MessageCommandExecuted += MessageCommandHandler;
            client.MessageReceived += MessageHandler;
            if (args.Length > 0 || Config.KEY != "") {
                await client.LoginAsync(TokenType.Bot, Config.KEY != "" ? Config.KEY : args[0]); // uses Config key in debug mode
            } else {
                Console.WriteLine("You have not specified a key in config or as an argument. This program will now exit.");
                Environment.Exit(1);
            }

            await client.StartAsync(); // client initialization completed

            client.Ready += async () => {
                Console.WriteLine("Bot is connected!");
                data = new(client);
                for (int i = 0; i < data.guilds.Count(); i++) {
                    await Initialization.CreateSlashCommandsAsync(client, client.GetGuild(data.guilds[i]), data/*, watcher*/);
                }
                ready = true;
            };
            while (client.ConnectionState != ConnectionState.Connected || !ready) {
                await Task.Delay(500);
            }
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 12 * * Tue,Thu,Sat"), Lambdas.XKCD_Automation, "XKCD Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0 * * *"), Lambdas.CatDaily_Automation, "Cat Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0/8 * * *"), Lambdas.AnimeDaily_Automation, "Anime Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0/8 * * *"), Lambdas.AniMemesDaily_Automation, "Animemes Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0 * * *"), Lambdas.QuestionOfTheDay_Automation, "Question of the Day Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0 * * *"), Lambdas.DBD_Automation, "Dead by Daylight Automation");

            for (int i = 0; i < data.guilds.Count(); i++) {
                await scheduler.addInvokeCommand(client.GetGuild(data.guilds[i]), data/*, watcher*/);
            }

            if (data == null) { return; }
            await scheduler.schedulerProcess(client, data, cacheManager/*, watcher*/);
            await Task.Delay(-1);
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            //Console.WriteLine($"{message} -> {after}");
        }
        private async Task SlashCommandHandler(SocketSlashCommand command) {
            if (client == null || data == null) { return; }
            if (command.IsDMInteraction) { await command.RespondAsync("This command can not be used in a DM."); return; }
            switch (command.CommandName) {
                case "key-modify":
                    await SlashCommands.keySet(command, client, data);
                    break;
                case "key-remove":
                    await SlashCommands.keyRemove(command, client, data);
                    break;
                case "keys-list":
                    await SlashCommands.keyList(command, client, data);
                    break;
                case "setup-channel":
                    await SlashCommands.setupChannels(command, client, data);
                    break;
                case "execute-task":
                    await SlashCommands.executeTask(command, scheduler, client, data, cacheManager);
                    break;
                case "set-task-channel":
                    await SlashCommands.setUpTask(command, scheduler, client, data, cacheManager);
                    break;
            }
        }

        private async Task MessageCommandHandler(SocketMessageCommand command) {
            if (client == null || data == null) {
                await command.RespondAsync("A catastrophic error has been detected. This command will not be executed! (DiscordSocketClient object or Database is null)", ephemeral: true);
                return;
            }
            switch (command.CommandName) {
                case "Report Message":
                    await MessageCommands.UserReport(command, client, data);
                    break;
            }
        }

        private async Task MessageHandler(SocketMessage message) {
            if (client == null || data == null) { return; }
            await moderation.HandleChatMessage(message, client, data);
        }
    }
}