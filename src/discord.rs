use serenity::all::{ChannelId, CreateEmbed, CreateEmbedFooter, CreateMessage, Http};

use crate::api::Post;

pub async fn send_embed(cache: &Http, post: Post, channel: &ChannelId) {
    let mut embed = CreateEmbed::new()
        .title(format!("{}", post.title))
        .description(format!("{}", post.body))
        .footer(CreateEmbedFooter::new(format!(
            "Powered by {}",
            post.powered_by
        )));
    if post.image_link.is_some() {
        embed = embed.clone().image(post.image_link.unwrap());
    }
    if post.url.is_some() {
        embed = embed.clone().url(post.url.unwrap());
    }

    channel
        .send_message(cache, CreateMessage::new().embed(embed))
        .await
        .unwrap();
}
