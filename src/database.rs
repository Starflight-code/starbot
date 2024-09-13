use std::path::Path;

use rusqlite::Connection;
use crate::{
    scheduler_data::{AutomationType, ScheduledAutomation},
    Error,
};
use serde_json::Value;

pub struct Guild {
    pub guild_id: u64,
}
#[derive(Debug)]
pub struct Automation {
    pub channelid: u64,
    pub lastids: String,
    pub cron: String,
    pub iterator: u32,
    pub name: String,
    pub scheduled_id: u32,
}

pub async fn create_connection() -> Connection {
    let mut requires_initialization = false;
    if !Path::new("data.db").exists() {
        requires_initialization = true;
    }
    let connection = rusqlite::Connection::open("data.db").expect("Database Connection Error");
    if requires_initialization {
        initialize_database(&connection);
    }
    return connection;
}

pub fn initialize_database(conn: &Connection) {
    conn.execute(
        "CREATE TABLE IF NOT EXISTS guilds (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                guild_id ULONG NOT NULL UNIQUE,
                automations TEXT, 
                metadata TEXT
                )",
        (),
    )
    .unwrap();
    conn.execute(
        "CREATE TABLE IF NOT EXISTS automations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(100) NOT NULL UNIQUE,
                handler INTEGER NOT NULL,
                has_image BOOLEAN NOT NULL DEFAULT 1,
                metadata TEXT
                )",
        (),
    )
    .unwrap();

    conn.execute(
        "CREATE TABLE IF NOT EXISTS scheduled (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        guild INTEGER NOT NULL,
        channel_id ULONG NOT NULL UNIQUE,
        automation INTEGER NOT NULL,
        post_id_history TEXT,
        iterator INTEGER DEFAULT 1,
        cron VARCHAR(20) NOT NULL,
        display_name VARCHAR(100) NOT NULL,
        settings TEXT,
        CONSTRAINT fk_guild
            FOREIGN KEY(guild)
            REFERENCES guilds(id),
        CONSTRAINT fk_automation
            FOREIGN KEY(automation)
            REFERENCES automations(id)
    )",
        (),
    )
    .unwrap();

    return ();
}

pub fn add_guild(conn: &Connection, guildid: &u64) {
    if !does_guild_exist(conn, guildid) {
        _ = conn
            .execute(
                "INSERT INTO guilds (guild_id, automations, metadata) VALUES (?1, ?2, ?3)",
                (&guildid, &"[]", &"[]"),
            )
            .unwrap();
    }
}

pub fn does_guild_exist(conn: &Connection, guild_id: &u64) -> bool {
    let mut statement = conn
        .prepare("SELECT guild_id FROM guilds WHERE guild_id=?1")
        .unwrap();
    return statement.exists([guild_id]).unwrap();
}

pub fn add_automation(
    conn: &Connection,
    automation_name: &String,
    automation_type: AutomationType,
    has_image: &bool,
) {
    let query = conn.prepare("SELECT id FROM automations WHERE name=?1");
    if !query.unwrap().exists([&automation_name]).unwrap() {
        let automation_name = automation_name.to_lowercase();
        conn.execute(
            "INSERT INTO automations (name, handler, metadata, has_image) VALUES (?1, ?2, ?3, ?4)",
            (automation_name, automation_type.value(), &"[]", has_image),
        )
        .unwrap();
    }
}

pub fn add_scheduled(
    conn: &Connection,
    guildid: &u64,
    channelid: &u64,
    automation: &String,
    cron: &String,
    name: &String,
) -> Result<(), Error> {
    let mut query = conn
        .prepare("SELECT id FROM automations WHERE name=?1")
        .unwrap();
    let mut automation_id: u32 = 0;
    let response = query.query_row([&automation], |row| Ok(automation_id = row.get(0)?));
    if response.is_err() {
        return Err("This automation does not exist".into());
    }
    let mut automation_json: String = String::new();
    let mut guild_db_id: u32 = 0;
    let _ = conn.query_row(
        "SELECT id, automations FROM guilds WHERE guild_id=?1",
        [guildid],
        |row| {
            Ok({
                guild_db_id = row.get(0)?;
                automation_json = row.get(1)?;
            })
        },
    );
    let mut automation_json: Value = serde_json::from_str(&automation_json).unwrap();
    let mut exists: bool = false;
    for automation_json in automation_json.as_array().unwrap() {
        if automation_json[0].as_u64().unwrap() == u64::from(automation_id) {
            exists = true;
            break;
        }
    }
    if exists {
        return Err("This is already scheduled".into());
    }
    conn.execute(
            "INSERT INTO scheduled (guild, channel_id, automation, post_id_history, cron, display_name, settings) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7)",
        (guild_db_id, channelid, automation_id, "[]", cron, name, "[]"),
    )
    .unwrap();
    let mut scheduled_id: u32 = 0;
    let mut query = conn
        .prepare("SELECT id FROM scheduled WHERE guild=?1 AND automation=?2")
        .unwrap();
    query
        .query_row((guild_db_id, automation_id), |row| {
            Ok(scheduled_id = row.get(0)?)
        })
        .unwrap();

    automation_json
        .as_array_mut()
        .unwrap()
        .push(serde_json::to_value((automation_id, scheduled_id)).unwrap());
    conn.execute(
        "UPDATE guilds SET automations=?1 WHERE id=?2",
        (automation_json.to_string(), guild_db_id),
    )
    .unwrap();
    Ok(())
}

pub fn update_scheduled(conn: &Connection, new_values: &Automation) {
    conn.execute(
        "UPDATE scheduled SET channel_id=?1, post_id_history=?2, cron=?3, iterator=?4, display_name=?5 WHERE id=?6",
        (
            new_values.channelid,
            &new_values.lastids,
            &new_values.cron,
            new_values.iterator,
            &new_values.name,
            new_values.scheduled_id,
        ),
    )
    .unwrap();
}

pub fn get_automations(conn: &Connection) -> Vec<ScheduledAutomation> {
    let mut automations: Vec<ScheduledAutomation> = Vec::new();
    let mut query = conn.prepare("SELECT s.channel_id, s.cron, s.display_name, s.iterator, s.post_id_history, s.id, a.name, a.handler, a.has_image FROM scheduled s LEFT JOIN automations a ON s.automation=a.id").unwrap();
    let rows = query
        .query_map([], |row| {
            Ok(ScheduledAutomation::new(
                row.get(1)?,
                row.get(6)?,
                row.get(2)?,
                row.get(0)?,
                row.get(3)?,
                row.get(4)?,
                row.get(5)?,
                row.get(7)?,
                row.get(8)?,
            ))
        })
        .unwrap();
    for row in rows {
        automations.push(row.unwrap());
    }
    return automations;
}

pub fn get_automation(conn: &Connection, channel_id: &u64) -> Option<ScheduledAutomation> {
    let mut query = conn.prepare("SELECT s.channel_id, s.cron, s.display_name, s.iterator, s.post_id_history, s.id, a.name, a.handler, a.has_image FROM scheduled s LEFT JOIN automations a ON s.automation=a.id WHERE s.channel_id=?1").unwrap();
    let rows = query.query_row([channel_id], |row| {
        Ok(ScheduledAutomation::new(
            row.get(1)?,
            row.get(6)?,
            row.get(2)?,
            row.get(0)?,
            row.get(3)?,
            row.get(4)?,
            row.get(5)?,
            row.get(7)?,
            row.get(8)?,
        ))
    });
    if rows.is_err() {
        return None;
    }
    return Some(rows.unwrap());
}
