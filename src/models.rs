use diesel::{prelude::Associations, AsChangeset, Identifiable, Insertable, Queryable, Selectable};

use crate::schema::{automation, guild, scheduled, ticket_type, ticket, message};

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

#[derive(Queryable, Selectable, Identifiable, Associations, Debug)]
#[diesel(table_name = crate::schema::ticket_type)]
#[diesel(belongs_to(Guild))]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct TicketType {
    pub id: i32,
    pub guild_id: i32,
    pub display_name: String,
    pub roles_send: String,
    pub roles_viewonly: String,
    pub roles_override: String,
    pub roles_mention_on_create: String,
}


#[derive(Queryable, Selectable, Identifiable, Associations, Debug)]
#[diesel(table_name = crate::schema::ticket)]
#[diesel(belongs_to(Guild))]
#[diesel(belongs_to(TicketType))]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Ticket {
    pub id: i32,
    pub guild_id: i32,
    pub channel_id: String,
    pub ticket_type_id: i32,
    pub initiator: String,
    pub overrides: String,
    pub claimed_by: String,
}

#[derive(Queryable, Selectable, Identifiable, Associations, Debug)]
#[diesel(table_name = crate::schema::message)]
#[diesel(belongs_to(Ticket))]
#[diesel(check_for_backend(diesel::sqlite::Sqlite))]
pub struct Message { 
    pub id: i32,
    pub ticket_id: i32,
    pub sender: String,
    pub sent_at: i64,
    pub anonymous: bool,
    pub content: String,
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

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = ticket_type)]
pub struct NewTicketType<'a> {
    pub guild_id: i32,
    pub display_name: &'a str,
    pub roles_send: &'a str,
    pub roles_viewonly: &'a str,
    pub roles_override: &'a str,
    pub roles_mention_on_create: &'a str,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = ticket)]
pub struct NewTicket<'a> {
    pub guild_id: i32,
    pub channel_id: &'a str,
    pub ticket_type_id: i32,
    pub initiator: &'a str,
    pub overrides: &'a str,
    pub claimed_by: &'a str,
}

#[derive(Insertable, AsChangeset)]
#[diesel(table_name = message)]
pub struct NewMessage<'a> {
    pub ticket_id: i32,
    pub sender: &'a str,
    pub sent_at: i64,
    pub anonymous: bool,
    pub content: &'a str,
}


/*
Guild Ticket Types:
guild_id (pk to Guild): int, ticket type display name: string, roles allowed (view/send):  json (role ids),roles allowed (view): json, control roles: json (allows adding new roles to ticket or removing them), roles to mention: json (role ids)

Ticket object:
guild_id (pk to Guild), channelid: string, tickettype: (pk to ticket type), initiator: (userid) string, overrides: json [action: (added/removed), (user or role), (user/role id)], claimed by: userid

Message:
for ticket: (pk of ticket), sent by: userid, date sent: datetime/int since unix epoch, anonymous: bool, content: string
*/
