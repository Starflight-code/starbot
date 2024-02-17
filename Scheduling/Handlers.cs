using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using StarBot;
using StarBot.Scheduling;
internal class Handlers {
    public static Func<HandlerArgs, Task<Post>> redditHandler = async (HandlerArgs runtimeData) => {
        //if (runtimeData.data.fetchValue(runtimeData.channelKey, runtimeData.guildID) == "") { return false; }

        //await runtimeData.data.initializeIterator(runtimeData.iteratorKey, runtimeData.guildID, 1);

        //var channel = runtimeData.client.GetChannel(ulong.Parse(runtimeData.data.fetchValue(runtimeData.channelKey, runtimeData.guildID))) as SocketTextChannel;

        /*if (channel == null) {
            await runtimeData.data.setValue(runtimeData.channelKey, "", runtimeData.guildID, true);
            return false;
        }*/

        JToken? post = WebManager.SelectRandomRedditPost(runtimeData.resourceLocation, runtimeData.data.fetchValue(runtimeData.cacheKey, runtimeData.guildID), runtimeData.cache);

        await runtimeData.data.setValue(runtimeData.cacheKey, WebManager.AddNewPostID(runtimeData.data.fetchValue(runtimeData.cacheKey, runtimeData.guildID), Validation.GeneratePostID(post)), runtimeData.guildID);
        return new Post(post["title"].ToString(), post["selftext"].ToString(), post["url_overridden_by_dest"].ToString(), $"https://reddit.com{post["permalink"]}");
        /*EmbedBuilder newEmbed = new() {
            Title = $"Daily Cat Image #{runtimeData.data.fetchValue("CatNumber", runtimeData.guildID)}",
            Description = $"{post["title"]}\n" +
                               $"https://reddit.com{post["permalink"]}",
            ImageUrl = post["url_overridden_by_dest"].ToString()
        };
        newEmbed.WithFooter("powered by https://reddit.com/r/cats/");*/

        //await runtimeData.data.iterateValue("CatNumber", runtimeData.guildID);

        /*if (!Config.DEBUG_MODE) {
            await channel.SendMessageAsync("", false, newEmbed.Build());
        }*/
        //throw new NotImplementedException();
    };
}