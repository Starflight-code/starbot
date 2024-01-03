using Discord;
using Discord.WebSocket;
using NCrontab;

namespace StarBot {
    internal class Scheduler {
        List<Statics.scheduledTask> tasks = new List<Statics.scheduledTask>();
        List<int> nextUp = new List<int>();

        public string getTaskName(int taskIndex) {
            return tasks[taskIndex].name;
        }
        public void registerTask(CrontabSchedule schedule, Func<DiscordSocketClient, Database, Task> lambda, string taskName) {
            tasks.Add(new Statics.scheduledTask(schedule, lambda, taskName));
        }
        public void findNextUp() {
            if (Config.DEBUG_MODE) {
                for (int i = 0; i < tasks.Count(); i++) {
                    nextUp.Add(i);
                }
                return;
            }
            nextUp.Clear();
            int soonestIndex = 0;
            for (int i = 0; i < tasks.Count(); i++) {
                if (tasks[i].schedule.GetNextOccurrence(DateTime.Now) < tasks[soonestIndex].schedule.GetNextOccurrence(DateTime.Now)) {
                    soonestIndex = i;
                }
            }
            DateTime soonestOccurrence = tasks[soonestIndex].schedule.GetNextOccurrence(DateTime.Now);
            for (int i = 0; i < tasks.Count(); i++) {
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
        public async Task databaseUpdate(DiscordSocketClient client, Database data) {
            await data.updateDB();
            if (!Config.DEBUG_MODE) {
                await (client.GetChannel(1125899458002034799) as SocketTextChannel).ModifyMessageAsync(1143042164490772502, m => { m.Content = data.getSerializedDB(); });
            }
        }

        public async Task addInvokeCommand(SocketGuild? guild) {
            var scheduledTaskInvoke = new SlashCommandBuilder();
            scheduledTaskInvoke.WithName("execute-task");
            scheduledTaskInvoke.WithDescription("Invoke a normally scheduled task manually");
            var builder = new SlashCommandOptionBuilder()
                .WithName("task-name")
                .WithDescription("Which task would you like to invoke?")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer);
            for (int i = 0; i < tasks.Count(); i++) {
                builder.AddChoice(tasks[i].name, i);
            }
            scheduledTaskInvoke.AddOption(builder);
            await guild.CreateApplicationCommandAsync(scheduledTaskInvoke.Build());

        }
        public async Task invokeTask(int taskIndex, DiscordSocketClient client, Database data) {
            await tasks[taskIndex].lambda.Invoke(client, data);
            await databaseUpdate(client, data);
        }
        public void logNextUp() {
            Console.WriteLine("Waiting until " + waitTimeReadable());
            string queued = "";
            for (int j = 0; j < nextUp.Count(); j++) {
                if (j != 0) {
                    queued += ", ";
                }
                queued += getTaskName(nextUp[j]);
            }
            Console.WriteLine("Queued Tasks: " + queued);
        }
        public async Task schedulerProcess(DiscordSocketClient client, Database data) {
            try {
                while (true) {
                    findNextUp();
                    logNextUp();
                    await Task.Delay(Config.DEBUG_MODE ? 5000 : waitTimeNextUp()); // waits for 5 seconds in debug mode, otherwise waits the correct time.
                    for (int i = 0; i < nextUp.Count(); i++) {
                        await tasks[nextUp[i]].lambda.Invoke(client, data);
                        Console.WriteLine($"Executed Task {getTaskName(i)}");
                    }
                    await databaseUpdate(client, data);
                }
            } catch (Exception e) // logs exceptions to Discord
                    {
                var channel = client.GetChannel(1187007545357905980) as SocketTextChannel;
                await channel.SendMessageAsync(DateTime.Now.ToString() + ": " + e.Message);
                throw;
            }
        }
    }
}