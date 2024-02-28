using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using StarBot;
using StarBot.Scheduling;
internal class Handlers {
    public static Func<HandlerArgs, Task<Post>> xkcdHandler = async (HandlerArgs runtimeData) => {
        await Task.Delay(0);
        return new Post();
    };
    public static Func<HandlerArgs, Task<Post>> redditHandler = async (HandlerArgs runtimeData) => {
        //if (runtimeData.data.fetchValue(runtimeData.channelKey, runtimeData.guildID) == "") { return false; }

        //await runtimeData.data.initializeIterator(runtimeData.iteratorKey, runtimeData.guildID, 1);

        //var channel = runtimeData.client.GetChannel(ulong.Parse(runtimeData.data.fetchValue(runtimeData.channelKey, runtimeData.guildID))) as SocketTextChannel;

        /*if (channel == null) {
            await runtimeData.data.setValue(runtimeData.channelKey, "", runtimeData.guildID, true);
            return false;
        }*/
        JToken? json = WebManager.SelectRandomRedditPost(runtimeData.resourceLocation, runtimeData.data.fetchValue(runtimeData.cacheKey, runtimeData.guildID), runtimeData.cache);

        await runtimeData.data.setValue(runtimeData.cacheKey, WebManager.AddNewPostID(runtimeData.data.fetchValue(runtimeData.cacheKey, runtimeData.guildID), Validation.GeneratePostID(json)), runtimeData.guildID);
        Post post = new Post(json["title"].ToString(), json["selftext"].ToString(), json["url_overridden_by_dest"].ToString(), $"https://reddit.com{json["permalink"]}");
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
        dbIncrement(runtimeData);
        sendRedditPostWithIterator(runtimeData, post);
        return post;
    };

    public static Func<HandlerArgs, Task> dbIncrement = async (HandlerArgs runtimeData) => {
        await runtimeData.data.initializeIterator(runtimeData.iteratorKey, runtimeData.guildID);
        await runtimeData.data.iterateValue(runtimeData.iteratorKey, runtimeData.guildID);
    };

    public static Func<HandlerArgs, Post, Task> sendRedditPostWithIterator = async (HandlerArgs runtimeData, Post post) => {
        EmbedBuilder newEmbed = new() {
            Title = $"Daily {runtimeData.baseName} Image #{runtimeData.data.fetchValue(runtimeData.iteratorKey, runtimeData.guildID)}",
            Description = post.title + "\n" +
                               post.linkToPost,
            ImageUrl = post.multimediaURL
        };
        //newEmbed.WithFooter("powered by https://reddit.com/r/cats/");*/
        ulong channelID = ulong.Parse(runtimeData.data.fetchValue(runtimeData.channelKey, runtimeData.guildID));
        SocketTextChannel? channel = runtimeData.client.GetChannel(channelID) as SocketTextChannel;
        if (!Config.DEBUG_MODE) {
            await channel.SendMessageAsync("", false, newEmbed.Build());
        }
        
    };
}