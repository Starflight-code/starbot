pub trait Environment {
    fn check(&self) -> bool;
    fn print(&self, to_print: &impl ValidationProcessor);
}

pub trait ValidationProcessor {
    fn log(&self, log_level: LogLevel, data: String);
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
    fn log(&self, level: LogLevel, data: String) {
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
    fn print(&self, print_to: &impl ValidationProcessor) {
        let mut to_send: String = format!(
            "Execution Environment for Reddit Automation: {}",
            self.automation
        );
        to_send.push_str(
            format!(
                "\nHistory: {} \n{:#}",
                self.history_state, &self.selected_json
            )
            .as_str(),
        );
        print_to.log(LogLevel::Warning, to_send);
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
