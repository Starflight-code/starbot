using Debug;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

// NOTICE: This file needs extensive refactoring

namespace StarBot {
    internal class Lambdas {
        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> XKCD_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // XKCD Automation
            if (await data.readFromDB<ulong>("xkcdchannel", guildID) == 0) { return; }

            string url = "https://xkcd.com/info.0.json";
            JObject json = WebManager.FetchJSON(url);
            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("xkcdchannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("xkcdchannel", 0, guildID);
                return;
            }

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
        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> CatDaily_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // Cat Daily API
            if (await data.readFromDB<ulong>("catchannel", guildID) == 0) { return; }

            string url = "https://www.reddit.com/r/cat/.json?limit=100&t=day";

            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("catchannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("Cat Channel", 0, guildID);
                return;
            }

            JToken? post = WebManager.SelectRandomRedditPost(url, await data.readFromDB<string>("lastcatids", guildID), cache, debug);

            data.writeToDB<string>("lastcatids", WebManager.AddNewPostID(await data.readFromDB<string>("lastcatids", guildID), Validation.GeneratePostID(post)), guildID);

            EmbedBuilder newEmbed = new() {
                Title = $"Daily Cat Image #{await data.readFromDB<int>("catnumber", guildID)}",
                Description = $"{post["title"]}\n" +
                                   $"https://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/cats/");
            //WebManager.sendPost()

            data.iterateValue("catnumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> DBD_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // Cat Daily API
            if (await data.readFromDB<ulong>("dbdchannel", guildID) == 0) { return; }

            string url = "https://www.reddit.com/r/deadbydaylight/.json?limit=100&t=day";

            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("dbdchannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("dbdchannel", 0, guildID);
                return;
            }
            Func<JToken, bool> validation = (JToken token) => {
                try {
                    var flairs = token["link_flair_richtext"];
                    foreach (JToken flair in flairs) {
                        if (flair["t"].ToString().Contains("Shitpost / Meme") && Validation.IsLinkToImage(token["url_overridden_by_dest"].ToString())) {
                            return true;
                        }
                    }
                    return false;
                } catch { // object may be null (for some reason, detected in error logs)
                    return false; // if there's an error, the post isn't valid
                }
            };


            JToken? post = WebManager.SelectRandomRedditPost(url, await data.readFromDB<string>("lastdbdids", guildID), cache, validation);

            data.writeToDB<string>("lastDBDIDs", WebManager.AddNewPostID(await data.readFromDB<string>("lastdbdids", guildID), Validation.GeneratePostID(post)), guildID);

            EmbedBuilder newEmbed = new() {
                Title = $"DBD Image #{await data.readFromDB<int>("dbdnumber", guildID)}",
                Description = $"{post["title"]}\n" +
                                   $"https://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/deadbydaylight/");

            data.iterateValue("dbdnumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> AnimeDaily_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // Anime Daily API
            if (await data.readFromDB<ulong>("animechannel", guildID) == 0) { return; }

            string url = "https://www.reddit.com/r/awwnime/.json?limit=100&t=day";

            JToken? post = WebManager.SelectRandomRedditPost(url, await data.readFromDB<string>("lastanimeids", guildID), cache, debug);
            data.writeToDB<string>("lastanimeids", WebManager.AddNewPostID(await data.readFromDB<string>("lastanimeids", guildID), Validation.GeneratePostID(post)), guildID);

            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("animechannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("animechannel", 0, guildID);
                return;
            }

            EmbedBuilder newEmbed = new() {
                Title = "Anime Image #" + await data.readFromDB<int>("animenumber", guildID),
                Description = post["title"] +
                            $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/awwnime/");

            data.iterateValue("animenumber", guildID);
            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> QuestionOfTheDay_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // Question of the Day
            if (await data.readFromDB<ulong>("qotdchannel", guildID) == 0) { return; }

            string url = "https://www.reddit.com/r/AskReddit/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url, await data.readFromDB<string>("lastqotdids", guildID), cache, debug, false);

            data.writeToDB<string>("lastqotdids", WebManager.AddNewPostID(await data.readFromDB<string>("lastqotdids", guildID), Validation.GeneratePostID(post)), guildID);

            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("qotdchannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("qotdchannel", 0, guildID);
                return;
            }

            EmbedBuilder newEmbed = new() {
                Title = $"Question of the Day #{await data.readFromDB<int>("qotdnumber", guildID)}",
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}"
            };
            newEmbed.WithFooter("powered by https://www.reddit.com/r/AskReddit/");

            data.iterateValue("qotdnumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };

        public static Func<DiscordSocketClient, SqlDatabase, ulong, Caching.MemoryCacheManager, DebugComms, Task> AniMemesDaily_Automation = async (DiscordSocketClient client, SqlDatabase data, ulong guildID, Caching.MemoryCacheManager cache, DebugComms debug) => { // Animemes Daily API
            if (await data.readFromDB<ulong>("animemeschannel", guildID) == 0) { return; }

            string url = "https://www.reddit.com/r/animemes/.json?limit=100&t=day";
            JToken? post = WebManager.SelectRandomRedditPost(url, await data.readFromDB<string>("lastanimemesids", guildID), cache, debug);

            data.writeToDB<string>("lastanimemesids", WebManager.AddNewPostID(await data.readFromDB<string>("lastanimemesids", guildID), Validation.GeneratePostID(post)), guildID);

            SocketTextChannel? channel = client.GetChannel(await data.readFromDB<ulong>("animemeschannel", guildID)) as SocketTextChannel;

            if (channel == null) {
                data.writeToDB<ulong>("animemeschannel", 0, guildID);
                return;
            }

            EmbedBuilder newEmbed = new() {
                Title = "Anime Meme #" + await data.readFromDB<int>("animemesnumber", guildID),
                Description = post["title"] +
                $"\nhttps://reddit.com{post["permalink"]}",
                ImageUrl = post["url_overridden_by_dest"].ToString()
            };
            newEmbed.WithFooter("powered by https://reddit.com/r/animemes/");

            data.iterateValue("animemesnumber", guildID);

            if (!Config.DEBUG_MODE) {
                await channel.SendMessageAsync("", false, newEmbed.Build());
            }
        };
    }
}