use crate::api;
use crate::database;
use crate::discord;
use crate::memcache::Memcache;
use crate::scheduler_data::{AutomationType, ScheduledAutomation};

use chrono::DateTime;
use serenity::all::{ChannelId, Http};
use std::collections::HashSet;
use std::time::Duration;
use tokio::time::sleep;

use chrono::Local;

pub fn generate_timeline(
    scheduled: &Vec<ScheduledAutomation>,
) -> (Vec<usize>, DateTime<Local>, HashSet<String>) {
    let mut execute_next: Vec<usize> = Vec::new();
    let mut earliest_run: i64 = i64::MAX;
    let mut automations: HashSet<String> = HashSet::new();
    for automation in scheduled {
        if automation.next_up().timestamp() < earliest_run {
            earliest_run = automation.next_up().timestamp();
        }
    }
    for index in 0..scheduled.len() {
        if scheduled[index].next_up().timestamp() == earliest_run {
            automations.insert(scheduled[index].db_name.clone());
            execute_next.push(index);
        }
    }
    let return_time = scheduled[execute_next[0]].next_up();
    return (execute_next, return_time, automations);
}

pub fn display_next_up(scheduled: &HashSet<String>) -> String {
    let mut names = Vec::new();
    for automation in scheduled {
        names.push(automation.clone());
    }
    names.join(", ")
}

pub async fn scheduler(client: serenity::Client, memcache: &mut Memcache) {
    let connection = database::create_connection().await;
    let mut no_automations = false;
    loop {
        let mut automations = database::get_automations(&connection);
        if automations.len() == 0 {
            if !no_automations {
                println!("No automations exist, re-checking every minute...");
            }
            no_automations = true;
            sleep(Duration::from_secs(60)).await;
            continue;
        }
        let next_up = generate_timeline(&automations);
        let time_to_wait: u64 = (next_up.1 - Local::now()).num_seconds().try_into().unwrap();
        println!(
            "{}: Waiting until {} for execution of {}. Waiting: {} seconds...",
            Local::now().format("%m/%d/%Y %H:%M"),
            next_up.1.format("%m/%d/%Y %H:%M"),
            display_next_up(&next_up.2),
            time_to_wait
        );

        sleep(Duration::from_secs(time_to_wait)).await;
        for i in next_up.0 {
            match automations[i].handler {
                AutomationType::Reddit => {
                    let response = api::reddit_handler(&mut automations[i], memcache).await;
                    discord::send_embed(
                        &client.http,
                        response,
                        &ChannelId::new(automations[i].channelid),
                    )
                    .await;
                }
                AutomationType::XKCD => {
                    let response = api::xkcd_handler().await;
                    discord::send_embed(
                        &client.http,
                        response,
                        &ChannelId::new(automations[i].channelid),
                    )
                    .await
                }
            }
            automations[i].update_db(&connection);
        }
    }
}

pub async fn run_automation(cache: &Http, automation: &mut ScheduledAutomation) {
    let connection = database::create_connection().await;
    match automation.handler {
        AutomationType::Reddit => {
            let response = api::reddit_handler(automation, &mut Memcache::new()).await;
            discord::send_embed(cache, response, &ChannelId::new(automation.channelid)).await;
        }
        AutomationType::XKCD => {
            let response = api::xkcd_handler().await;
            discord::send_embed(cache, response, &ChannelId::new(automation.channelid)).await
        }
    }
    automation.update_db(&connection);
}
