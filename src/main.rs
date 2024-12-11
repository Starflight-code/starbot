pub mod api;
pub mod database;
pub mod discord;
pub mod memcache;
mod models;
pub mod monitor;
mod scheduler;
pub mod scheduler_data;
mod schema;
pub mod settings;

use std::env;
use std::process::exit;
use std::time::Duration;

use ::serenity::all::{
    ComponentInteractionDataKind, CreateEmbed, CreateMessage, CreateSelectMenu,
    CreateSelectMenuKind, CreateSelectMenuOption, GuildId,
};
use chrono::{TimeDelta, Utc};
use diesel::prelude::*;
use diesel_migrations::{embed_migrations, EmbeddedMigrations, MigrationHarness};
use lazy_static::lazy_static;
use memcache::Memcache;
use models::{Automation, Guild, NewAutomation, NewGuild, NewScheduled, Scheduled};
use poise::{serenity_prelude as serenity, CreateReply};
use scheduler::run_automation;
use scheduler_data::{AutomationType, ScheduledAutomation};
use serde_json::json;
use serenity::async_trait;
use serenity::model::channel::Message;
use serenity::prelude::*;

pub const MIGRATIONS: EmbeddedMigrations = embed_migrations!("migrations/");
struct Handler {}

lazy_static! {
    static ref GLOBAL_MEMCACHE: Mutex<Memcache> = Mutex::new(Memcache::new());
}

struct Data {} // User data, which is stored and accessible in all command invocations
type Error = Box<dyn std::error::Error + Send + Sync>;
type Context<'a> = poise::Context<'a, Data, Error>;

#[async_trait]
impl EventHandler for Handler {
    async fn message(&self, ctx: serenity::prelude::Context, msg: Message) {
        if msg.author.bot {
            return;
        }

        use crate::schema::guild::dsl as guild_dsl;
        let mut connection = database::establish_connection().await;

        let channel = msg.channel(&ctx.http).await.unwrap();
        if channel.private().is_some() {
            let mut guilds = Vec::new();
            for guild in ctx.cache.guilds().iter() {
                if guild.member(&ctx.http, msg.author.id).await.is_ok() {
                    guilds.push(*guild);
                }
            }
            //msg.reply(&ctx.http, "You're accessing some functionality that isn't quite ready yet. It's currently disabled, please try again later!").await.unwrap();
            //return;

            let mut guild_menus = Vec::new();
            let mut guild_info = Vec::new();
            let mut iter = 0;
            for guild in guilds {
                iter += 1;
                let mut name = ctx.http.get_guild(guild).await.unwrap().name;
                guild_info.push((
                    format!("{}: {}", iter, name),
                    String::from("Create a new ticket."),
                    true,
                ));
                if name.len() > 24 {
                    name.truncate(22);
                    name.push_str("..");
                }
                guild_menus.push(CreateSelectMenuOption::new(name, format!("{}", guild)));
            }

            let m = msg
                .channel_id
                .send_message(
                    &ctx,
                    CreateMessage::new()
                        .embed(CreateEmbed::new().title("Your Guilds").fields(guild_info))
                        .content("Please select your favorite animal")
                        .select_menu(
                            CreateSelectMenu::new(
                                "guild_select",
                                CreateSelectMenuKind::String {
                                    options: guild_menus,
                                },
                            )
                            .custom_id("guild_select")
                            .placeholder("No animal selected"),
                        ),
                )
                .await
                .unwrap();

            let interaction = match m
                .await_component_interaction(&ctx.shard)
                .timeout(Duration::from_secs(60 * 5))
                .await
            {
                Some(x) => x,
                None => {
                    m.delete(&ctx).await.unwrap_or_default();
                    m.reply(
                        &ctx,
                        "Interaction Timed Out. This application will no longer respond to the menu provided above.",
                    )
                    .await
                    .unwrap();
                    return;
                }
            };

            let guildid = match &interaction.data.kind {
                ComponentInteractionDataKind::StringSelect { values } => &values[0],
                _ => panic!("unexpected interaction data kind"),
            };

            let guild: Result<Guild, _> = guild_dsl::guild
                .filter(guild_dsl::guild_id.eq(guildid))
                .first(&mut connection);

            if guild.is_err() {
                msg.reply(&ctx.http, "An internal error has occured: GUILD_DOES_NOT_EXIST. Please contact my developer.").await.unwrap();
            }

            let guild = guild.unwrap();
            ctx.http
                .create_channel(
                    GuildId::new(guild.guild_id.parse().unwrap()),
                    &json!({
                    "name": "Test Channel",
                    "type": 0,
                    "parent_id": 1307187771542343771 as u64,
                    "permission_overwrites": [
                        {
                            "id": 233056950361915392 as u64,
                            "type": 1, // member
                            "allow": "68608", // allow see history, messages, send messages
                            "deny": "0"
                        }
                    ]
                    }),
                    None,
                )
                .await
                .unwrap();
            return;
        }

        if msg.content == "!ping" {
            if let Err(why) = msg.channel_id.say(&ctx.http, "Pong!").await {
                println!("Error sending message: {why:?}");
            }
        } else if msg.content == "!setup" {
            let guild = NewGuild {
                guild_id: &msg.guild_id.unwrap().get().to_string(),
                automations: "[]",
                metadata: "[]",
            };

            diesel::insert_into(guild_dsl::guild)
                .values(guild)
                .execute(&mut connection)
                .unwrap();

            if let Err(why) = msg.channel_id.say(&ctx.http, "Guild is now set up.").await {
                println!("Error sending message: {why:?}");
            }
        }
    }
}

