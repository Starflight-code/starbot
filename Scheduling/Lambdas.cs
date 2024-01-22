using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace StarBot {
    internal class Lambdas {
        public static Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> XKCD_Automation = async (DiscordSocketClient client, Database data, ulong guildID, Caching.MemoryCacheManager cache) => { // XKCD Automation
            if (data.fetchValue("XKCD Channel", guildID) == "") { return; }

            string url = "https://xkcd.com/info.0.json";
            JObject json = WebManager.FetchJSON(url);
            var channel = client.GetChannel(ulong.Parse(data.fetchValue("XKCD Channel", guildID))) as SocketTextChannel;

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

        public static Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> CatDaily_Automation = async (DiscordSocketClient client, Database data, ulong guildID, Caching.MemoryCacheManager cache) => { // Cat Daily API
            if (data.fetchValue("Cat Channel", guildID) == "") { return; }

            string url = "https://www.reddit.com/r/cat/.json?limit=100&t=day";

            await data.initializeIterator("CatNumber", 1);

            var channel = client.GetChannel(ulong.Parse(data.fetchValue("Cat Channel", guildID))) as SocketTextChannel;

            JToken? post = WebManager.SelectRandomRedditPost(url, data.fetchValue("lastCatIDs", guildID), cache);

            await data.setValue("lastCatIDs", WebManager.AddNewPostID(data.fetchValue("lastCatIDs", guildID), Validation.GeneratePostID(post)), guildID);

            EmbedBuilder newEmbed = new() {
                Title = $"Daily Cat Image #{data.fetchValue("CatNumber", guildID)}",
                Description = $"{post["title"]}\n" +
                                   $"https://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/cats/");

            await data.iterateValue("CatNumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> AnimeDaily_Automation = async (DiscordSocketClient client, Database data, ulong guildID, Caching.MemoryCacheManager cache) => { // Anime Daily API
            if (data.fetchValue("Anime Channel", guildID) == "") { return; }

            string url = "https://www.reddit.com/r/awwnime/.json?limit=100&t=day";

            JToken? post = WebManager.SelectRandomRedditPost(url, data.fetchValue("lastanimeIDs", guildID), cache);
            await data.initializeIterator("AnimeNumber", 1);
            await data.setValue("lastAnimeIDs", WebManager.AddNewPostID(data.fetchValue("lastanimeIDs", guildID), Validation.GeneratePostID(post)), guildID);

            var channel = client.GetChannel(ulong.Parse(data.fetchValue("Anime Channel", guildID))) as SocketTextChannel;

            EmbedBuilder newEmbed = new() {
                Title = "Anime Image #" + data.fetchValue("AnimeNumber", guildID),
                Description = post["title"] +
                            $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/awwnime/");

            await data.iterateValue("AnimeNumber", guildID);
            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> QuestionOfTheDay_Automation = async (DiscordSocketClient client, Database data, ulong guildID, Caching.MemoryCacheManager cache) => { // Question of the Day
            if (data.fetchValue("QOTD Channel", guildID) == "") { return; }

            string url = "https://www.reddit.com/r/AskReddit/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url, data.fetchValue("lastQuestionOfTheDayIDs", guildID), cache, false);
            await data.initializeIterator("QuestionNumber", 1);
            await data.setValue("lastQuestionOfTheDayIDs", WebManager.AddNewPostID(data.fetchValue("lastQuestionOfTheDayIDs", guildID), Validation.GeneratePostID(post)), guildID);

            var channel = client.GetChannel(ulong.Parse(data.fetchValue("QOTD Channel", guildID))) as SocketTextChannel;

            EmbedBuilder newEmbed = new() {
                Title = $"Question of the Day #{data.fetchValue("QuestionNumber", guildID)}",
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}"
            };
            newEmbed.WithFooter("powered by https://www.reddit.com/r/AskReddit/");

            await data.iterateValue("QuestionNumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, Database, ulong, Caching.MemoryCacheManager, Task> AniMemesDaily_Automation = async (DiscordSocketClient client, Database data, ulong guildID, Caching.MemoryCacheManager cache) => { // Animemes Daily API
            if (data.fetchValue("AniMemes Channel", guildID) == "") { return; }

            string url = "https://www.reddit.com/r/animemes/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url, data.fetchValue("lastAnimemesIDs", guildID), cache);
            await data.initializeIterator("AnimemesNumber", 1);
            await data.setValue("lastAnimemesIDs", WebManager.AddNewPostID(data.fetchValue("lastAnimemesIDs", guildID), Validation.GeneratePostID(post)), guildID);

            var channel = client.GetChannel(ulong.Parse(data.fetchValue("AniMemes Channel", guildID))) as SocketTextChannel;

            EmbedBuilder newEmbed = new() {
                Title = "Anime Meme #" + data.fetchValue("AnimemesNumber", guildID),
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/animemes/");

            await data.iterateValue("AnimemesNumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };
    }
}