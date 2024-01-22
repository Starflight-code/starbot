using System.Net.Cache;
using Discord;
using Discord.WebSocket;
using NCrontab;
using StarBot.Caching;

namespace StarBot {
    internal class Scheduler {

        public struct scheduledTask {
            public CrontabSchedule schedule;
            public Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> lambda;
            public string name;
            public scheduledTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> lambda, string name) {
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
        public void registerTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> lambda, string taskName) {
            tasks.Add(new scheduledTask(schedule, lambda, taskName));
        }
        public void findNextUp() {
            nextUp.Clear();
            if (Config.DEBUG_MODE) {
                for (int i = 0; i < tasks.Count; i++) {
                    nextUp.Add(i);
                }
                return;
            }
            int soonestIndex = 0;
            for (int i = 0; i < tasks.Count; i++) {
                if (tasks[i].schedule.GetNextOccurrence(DateTime.Now) < tasks[soonestIndex].schedule.GetNextOccurrence(DateTime.Now)) {
                    soonestIndex = i;
                }
            }
            DateTime soonestOccurrence = tasks[soonestIndex].schedule.GetNextOccurrence(DateTime.Now);
            for (int i = 0; i < tasks.Count; i++) {
                if (tasks[i].schedule.GetNextOccurrence(DateTime.Now) == soonestOccurrence) {
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
            if (!Config.DEBUG_MODE) {
                await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB(guildID); });
            }
        }

        public async Task addInvokeCommand(SocketGuild? guild) {
            var report = new MessageCommandBuilder();
            report.WithName("Report Message");
            report.WithDMPermission(false);
            await guild.CreateApplicationCommandAsync(report.Build());

            var scheduledTaskInvoke = new SlashCommandBuilder();
            scheduledTaskInvoke.WithName("execute-task");
            scheduledTaskInvoke.WithDescription("Invoke a normally scheduled task manually");
            var builder = new SlashCommandOptionBuilder()
                .WithName("task-name")
                .WithDescription("Which task would you like to invoke?")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            for (int i = 0; i < tasks.Count; i++) {
                builder.AddChoice(tasks[i].name, i);
            }
            scheduledTaskInvoke.AddOption(builder);
            await guild.CreateApplicationCommandAsync(scheduledTaskInvoke.Build());

            var scheduledTaskSetup = new SlashCommandBuilder();
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
            scheduledTaskSetup.AddOption(builder);
            await guild.CreateApplicationCommandAsync(setupBuilder.Build());

        }
        public async Task invokeTask(int taskIndex, DiscordSocketClient client, Database data, Caching.MemoryCacheManager cacheManager, ulong guildID) {
            await tasks[taskIndex].lambda.Invoke(client, data, guildID, cacheManager);
            await databaseUpdate(client, data, guildID);
        }
        public void logNextUp() {
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
        public async Task schedulerProcess(DiscordSocketClient client, Database data, MemoryCacheManager cacheManager) {
            try {
                while (true) {
                    findNextUp();
                    logNextUp();
                    await Task.Delay(Config.DEBUG_MODE ? 5000 : waitTimeNextUp()); // waits for 5 seconds in debug mode, otherwise waits the correct time.
                    for (int i = 0; i < nextUp.Count; i++) {

                        foreach (SocketGuild guild in client.Guilds) {
                            await tasks[nextUp[i]].lambda.Invoke(client, data, guild.Id, cacheManager);
                        }
                        Console.WriteLine($"Executed Task {getTaskName(i)}");
                    }
                    foreach (SocketGuild guild in client.Guilds) {
                        await databaseUpdate(client, data, guild.Id);
                    }
                }
            } catch (Exception e) // logs exceptions to Discord
                    {
                var channel = client.GetChannel(Config.ERROR_LOG_CHANNEL) as SocketTextChannel;
                await channel.SendMessageAsync(DateTime.Now.ToString() + ": " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }
    }
}