use std::collections::HashSet;

pub trait Environment {
    fn check(&self) -> bool;
    fn print(&self);
}

pub trait ValidationProcessor {
    fn log(log_level: LogLevel, data: String);
}

pub enum LogLevel {
    Info,
    Warning,
    Error,
}

impl From<LogLevel> for String {
    fn from(item: LogLevel) -> String {
        match item {
            LogLevel::Info => String::from("Info"),
            LogLevel::Warning => String::from("Warning"),
            LogLevel::Error => String::from("Error"),
        }
    }
}

pub struct ConsoleLog {}

impl ValidationProcessor for ConsoleLog {
    fn log(level: LogLevel, data: String) {
        println!("{}: {}", String::from(level), data)
    }
}

pub struct RedditEnv {
    selected_json: serde_json::Value,
    automation: String,
    history_state: String,
}

impl RedditEnv {
    pub fn new(
        selected_json: serde_json::Value,
        automation: String,
        history_state: &Vec<serde_json::Value>,
    ) -> RedditEnv {
        let history: Vec<&str> = history_state
            .into_iter()
            .map(|x| x.as_str().unwrap())
            .collect();
        RedditEnv {
            selected_json,
            automation,
            history_state: history.join(","),
        }
    }
}

impl Environment for RedditEnv {
    fn print(&self) {
        println!(
            "Execution Environment for Reddit Automation: {}",
            self.automation
        );
        println!("History: {}", self.history_state);
        dbg!(&self.history_state);
    }
    fn check(&self) -> bool {
        let history: Vec<&str> = self.history_state.split(",").collect();
        for i in 0..history.len() {
            for j in i + 1..history.len() {
                if history[i].trim() == history[j].trim() {
                    return false;
                }
            }
        }
        return true;
    }
}
