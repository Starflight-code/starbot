pub mod api;
pub mod database;
pub mod discord;
pub mod memcache;
mod scheduler;
pub mod scheduler_data;
pub mod settings;

use std::env;
use std::process::exit;

use chrono::{TimeDelta, Utc};
use memcache::Memcache;
use poise::{serenity_prelude as serenity, CreateReply};
use scheduler::run_automation;
use scheduler_data::AutomationType;
use serenity::async_trait;
use serenity::model::channel::Message;
use serenity::prelude::*;
struct Handler {}

struct Data {} // User data, which is stored and accessible in all command invocations
type Error = Box<dyn std::error::Error + Send + Sync>;
type Context<'a> = poise::Context<'a, Data, Error>;

#[async_trait]
impl EventHandler for Handler {
    async fn message(&self, ctx: serenity::prelude::Context, msg: Message) {
        if msg.content == "!ping" {
            if let Err(why) = msg.channel_id.say(&ctx.http, "Pong!").await {
                println!("Error sending message: {why:?}");
            }
        } else if msg.content == "!setup" {
            let connection = database::create_connection().await;
            database::add_guild(&connection, &msg.guild_id.unwrap().get());
            if let Err(why) = msg.channel_id.say(&ctx.http, "Guild is now set up.").await {
                println!("Error sending message: {why:?}");
            }
        }
    }
}

#[poise::command(slash_command, required_permissions = "ADMINISTRATOR")]
async fn execute_task(ctx: Context<'_>) -> Result<(), Error> {
    ctx.defer_ephemeral().await?;
    let connection = database::create_connection().await;
    if !database::does_guild_exist(
        &connection,
        &ctx.guild_id()
            .expect("Can only be executed in a guild")
            .get(),
    ) {
        return Err("Initialize this guild first with !setup".into());
    }
    let automation = database::get_automation(&connection, &ctx.channel_id().get());
    let name;
    match automation {
        Some(_) => {
            let mut automation = automation.unwrap();
            name = automation.db_name.clone();
            run_automation(&ctx.serenity_context().http, &mut automation).await
        }
        None => return Err("That automation does not exist".into()),
    }
    ctx.send(
        CreateReply::default()
            .content(format!("Automation {} executed successfully!", name))
            .ephemeral(true),
    )
    .await?;
    Ok(())
}
#[poise::command(slash_command, required_permissions = "ADMINISTRATOR")]
async fn create_automation(
    ctx: Context<'_>,
    #[description = "Name/Subreddit"] automation: String,
    #[description = "Automation Handler"] handler: AutomationType,
    #[description = "Has Image"] has_image: bool,
) -> Result<(), Error> {
    let connection = database::create_connection().await;
    if !database::does_guild_exist(
        &connection,
        &ctx.guild_id()
            .expect("Can only be executed in a guild")
            .get(),
    ) {
        return Err("Initialize this guild first with !setup".into());
    }
    database::add_automation(&connection, &automation, handler, &has_image);
    ctx.send(
        CreateReply::default()
            .content(format!("Automation {} created!", automation))
            .ephemeral(true),
    )
    .await?;
    Ok(())
}

#[poise::command(slash_command)]
async fn add_schedule(
    ctx: Context<'_>,
    #[description = "Name/Subreddit"] mut automation: String,
    #[description = "Cron Expression"] cron: String,
    #[description = "Display Name"] name: String,
) -> Result<(), Error> {
    let connection = database::create_connection().await;
    if !database::does_guild_exist(
        &connection,
        &ctx.guild_id()
            .expect("Can only be executed in a guild")
            .get(),
    ) {
        return Err("Initialize this guild first with !setup".into());
    }
    let is_cron_valid = croner::Cron::new(&cron).parse();
    if is_cron_valid.is_err() {
        return Err("The Cron expression you gave is invalid.".into());
    }
    let limit = 5; // 5 runs per day allowed
    let cron_iter = is_cron_valid.unwrap().iter_from(Utc::now()).take(limit + 1);
    if cron_iter.last().unwrap() < Utc::now() + TimeDelta::days(1) {
        return Err("The provided cron expression exceeds our limits.".into());
    }
    automation = automation.to_lowercase();

    let result = database::add_scheduled(
        &connection,
        &ctx.guild_id().unwrap().get(),
        &ctx.channel_id().get(),
        &automation,
        &cron,
        &name,
    );
    if result.is_err() {
        return result;
    }
    ctx.send(
        CreateReply::default()
            .content(format!(
                "Automation {} added to this channel, it will be scheduled after the current queue is reset.",
                automation
            ))
            .ephemeral(true),
    )
    .await?;
    Ok(())
}

#[tokio::main]
async fn main() {
    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        println!("A token should be passed in through command line arguments.\nEx: ./StarBot <Bot-Token>");
        exit(1);
    }
    let token = &args[1];
    let intents = GatewayIntents::GUILD_MESSAGES
        | GatewayIntents::DIRECT_MESSAGES
        | GatewayIntents::MESSAGE_CONTENT;

    let framework = poise::Framework::builder()
        .options(poise::FrameworkOptions {
            commands: vec![create_automation(), add_schedule(), execute_task()],
            ..Default::default()
        })
        .setup(|ctx, _ready, framework| {
            Box::pin(async move {
                poise::builtins::register_globally(ctx, &framework.options().commands).await?;
                Ok(Data {})
            })
        })
        .build();

    let mut client = Client::builder(&token, intents)
        .event_handler(Handler {})
        .framework(framework)
        .await
        .expect("Err creating client");

    let scheduler_client = Client::builder(&token, intents)
        .await
        .expect("Err creating client");
    tokio::spawn(async {
        let mut memcache = Memcache::new();
        _ = scheduler::scheduler(scheduler_client, &mut memcache).await;
        tokio::task::yield_now().await
    });
    if let Err(why) = client.start().await {
        println!("Client error: {why:?}");
    }
}
