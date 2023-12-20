using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace StarBot {
    internal class Lambdas {
        public static Func<DiscordSocketClient, Database, Task> XKCD_Automation = (async (DiscordSocketClient client, Database data) => { // XKCD Automation

            string url = "https://xkcd.com/info.0.json";
            JObject json = Program.fetchJSON(url);
            var channel = client.GetChannel(1106385489180758056) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new EmbedBuilder();
            newEmbed.Title = json["safe_title"] + " - " + json["num"];
            newEmbed.Description = json["alt"].ToString();
            newEmbed.ImageUrl = json["img"].ToString();
            newEmbed.WithFooter("powered by xkcd.com");

            await channel.SendMessageAsync("", false, newEmbed.Build());
        });

        public static Func<DiscordSocketClient, Database, Task> CatDaily_Automation = (async (DiscordSocketClient client, Database data) => { // Cat Daily API
            string url = "https://www.reddit.com/r/cat/.json?limit=100&t=day";
            JObject json = Program.fetchJSON(url);
            Random rand = new Random();
            int i = 0;
            int randomValue;
            while (true) {
                i++;
                randomValue = rand.Next(100); // 0-99
                try {
                    if (json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString().EndsWith("jpg")) {
                        break;
                    }
                } catch (System.NullReferenceException) {
                    Console.WriteLine("Null Pointer Exception in AniMemesDaily Lambda");
                }
                if (i >= 150) {
                    json = Program.fetchJSON(url);
                }
            }
            await data.initializeIterator("CatNumber", 1);

            var channel = client.GetChannel(1106385469312352288) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new EmbedBuilder();
            newEmbed.Title = "Daily Cat Image #" + (data.fetchValue("CatNumber"));
            newEmbed.Description = json["data"]["children"][randomValue]["data"]["title"] + "\nhttps://reddit.com" +
                 json["data"]["children"][randomValue]["data"]["permalink"];
            newEmbed.ImageUrl = json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString();
            newEmbed.WithFooter("powered by https://reddit.com/r/cats/");

            await data.iterateValue("CatNumber");

            await channel.SendMessageAsync("", false, newEmbed.Build());
        });

        public static Func<DiscordSocketClient, Database, Task> AnimeDaily_Automation = (async (DiscordSocketClient client, Database data) => { // Anime Daily API
            string url = "https://www.reddit.com/r/awwnime/.json?limit=100&t=day";
            JObject json = Program.fetchJSON(url);
            Random rand = new Random();
            int i = 0;
            int randomValue;
            var validPost = (int randomValue, JObject json, Database data) => {
                bool condition1 = json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString().EndsWith("jpg") || json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString().EndsWith("jpeg");
                bool condition2 = data.fetchValue("lastanimeID") != json["data"]["children"][randomValue]["data"]["subreddit_id"].ToString() + "-" + json["data"]["children"][randomValue]["data"]["id"].ToString();
                return condition1 && condition2;
            };
            while (true) {
                i++;
                randomValue = rand.Next(100); // 0-99
                try {
                    if (json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString().EndsWith("jpg")) {
                        break;
                    }
                } catch (System.NullReferenceException) {
                    Console.WriteLine("Null Pointer Exception in AniMemesDaily Lambda");
                }
                if (i >= 150) {
                    json = Program.fetchJSON(url);
                }
            }
            await data.initializeIterator("AnimeNumber", 1);
            await data.setValue("lastAnimeID", json["data"]["children"][randomValue]["data"]["subreddit_id"].ToString() + "-" + json["data"]["children"][randomValue]["data"]["id"].ToString());

            var channel = client.GetChannel(1099741439476379730) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new EmbedBuilder();
            newEmbed.Title = "Anime Image #" + (data.fetchValue("AnimeNumber"));
            newEmbed.Description = json["data"]["children"][randomValue]["data"]["title"] + "\nhttps://reddit.com" +
                 json["data"]["children"][randomValue]["data"]["permalink"];
            newEmbed.ImageUrl = json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString();
            newEmbed.WithFooter("powered by https://reddit.com/r/awwnime/");

            await data.iterateValue("AnimeNumber");

            await channel.SendMessageAsync("", false, newEmbed.Build());
        });

        public static Func<DiscordSocketClient, Database, Task> QuestionOfTheDay_Automation = (async (DiscordSocketClient client, Database data) => { // Question of the Day
            string url = "https://www.reddit.com/r/AskReddit/.json?limit=100&t=day";
            JObject json = await Program.fetchJSON(url);
            Random rand = new Random();
            int randomValue = rand.Next(100); // 0-99
            await data.initializeIterator("QuestionNumber", 1);

            var channel = client.GetChannel(1141893287981088798) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new EmbedBuilder();
            newEmbed.Title = "Question of the Day #" + (data.fetchValue("QuestionNumber"));
            newEmbed.Description = json["data"]["children"][randomValue]["data"]["title"] + "\nhttps://reddit.com" +
                 json["data"]["children"][randomValue]["data"]["permalink"];
            newEmbed.WithFooter("powered by https://www.reddit.com/r/AskReddit/");

            await data.iterateValue("QuestionNumber");

            await channel.SendMessageAsync("", false, newEmbed.Build());
        });

        public static Func<DiscordSocketClient, Database, Task> AniMemesDaily_Automation = (async (DiscordSocketClient client, Database data) => { // Animemes Daily API
            string url = "https://www.reddit.com/r/animemes/.json?limit=100&t=day";
            JObject json = Program.fetchJSON(url);
            Random rand = new Random();
            int i = 0;
            int randomValue;
            while (true) {
                i++;
                randomValue = rand.Next(100); // 0-99
                try {
                    if (json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString().EndsWith("jpg")) {
                        break;
                    }
                } catch (System.NullReferenceException) {
                    Console.WriteLine("Null Pointer Exception in AniMemesDaily Lambda");
                }
                if (i >= 150) {
                    json = Program.fetchJSON(url);
                }
            }
            await data.initializeIterator("AnimemesNumber", 1);

            var channel = client.GetChannel(1124438394903207956) as SocketTextChannel;
            var state = client.ConnectionState;

            EmbedBuilder newEmbed = new EmbedBuilder();
            newEmbed.Title = "Anime Meme #" + (data.fetchValue("AnimemesNumber"));
            newEmbed.Description = json["data"]["children"][randomValue]["data"]["title"] + "\nhttps://reddit.com" +
                 json["data"]["children"][randomValue]["data"]["permalink"];
            newEmbed.ImageUrl = json["data"]["children"][randomValue]["data"]["url_overridden_by_dest"].ToString();
            newEmbed.WithFooter("powered by https://reddit.com/r/animemes/");

            await data.iterateValue("AnimemesNumber");

            await channel.SendMessageAsync("", false, newEmbed.Build());
        });
    }
}