using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace StarBot {
    internal class Lambdas {
        public static Func<DiscordSocketClient, Database, Task> XKCD_Automation = async (DiscordSocketClient client, Database data) => { // XKCD Automation

            string url = "https://xkcd.com/info.0.json";
            JObject json = Program.fetchJSON(url);
            var channel = client.GetChannel(1106385489180758056) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new() {
                Title = json["safe_title"] + " - " + json["num"],
                Description = json["alt"].ToString(),
                ImageUrl = json["img"].ToString()
            };
            newEmbed.WithFooter("powered by xkcd.com");

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, Task> CatDaily_Automation = async (DiscordSocketClient client, Database data) => { // Cat Daily API
            string url = "https://www.reddit.com/r/cat/.json?limit=100&t=day";

            await data.initializeIterator("CatNumber", 1);

            var channel = client.GetChannel(1106385469312352288) as SocketTextChannel;

            JToken? post = WebManager.SelectRandomRedditPost(url);

            EmbedBuilder newEmbed = new() {
                Title = $"Daily Cat Image #{data.fetchValue("CatNumber")}",
                Description = $"{post["title"]}\n" +
                                   $"https://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/cats/");

            await data.iterateValue("CatNumber");

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, Task> AnimeDaily_Automation = async (DiscordSocketClient client, Database data) => { // Anime Daily API
            string url = "https://www.reddit.com/r/awwnime/.json?limit=100&t=day";
            JObject json = Program.fetchJSON(url);
            Random rand = new();

            JToken? post = WebManager.SelectRandomRedditPost(url, data.fetchValue("lastanimeIDs"));
            await data.initializeIterator("AnimeNumber", 1);
            await data.setValue("lastAnimeIDs", WebManager.AddNewPostID(data.fetchValue("lastanimeIDs"), WebManager.GeneratePostID(post)));

            var channel = client.GetChannel(1099741439476379730) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new() {
                Title = "Anime Image #" + data.fetchValue("AnimeNumber"),
                Description = post["title"] +
                            $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/awwnime/");

            await data.iterateValue("AnimeNumber");
            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, Task> QuestionOfTheDay_Automation = async (DiscordSocketClient client, Database data) => { // Question of the Day
            string url = "https://www.reddit.com/r/AskReddit/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url, false);
            await data.initializeIterator("QuestionNumber", 1);

            var channel = client.GetChannel(1141893287981088798) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new() {
                Title = $"Question of the Day #{data.fetchValue("QuestionNumber")}",
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}"
            };
            newEmbed.WithFooter("powered by https://www.reddit.com/r/AskReddit/");

            await data.iterateValue("QuestionNumber");

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, Task> AniMemesDaily_Automation = async (DiscordSocketClient client, Database data) => { // Animemes Daily API
            string url = "https://www.reddit.com/r/animemes/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url);
            await data.initializeIterator("AnimemesNumber", 1);

            var channel = client.GetChannel(1124438394903207956) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new() {
                Title = "Anime Meme #" + data.fetchValue("AnimemesNumber"),
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/animemes/");

            await data.iterateValue("AnimemesNumber");

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };
    }
}