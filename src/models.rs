use diesel::{prelude::Associations, AsChangeset, Identifiable, Insertable, Queryable, Selectable};

use crate::schema::{automation, guild, scheduled};

#[derive(Queryable, Selectable, Identifiable, Debug)]
#[diesel(table_name = crate::schema::automation)]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Automation {
    pub id: i32,
    pub name: String,
    pub handler: i32,
    pub metadata: String,
    pub has_image: bool,
}

#[derive(Queryable, Selectable, Identifiable, Debug)]
#[diesel(table_name = crate::schema::guild)]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Guild {
    pub id: i32,
    pub guild_id: i64,
    pub automations: String,
    pub metadata: String,
}

#[derive(Queryable, Selectable, Identifiable, Associations, Debug)]
#[diesel(table_name = crate::schema::scheduled)]
#[diesel(belongs_to(Guild))]
#[diesel(belongs_to(Automation))]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Scheduled {
    pub id: i32,
    pub channel_id: i64,
    pub post_id_history: String,
    pub iterator: i32,
    pub cron: String,
    pub display_name: String,
    pub settings: String,
    pub guild_id: i32,
    pub automation_id: i32,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = scheduled)]
pub struct NewScheduled<'a> {
    pub channel_id: i64,
    pub automation_id: i32,
    pub post_id_history: &'a str,
    pub iterator: i32,
    pub cron: &'a str,
    pub display_name: &'a str,
    pub settings: &'a str,
    pub guild_id: i32,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = automation)]
pub struct NewAutomation<'a> {
    pub name: &'a str,
    pub handler: i32,
    pub metadata: &'a str,
    pub has_image: bool,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = guild)]
pub struct NewGuild<'a> {
    pub guild_id: i64,
    pub automations: &'a str,
    pub metadata: &'a str,
}