#[poise::command(slash_command, required_permissions = "ADMINISTRATOR")]
async fn execute_task(ctx: Context<'_>) -> Result<(), Error> {
    ctx.defer_ephemeral().await?;

    use crate::schema::guild::dsl as guild_dsl;
    use crate::schema::scheduled::dsl as scheduled_dsl;
    let mut connection = database::establish_connection().await;

    //let connection = database::create_connection().await;
    let guildid = ctx
        .guild_id()
        .expect("Can only be executed in a guild")
        .get()
        .to_string();

    if (guild_dsl::guild
        .filter(guild_dsl::guild_id.eq(guildid))
        .first(&mut connection) as Result<Guild, _>)
        .is_err()
    {
        return Err("Initialize this guild first with !setup".into());
    }
    let channelid = ctx.channel_id().get().to_string();
    let db_automation: Option<Scheduled> = scheduled_dsl::scheduled
        .filter(scheduled_dsl::channel_id.eq(channelid))
        .first(&mut connection)
        .optional()
        .unwrap();
    let name;

    match db_automation {
        Some(automation) => {
            let mut scheduled = ScheduledAutomation::from_db(automation, &mut connection);
            name = scheduled.db_name.clone();
            run_automation(&ctx.serenity_context().http, &mut scheduled).await
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
    use crate::schema::automation::dsl as auto_dsl;
    use crate::schema::guild::dsl as guild_dsl;
    let mut connection = database::establish_connection().await;

    let guildid = ctx
        .guild_id()
        .expect("Can only be executed in a guild")
        .get()
        .to_string();

    if (guild_dsl::guild
        .filter(guild_dsl::guild_id.eq(guildid))
        .first(&mut connection) as Result<Guild, _>)
        .is_err()
    {
        return Err("Initialize this guild first with !setup".into());
    }

    let automation = automation.to_lowercase();

    let auto = NewAutomation {
        name: &automation,
        handler: handler.value(),
        metadata: "[]",
        has_image,
    };

    diesel::insert_into(auto_dsl::automation)
        .values(auto)
        .execute(&mut connection)
        .unwrap();

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
    use crate::schema::automation::dsl as auto_dsl;
    use crate::schema::guild::dsl as guild_dsl;
    use crate::schema::scheduled::dsl as scheduled_dsl;
    let mut connection = database::establish_connection().await;

    let guildid = ctx
        .guild_id()
        .expect("Can only be executed in a guild")
        .get()
        .to_string();

    let guild: Result<Guild, diesel::result::Error> = guild_dsl::guild
        .filter(guild_dsl::guild_id.eq(guildid))
        .first(&mut connection);

    if guild.is_err() {
        return Err("Initialize this guild first with !setup".into());
    }
    let guild = guild.unwrap();

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

    let auto: Automation = auto_dsl::automation
        .filter(auto_dsl::name.eq(&automation))
        .first(&mut connection)
        .unwrap();

    let schedule = NewScheduled {
        channel_id: &ctx.channel_id().get().to_string(),
        automation_id: auto.id,
        post_id_history: "[]",
        iterator: 1,
        cron: &cron,
        display_name: &name,
        settings: "[]",
        guild_id: guild.id,
    };

    diesel::insert_into(scheduled_dsl::scheduled)
        .values(schedule)
        .execute(&mut connection)
        .unwrap();

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
    let mut connection = database::establish_connection().await;

    connection
        .run_pending_migrations(MIGRATIONS)
        .expect("Error applying Diesel-rs SQLite migrations");

    let mut args: Vec<String> = env::args().collect();
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

    let mut client: Client = Client::builder(&token, intents)
        .event_handler(Handler {})
        .framework(framework)
        .await
        .expect("Err creating client");

    let scheduler_client = Client::builder(&token, intents)
        .await
        .expect("Err creating client");

    tokio::spawn(async {
        _ = scheduler::scheduler(scheduler_client).await;
        tokio::task::yield_now().await
    });
    if let Err(why) = client.start().await {
        println!("Client error: {why:?}");
    }
}
