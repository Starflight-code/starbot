using Discord;
using Discord.WebSocket;
using StarBot.Caching;
using StarBot.DiscordInterop;

namespace StarBot {
    internal class Program {
        private Scheduler scheduler = new();
        private Database data = new();
        private DiscordSocketClient? client;
        SocketGuild? guild;
        MemoryCacheManager cacheManager = new();
        public static Task Main(string[] args) => new Program().MainAsync(args);
        private Task Log(Discord.LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        public async Task MainAsync(string[] args) {
            bool ready = false;
            var config = new DiscordSocketConfig { MessageCacheSize = 5 };
            client = new DiscordSocketClient(config);
            if (Config.DISCORD_NET_LOGGING) { // only handles the log event if logging is enabled
                client.Log += Log;
            }
            client.SlashCommandExecuted += SlashCommandHandler;
            if (args.Length > 0 || Config.DEBUG_MODE) {
                await client.LoginAsync(TokenType.Bot, Config.DEBUG_MODE ? Config.KEY : args[0]); // uses Config key in debug mode
            } else {
                Console.WriteLine("You have not specified a key and this binary is not in debug mode.");
                Environment.Exit(1);
            }

            await client.StartAsync(); // client initialization completed

            client.Ready += async () => {
                Console.WriteLine("Bot is connected!");
                this.guild = client.GetGuild(696808297805774888);
                ready = true;
                await Initialization.CreateSlashCommandsAsync(client, guild);
            };
            while (client.ConnectionState != ConnectionState.Connected || !ready) {
                await Task.Delay(500);
            }
            if (data.fetchValue("FirstRun") == "") { // import data from Discord upon first run
                string syncMessage = (await (client.GetChannel(1125899458002034799) as SocketTextChannel).GetMessageAsync(1143042164490772502)).CleanContent; // cross bot instance automatic sync/cloud backup using Discord

                data.setSerializedDB(syncMessage);
                await data.updateDB();
            }

            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 12 * * Tue,Thu,Sat"), Lambdas.XKCD_Automation, "XKCD Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0 * * *"), Lambdas.CatDaily_Automation, "Cat Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0/8 * * *"), Lambdas.AnimeDaily_Automation, "Anime Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0/8 * * *"), Lambdas.AniMemesDaily_Automation, "Animemes Automation");
            scheduler.registerTask(NCrontab.CrontabSchedule.Parse("0 0 * * *"), Lambdas.QuestionOfTheDay_Automation, "Question of the Day Automation");

            await scheduler.addInvokeCommand(guild);
            await scheduler.schedulerProcess(client, data, cacheManager);
            await Task.Delay(-1);
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            //Console.WriteLine($"{message} -> {after}");
        }
        private async Task SlashCommandHandler(SocketSlashCommand command) {
            if (client == null) { return; }
            switch (command.CommandName) {
                case "key-modify":
                    await SlashCommands.keySet(command, client, data);
                    break;
                case "key-remove":
                    await SlashCommands.keyRemove(command, client, data);
                    break;
                case "starbot-interest":
                    await SlashCommands.starbotInterest(command, client);
                    break;
                case "execute-task":
                    await SlashCommands.executeTask(command, scheduler, client, data, cacheManager);
                    break;
            }
        }
    }
}