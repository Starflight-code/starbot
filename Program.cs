using Discord;
using Discord.Net;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarBot {
    internal class Program {
        //public bool debugMode = Config.DEBUG_MODE;
        //private List<CrontabSchedule> scheduleList = new List<CrontabSchedule>();
        //private List<Func<DiscordSocketClient, Database, Task>> scheduledLambdas = new List<Func<DiscordSocketClient, Database, Task>>();
        //private List<int> nextUp = new List<int>();
        private Scheduler scheduler = new();
        private Database data = new();
        SocketGuild? guild;
        public static Task Main(string[] args) => new Program().MainAsync(args);
        private Task Log(Discord.LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static dynamic fetchJSON(string URL) {
            var site = new Url(URL);
            // headers and user agent spoofing are required to avoid a 403 'unauthorized' http error code
            System.Threading.Tasks.Task<string> output = site.WithHeaders(new { Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8", User_Agent = "Mozilla/5.0" }).GetStringAsync();
            output.Wait();
            string outString = output.Result;
            try {
                return JObject.Parse(outString);

            } catch (Newtonsoft.Json.JsonReaderException) {
                return JArray.Parse(outString);
            }
        }

        private DiscordSocketClient? client;
        private HttpClient? web;

        public async Task MainAsync(string[] args) {
            bool ready = false;
            var config = new DiscordSocketConfig { MessageCacheSize = 5 };
            client = new DiscordSocketClient(config);
            web = new HttpClient();
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

            await client.StartAsync();


            // client initialization completed

            client.Ready += async () => {
                Console.WriteLine("Bot is connected!");
                guild = client.GetGuild(696808297805774888);


                var dbkeymodify = new SlashCommandBuilder();
                var dbkeyremove = new SlashCommandBuilder();
                var starbotInterest = new SlashCommandBuilder();


                // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
                dbkeymodify.WithName("key-modify");

                // Descriptions can have a max length of 100.
                dbkeymodify.WithDescription("Modify a key value pair in the database");

                dbkeymodify.AddOption("key", ApplicationCommandOptionType.String, "The key you would like to modify", isRequired: true);
                dbkeymodify.AddOption("value", ApplicationCommandOptionType.String, "The value you would like to map the key to", isRequired: true);

                dbkeyremove.WithName("key-remove");

                // Descriptions can have a max length of 100.
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
                    // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                    // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                    Console.WriteLine(json);
                }

                ready = true;
            };
            while (client.ConnectionState != ConnectionState.Connected && !ready) {
                await Task.Delay(1000);
            }
            if (!ready) {
                await Task.Delay(2000);
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
            await scheduler.schedulerProcess(client, data);
            await Task.Delay(-1);
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
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
                    await SlashCommands.executeTask(command, scheduler, client, data, new Caching.MemoryCacheManager()); // will not use scheduler cache, manual commands may be slower
                    break;
            }
        }
    }
}