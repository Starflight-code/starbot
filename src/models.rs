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
    pub automations: String,
    pub metadata: String,
    pub guild_id: String,
}

#[derive(Queryable, Selectable, Identifiable, Associations, Debug)]
#[diesel(table_name = crate::schema::scheduled)]
#[diesel(belongs_to(Guild))]
#[diesel(belongs_to(Automation))]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Scheduled {
    pub id: i32,
    pub post_id_history: String,
    pub iterator: i32,
    pub cron: String,
    pub display_name: String,
    pub settings: String,
    pub guild_id: i32,
    pub automation_id: i32,
    pub channel_id: String,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = scheduled)]
pub struct NewScheduled<'a> {
    pub channel_id: &'a str,
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
    pub guild_id: &'a str,
    pub automations: &'a str,
    pub metadata: &'a str,
}
/*
Guild Ticket Types:
guild_id (pk to Guild): int, ticket type display name: string, roles allowed (view/send):  json (role ids),roles allowed (view): json, control roles: json (allows adding new roles to ticket or removing them), roles to mention: json (role ids)

Ticket object:
guild_id (pk to Guild), channelid: string, tickettype: (pk to ticket type), initiator: (userid) string, overrides: json [action: (added/removed), (user or role), (user/role id)], claimed by: userid

Message:
for ticket: (pk of ticket), sent by: userid, date sent: datetime/int since unix epoch, anonymous: bool, content: string
*/
