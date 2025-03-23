use chrono::{DateTime, Local};
use diesel::{ExpressionMethods, SqliteConnection};
use diesel::{QueryDsl, RunQueryDsl};

use crate::models::{Automation, NewScheduled, Scheduled};
use crate::settings::_DUPLICATE_MAX_ARRAY_SIZE;

#[derive(Debug, poise::ChoiceParameter)]
pub enum AutomationType {
    Reddit = 1,
    XKCD = 2,
}

impl AutomationType {
    pub fn value(self) -> i32 {
        match self {
            AutomationType::Reddit => return 1,
            AutomationType::XKCD => return 2,
        }
    }

    pub fn from_db(id: i32) -> Option<AutomationType> {
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
    pub iterator: i32,
    pub lastids: String,
    pub db_scheduled_id: i32,
    pub handler: AutomationType,
    pub has_image: bool,
}

impl ScheduledAutomation {
    pub fn new(
        cron_expresssion: String,
        db_name: String,
        display_name: String,
        channelid: u64,
        iterator: i32,
        lastids: String,
        db_scheduled_id: i32,
        handler: i32,
        has_image: bool,
    ) -> ScheduledAutomation {
        return ScheduledAutomation {
            cron: croner::Cron::new(&cron_expresssion).parse().unwrap(),
            cron_expresssion,
            db_name,
            display_name,
            channelid,
            iterator,
            lastids: lastids,
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

    pub fn update_db(&self, db: &mut SqliteConnection) {
        use crate::schema::scheduled::dsl as scheduled_dsl;

        let current_object: Scheduled = scheduled_dsl::scheduled
            .filter(scheduled_dsl::id.eq(self.db_scheduled_id))
            .first(db)
            .unwrap();

        let to_update = NewScheduled {
            channel_id: &self.channelid.to_string(),
            automation_id: current_object.automation_id,
            post_id_history: &self.lastids,
            iterator: self.iterator,
            cron: &self.cron_expresssion,
            display_name: &self.display_name,
            settings: &current_object.settings,
            guild_id: current_object.guild_id,
        };

        diesel::update(scheduled_dsl::scheduled)
            .filter(scheduled_dsl::id.eq(self.db_scheduled_id))
            .set(to_update)
            .execute(db)
            .unwrap();
    }

    pub fn increment(&mut self) {
        self.iterator += 1;
    }

    pub fn add_id(&mut self, id: String) {
        let mut array: Vec<&str> = self.lastids.split(',').collect();
        while array.len() >= _DUPLICATE_MAX_ARRAY_SIZE.try_into().unwrap() {
            array.remove(0); // might be non-performant, but this is a really small list
        }
        array.push(&id);
        self.lastids = array.join(",");
    }

    pub fn get_ids(&mut self) -> Vec<String> {
        let ids: Vec<&str> = self.lastids.split(",").collect();
        let mut return_ids: Vec<String> = Vec::new();
        for id in ids {
            return_ids.push(String::from(id));
        }
        return_ids
    }

    pub fn is_post_duplicate(&self, id: String) -> bool {
        for last_id in self.lastids.split(",") {
            if String::from(last_id) == id {
                return true;
            }
        }
        return false;
    }

    pub fn from_db(item: Scheduled, db: &mut SqliteConnection) -> Self {
        use crate::schema::automation::dsl as automation_dsl;

        let parent_automation: Automation = automation_dsl::automation
            .filter(automation_dsl::id.eq(item.automation_id))
            .first(db)
            .unwrap();

        ScheduledAutomation::new(
            // eventually, ScheduledAutomation should merely wrap Scheduled and Automation for DB
            item.cron,
            parent_automation.name,
            item.display_name,
            item.channel_id.parse().unwrap(),
            item.iterator,
            item.post_id_history,
            item.id,
            parent_automation.handler,
            parent_automation.has_image,
        )
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn last_ids_check() {
        let scheduled = ScheduledAutomation {
            cron_expresssion: String::from("0 0 * * *"),
            cron: croner::Cron::new("0 0 * * *").parse().unwrap(),
            db_name: String::from("TestDB"),
            display_name: String::from("Test"),
            channelid: 1,
            iterator: 1,
            lastids: String::from("1,2,3,4,5"),
            db_scheduled_id: 1,
            handler: AutomationType::XKCD,
            has_image: false,
        };
        assert!(scheduled.is_post_duplicate(String::from("2")))
    }
}
