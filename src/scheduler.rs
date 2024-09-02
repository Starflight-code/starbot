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

/**
    Finds the automations next up for execution. Returns an array of automations to execute 
    at the same time, the time to execute them (unix seconds), and a set of the automation names.
 */
pub fn generate_timeline(
    scheduled: &Vec<ScheduledAutomation>,
) -> (Vec<usize>, DateTime<Local>, HashSet<String>) {
    let mut execute_next: Vec<usize> = Vec::new(); // indexes of scheduled to run next (all scheduled to run at the same time)
    let mut earliest_run: i64 = i64::MAX; // earliest scheduled unix timestamp
    let mut automations: HashSet<String> = HashSet::new(); // automation names for UI printout

    for automation in scheduled { // find earliest time
        if automation.next_up().timestamp() < earliest_run {
            earliest_run = automation.next_up().timestamp();
        }
    }

    for index in 0..scheduled.len() { // find automations with next scheduled equal to earliest time
        if scheduled[index].next_up().timestamp() == earliest_run {
            automations.insert(scheduled[index].db_name.clone());
            execute_next.push(index);
        }
    }
    let return_time = scheduled[execute_next[0]].next_up();
    return (execute_next, return_time, automations);
}

/**
    Generates a ", " deliminated string of `scheduled` automations.
    ```
    let mut scheduled = HashSet::new();
    scheduled.insert("String1");
    scheduled.insert("String2");
    scheduled.insert("String3");

    assert_eq!(display_next_up(&scheduled), String::from("String1, String2, String3"))
    ```
    This may not end up in the order of String1, String2, String3 (HashSets are an unordered data structure)
 */
pub fn display_next_up(scheduled: &HashSet<String>) -> String {
    let mut names = Vec::new();
    for automation in scheduled {
        names.push(automation.clone());
    }
    names.join(", ")
}


#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn display_next_up_test() {
        let mut scheduled = HashSet::new();
        scheduled.insert("V1".to_string());
        scheduled.insert("V2".to_string());
        scheduled.insert("V3".to_string());

        let result = display_next_up(&scheduled);

        assert!(result.contains("V1"));
        assert!(result.contains("V2"));
        assert!(result.contains("V3"));
        assert!(result.split(", ").count() == 3);
    }
}


/** 
    Requires an authenticated `client` and instanciated `memcache` object. Creates an internal connection
    to a SQLite Database and loads automations from it. Finds next automations scheduled for execution, and 
    waits for their execution time. It creates web requests if a non-expired instance of the web request, response
    is not available in `memcache` and sends messages to channels linked with automation instances with the
    authenticated `client` object.
*/
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
        let time_to_wait: u64 = ((next_up.1 - Local::now()).num_seconds() + 1)
            .try_into()
            .unwrap(); // adds an extra second to prevent multiple messages per cycle (Rust is too fast)
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

/**
    Runs an automation using a `cache` from an authenticated client object and an `automation` object. Dispatches
    the automation handler based off handler metadata within the `automation` object.
 */
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
