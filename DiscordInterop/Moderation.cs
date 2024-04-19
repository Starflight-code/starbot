using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using StarBot;
using StarBot.DiscordInterop;

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

    HashSet<ulong> seenIDs = new();
    HashSet<ulong> approvedIDs = new();
    public Moderation() {
        webClient.Timeout = TimeSpan.FromMinutes(10);
    }
    public async Task HandleChatMessage(SocketMessage message, DiscordSocketClient? client, Database data) {
        Console.WriteLine($"Message Recieved {message.CleanContent}");
        if (message.Channel.GetChannelType() != ChannelType.Text) {
            return;
        }

        ulong guildId = (message.Channel as SocketGuildChannel).Guild.Id;

        if (seenIDs.Contains(guildId) && !approvedIDs.Contains(guildId) || data.fetchValue("Ai Channel", guildId) == "") {
            seenIDs.Add(guildId);
            return; // if server is not AI enabled, do not monitor
        }
        seenIDs.Add(guildId);
        approvedIDs.Add(guildId);
        //Console.WriteLine("Valid Guild");

        if (/*UserManager.isStaff(client, guildId, message.Author.Id) || */UserManager.isBot(client, message.Author.Id)) {
            return; // doesn't watch staff or bot spam
        }

        if (message.CleanContent.Trim() == "") {
            return; // null messages can't be scanned
        }

        string prompt = "You are a moderator and decide if messages violate rules. " +
        "Only mark a message as a rule violation if you're confident that the violation is present.\n" +
        //"Reply with only 0 if the message is compliant or 1 if the message violates the rules." +
        "Respond only with a list of violated rule numbers or \"\" if no rules are violated.\n" +
        "Example Response: \"1, 3, 7\" or \"2, 4\"\n" +
        //"Respond with \"\" if no rules are violated." +
        "\n" +
        "Rules:\n" +
        "1: Don't be excessively mean\n" +
        "2: Don't use explicit language, adult content or phrases\n" +
        "3: No personally identifiable information allowed\n" +
        "4: Don't participate in fraudulent activity\n" +
        "5: Advertisements are not allowed\n" +
        "6: Post content in appropriate channels\n" +
        "7: Don't discuss moderation actions (warns, mutes, bans) or appeals\n" +
        "8: No non-english allowed\n" +
        "9: Avoid posting offsite links except to moderated platforms like Youtube or Twitch";// +
                                                                                              //"Respond only with rule numbers or \"\" if no rules are violated.";
        await Task.Run(async () => {
            //Console.WriteLine("Starting AI Processing...");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate"); //\"{message.CleanContent}\"
            request.Content = new StringContent(JsonConvert.SerializeObject(new modelSend(prompt, $"Message: \"{message.CleanContent}\"")), null, "text/plain");

            HttpResponseMessage response = await webClient.SendAsync(request);
            try {
                //Console.WriteLine($"Status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
            } catch {
                //Console.WriteLine("Error, HTTP Request Failed (AI)");
                return;
            }
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
            modelOutput json = JsonConvert.DeserializeObject<modelOutput>(await response.Content.ReadAsStringAsync());
            //Console.WriteLine($"Finished with: {json.response}");
            //Console.WriteLine($"Output: {output.response}");
            char[] output = json.response.Trim().ToCharArray();
            HashSet<int> returnVal = new();
            for (int i = 0; i < output.Length; i++) {
                try {
                    if (output[i] == '(') { break; }
                    returnVal.Add(int.Parse(output[i].ToString()));
                } catch { }
            }
            //Console.WriteLine(returnVal.ToArray());
            handleAIModeration(returnVal, guildId, message, client, data);

        });
    }

    private void handleAIModeration(HashSet<int> results, ulong guildId, SocketMessage message, DiscordSocketClient? client, Database data) {
        if (results.Count == 0) {
            return;
        }
        SocketGuild guild = client.GetGuild(guildId);
        //Console.WriteLine("Pushing to output channel");
        guild.GetTextChannel(ulong.Parse(data.fetchValue("Ai Channel", guildId))).SendMessageAsync($"User: {message.Author.Username} Potential Violations: {string.Join(", ", results)} {message.GetJumpUrl()}\n ```{message.CleanContent}```");
    }

}