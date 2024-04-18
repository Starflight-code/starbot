using System.Net;
using System.Net.Http.Json;
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
    public async Task HandleChatMessage(/*SocketMessage message, DiscordSocketClient? client, Database data*/) {
        //if (message.Channel.GetType() != typeof(SocketGuildChannel)) {
        //    return;
        //}

        //ulong guildId = (message.Channel as SocketGuildChannel).Guild.Id;

        //if (data.fetchValue("AI", guildId) == "") {
        //    return;
        //}

        string prompt = "You are a moderator and decide if messages violate rules." +
        "Reply with only 0 if the message is compliant or 1 if the message violates the rules." +
        "" +
        "Rules:" +
        "1: Show kindness to everyone" +
        "2: Don't use explicit language, adult content or phrases" +
        "3: Don't share personal/sensitive information" +
        "4: Don't participate in fraudulent activity" +
        "5: Advertisements are not allowed" +
        "6: Post content in appropriate channels" +
        "7: Do not discuss moderation action in public channels" +
        "8: English Only" +
        "9: Avoid posting offsite links except to moderated platforms like Youtube or Twitch";
        //await Task.Run(async () => {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate");
        var content = new StringContent(JsonConvert.SerializeObject(new modelSend(prompt, "Message: \"Good morning everyone, how's your day going?\"")), null, "text/plain");
        request.Content = content;
        var response = await webClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        modelOutput output = JsonConvert.DeserializeObject<modelOutput>(await response.Content.ReadAsStringAsync());
        Console.WriteLine($"Output: {output.response}");

        //});

    }
}