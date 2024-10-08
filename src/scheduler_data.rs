use chrono::{DateTime, Local};
use rusqlite::Connection;

use crate::database::{self, Automation};
use crate::settings::_DUPLICATE_MAX_ARRAY_SIZE;

#[derive(Debug, poise::ChoiceParameter)]
pub enum AutomationType {
    Reddit = 1,
    XKCD = 2,
}

impl AutomationType {
    pub fn value(self) -> u32 {
        match self {
            AutomationType::Reddit => return 1,
            AutomationType::XKCD => return 2,
        }
    }

    pub fn from_db(id: u32) -> Option<AutomationType> {
        match id {
            1 => {
                return Some(AutomationType::Reddit);
            }
            2 => {
                return Some(AutomationType::XKCD);
            }
            _ => {
                return None;
            }
        }
    }
}

pub struct ScheduledAutomation {
    pub cron_expresssion: String,
    pub cron: croner::Cron,
    pub db_name: String,
    pub display_name: String,
    pub channelid: u64,
    pub iterator: u32,
    pub lastids: serde_json::Value,
    pub db_scheduled_id: u32,
    pub handler: AutomationType,
    pub has_image: bool,
}

impl ScheduledAutomation {
    pub fn new(
        cron_expresssion: String,
        db_name: String,
        display_name: String,
        channelid: u64,
        iterator: u32,
        lastids: String,
        db_scheduled_id: u32,
        handler: u32,
        has_image: bool,
    ) -> ScheduledAutomation {
        return ScheduledAutomation {
            cron: croner::Cron::new(&cron_expresssion).parse().unwrap(),
            cron_expresssion,
            db_name,
            display_name,
            channelid,
            iterator,
            lastids: serde_json::from_str(&lastids).unwrap(),
            db_scheduled_id,
            handler: AutomationType::from_db(handler).unwrap(),
            has_image,
        };
    }

    pub fn next_up(&self) -> DateTime<Local> {
        return self
            .cron
            .find_next_occurrence(&Local::now(), false)
            .unwrap();
    }
    pub fn update_db(&self, conn: &Connection) {
        let new_values = Automation {
            channelid: self.channelid,
            lastids: self.lastids.to_string(),
            cron: self.cron_expresssion.clone(),
            iterator: self.iterator,
            name: self.display_name.clone(),
            scheduled_id: self.db_scheduled_id,
        };
        database::update_scheduled(conn, &new_values)
    }

    pub fn increment(&mut self) {
        self.iterator += 1;
    }

    pub fn add_id(&mut self, id: String) {
        let array = self.lastids.as_array_mut().unwrap();
        while array.len() >= _DUPLICATE_MAX_ARRAY_SIZE.try_into().unwrap() {
            array.remove(0);
        }
        array.push(id.into());
    }

    pub fn get_ids(&mut self) -> &Vec<serde_json::Value> {
        self.lastids.as_array().unwrap()
    }

    pub fn is_post_duplicate(&self, id: String) -> bool {
        let json_id = serde_json::Value::from(id);
        return self.lastids.as_array().unwrap().contains(&json_id);
    }
}
