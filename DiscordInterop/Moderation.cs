using System.Net;
using System.Net.Http.Json;
using Discord;
using Discord.WebSocket;
using Flurl.Http;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using StarBot;

class Moderation {
    struct modelSend {
        public string model = "mistral-openorca";
        public string prompt;
        public string system;
        public bool stream = false;
        public modelSend(string systemPrompt, string prompt) {
            system = systemPrompt;
            this.prompt = prompt;
        }
    }
    struct modelOutput {
        public string model;
        public DateTime created_at;
        public string response;
        public int[] context;
        public ulong total_duration;
        public ulong load_duration;
        public int prompt_eval_count;
        public ulong prompt_eval_duration;
        public int eval_count;
        public ulong eval_duration;
    }
    HttpClient webClient = new();
    public Moderation() {
    }
    public async Task HandleChatMessage(SocketMessage message, DiscordSocketClient? client, Database data) {
        if (message.Channel.GetType() != typeof(SocketGuildChannel)) {
            return;
        }

        ulong guildId = (message.Channel as SocketGuildChannel).Guild.Id;

        if (data.fetchValue("Ai Channel", guildId) == "") {
            return;
        }

        string prompt = "You are a moderator and decide if messages violate rules. " +
        //"Reply with only 0 if the message is compliant or 1 if the message violates the rules." +
        "Reply with only a list of rules the message violates. Output: \"1, 2, 4\" or \"\"" +
        "\n" +
        "Rules:\n" +
        "1: Show kindness to everyone\n" +
        "2: Don't use explicit language, adult content or phrases\n" +
        "3: Don't share personal/sensitive information\n" +
        "4: Don't participate in fraudulent activity\n" +
        "5: Advertisements are not allowed\n" +
        "6: Post content in appropriate channels\n" +
        "7: Do not discuss moderation action in public channels\n" +
        "8: English Only\n" +
        "9: Avoid posting offsite links except to moderated platforms like Youtube or Twitch";
        await Task.Run(async () => {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate");
            var content = new StringContent(JsonConvert.SerializeObject(new modelSend(prompt, $"Message: \"{message.CleanContent}\"")), null, "text/plain");
            request.Content = content;
            var response = await webClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
            modelOutput json = JsonConvert.DeserializeObject<modelOutput>(await response.Content.ReadAsStringAsync());
            //Console.WriteLine($"Output: {output.response}");
            string[] output = json.response.Trim().Replace("\\", "").Replace("\"", "").Split(',');
            List<int> returnVal = new();
            for (int i = 0; i < output.Length; i++) {
                try {
                    returnVal.Add(int.Parse(output[i].Trim()));
                } catch { }
            }
            //Console.WriteLine(returnVal.ToString());


        });
    }

    private void handleAIModeration(int[] results, ulong guildId, SocketMessage message, DiscordSocketClient? client, Database data) {
        if (results.Length == 0) {
            return;
        }
        SocketGuild guild = client.GetGuild(guildId);
        guild.GetTextChannel(ulong.Parse(data.fetchValue("Ai Channel", guildId))).SendMessageAsync($"User: {message.Author.Username} Potential Violations: {string.Join(", ", results)} {message.GetJumpUrl()}\n ```{message.CleanContent}```");
    }

}