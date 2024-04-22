using Debug;
using Discord;
using Discord.WebSocket;
using NCrontab;
using StarBot.Caching;

namespace StarBot {
    internal class Scheduler {

        public struct scheduledTask {
            public CrontabSchedule schedule;
            public Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, DebugComms, Task> lambda;
            public string name;
            public scheduledTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, DebugComms, Task> lambda, string name) {
                this.schedule = schedule;
                this.lambda = lambda;
                this.name = name;
            }
        }
        List<scheduledTask> tasks = new();
        List<int> nextUp = new();

        public string getTaskName(int taskIndex) {
            return tasks[taskIndex].name;
        }
        public void registerTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, DebugComms, Task> lambda, string taskName) {
            tasks.Add(new scheduledTask(schedule, lambda, taskName));
        }
        public void findNextUp() {
            DateTime now = DateTime.Now;
            nextUp.Clear();
            if (Config.DEBUG_MODE) {
                for (int i = 0; i < tasks.Count; i++) {
                    nextUp.Add(i);
                }
                return;
            }
            int soonestIndex = 0;
            for (int i = 0; i < tasks.Count; i++) {
                if (tasks[i].schedule.GetNextOccurrence(now) < tasks[soonestIndex].schedule.GetNextOccurrence(now)) {
                    soonestIndex = i;
                }
            }
            DateTime soonestOccurrence = tasks[soonestIndex].schedule.GetNextOccurrence(now);
            for (int i = 0; i < tasks.Count; i++) {
                if (tasks[i].schedule.GetNextOccurrence(now) == soonestOccurrence) {
                    nextUp.Add(i);
                }
            }
        }
        public int waitTimeNextUp() {
            return (int)(tasks[nextUp[0]].schedule.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
        }
        public string waitTimeReadable() {
            DateTime waitUntil = tasks[nextUp[0]].schedule.GetNextOccurrence(DateTime.Now).ToLocalTime();
            return waitUntil.ToShortTimeString() + " on " + waitUntil.ToShortDateString();
        }
        public async Task databaseUpdate(DiscordSocketClient client, Database data, ulong guildID) {
            await data.updateDB(guildID);
        }

        public async Task addInvokeCommand(SocketGuild? guild, Database data/*, Watcher watcher*/) {
            var report = new MessageCommandBuilder();
            var scheduledTaskInvoke = new SlashCommandBuilder();
            var scheduledTaskSetup = new SlashCommandBuilder();


            report.WithName("Report Message");
            report.WithDMPermission(false);

            scheduledTaskInvoke.WithName("execute-task");
            scheduledTaskInvoke.WithDescription("Invoke a normally scheduled task manually");
            var taskInvokeBuilder = new SlashCommandOptionBuilder()
                .WithName("task-name")
                .WithDescription("Which task would you like to invoke?")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            for (int i = 0; i < tasks.Count; i++) {
                taskInvokeBuilder.AddChoice(tasks[i].name, i);
            }
            scheduledTaskInvoke.AddOption(taskInvokeBuilder);

            scheduledTaskSetup.WithName("set-task-channel");
            scheduledTaskSetup.WithDescription("Set the channel for a scheduled task to run in.");
            var setupBuilder = new SlashCommandOptionBuilder()
                .WithName("task-name")
                .WithDescription("Which task would you like to set up?")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            for (int i = 0; i < tasks.Count; i++) {
                setupBuilder.AddChoice(tasks[i].name, i);
            }
            scheduledTaskSetup.AddOption(setupBuilder);
            await guild.CreateApplicationCommandAsync(report.Build());
            await guild.CreateApplicationCommandAsync(scheduledTaskInvoke.Build());
            await guild.CreateApplicationCommandAsync(scheduledTaskSetup.Build());
        }
        public async Task invokeTask(int taskIndex, DiscordSocketClient client, Database data, Caching.MemoryCacheManager cacheManager, ulong guildID) {
            DebugComms debug = new DebugComms(); // passes through a blank debugComms for compatiblity
            await tasks[taskIndex].lambda.Invoke(client, data, guildID, cacheManager, debug);
            await databaseUpdate(client, data, guildID);
        }

        public void returnTaskDbName(int taskIndex) {

        }
        public void logNextUp(DiscordSocketClient client) {
            Console.WriteLine("Waiting until " + waitTimeReadable());
            string queued = "";
            for (int j = 0; j < nextUp.Count; j++) {
                if (j != 0) {
                    queued += ", ";
                }
                queued += getTaskName(nextUp[j]);
            }
            Console.WriteLine("Queued Tasks: " + queued);
        }
        public async Task schedulerProcess(DiscordSocketClient client, Database data, MemoryCacheManager cacheManager/*, Watcher watcher*/) {
            DebugComms debug = new();
            debug.setVerbosity(true);
            int guildIndexForRecovery = 0;
            int lambdaIndex = 0;
            try {
                while (true) {
                    debug.UpdatePosition("SetActivity");
                    Random random = new();
                    int indexOfNextStatus = random.Next(Config.STATUS_MESSAGES.Length);

                    await client.SetGameAsync(Config.STATUS_MESSAGES[indexOfNextStatus].message, type: Config.STATUS_MESSAGES[indexOfNextStatus].activity);
                    debug.UpdatePosition("findNextUp");
                    findNextUp();
                    logNextUp(client);

                    debug.UpdatePosition("delay");
                    await Task.Delay(Config.DEBUG_MODE ? 5000 : waitTimeNextUp()); // waits for 5 seconds in debug mode, otherwise waits the correct time.
                    await client.SetGameAsync("the internet, sending the best content to your channels.", type: ActivityType.Listening);
                    debug.UpdatePosition("Executing Lambdas");
                    List<SocketGuild> guilds = client.Guilds.ToList();
                    for (int i = 0; i < nextUp.Count; i++) {
                        lambdaIndex = i;
                        debug.UpdatePosition($"Executing Lambdas, on: {getTaskName(nextUp[i])}");
                        try {
                            for (int j = 0; j < guilds.Count(); j++) {
                                guildIndexForRecovery = j;
                                await tasks[nextUp[i]].lambda.Invoke(client, data, guilds[j].Id, cacheManager, debug);
                            }
                            Console.WriteLine($"Executed Task {getTaskName(nextUp[i])}");

                        } catch (Exception e) // logs exceptions to Discord
                        {
                            var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;
                            bool attempt = Recovery.attemptRecovery(guilds.GetRange(guildIndexForRecovery, guilds.Count() - guildIndexForRecovery), tasks[nextUp[lambdaIndex]], client, data, cacheManager, debug);
                            await channel.SendMessageAsync($"{DateTime.Now.ToString()}: {e.Message}\n```{e.StackTrace}```\nRecovery Attempt Successful: {attempt.ToString()}");
                            throw;
                        }
                    }
                    foreach (SocketGuild guild in client.Guilds) {
                        debug.UpdatePosition($"Database Update: {guild.Name} ({guild.Id})");
                        await databaseUpdate(client, data, guild.Id);
                    }
                }
            } catch (Exception e) // logs exceptions to Discord
                    {
                debug.LogState(client, e);
                throw;
            }
        }
    }
}