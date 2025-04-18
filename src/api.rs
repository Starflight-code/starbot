use rand::{self, Rng};
use reqwest::header;

use crate::{
    monitor::{ConsoleLog, Environment, RedditEnv},
    scheduler_data::ScheduledAutomation,
    GLOBAL_MEMCACHE,
};

const _MAX_CHECK_FAILURES: i32 = 150; // maximum amount of failures to allow while looping (prevents infinite loop)
const _NUMBER_OF_POSTS: usize = 100; // number of posts to request for Reddit handler

pub struct Post {
    pub title: String,
    pub body: String,
    pub powered_by: String,
    pub url: Option<String>,
    pub image_link: Option<String>,
}

#[allow(unused_macros)]
macro_rules! unwrap_or_retry {
    ($e:expr) => {
        match $e {
            Ok(x) => x,
            Err(_) => {
                failures += 1;
                continue;
            }
        }
    };
}

#[allow(unused_macros)]
macro_rules! manage_failures {
    ($b:block, $f:block, $n:literal) => {
        #[allow(unused_mut)]
        let mut failures = 0;
        while failures < $n {
            $b
        }
        $f
    };
}

pub async fn reddit_handler(automation: &mut ScheduledAutomation) -> Result<Post, String> {
    let http_client = reqwest::Client::new();
    let subreddit = automation.db_name.as_str();

    let mut headers = header::HeaderMap::new();
    headers.append(
        reqwest::header::ACCEPT,
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8"
            .parse()
            .unwrap(),
    );
    headers.append(reqwest::header::USER_AGENT, "Mozilla/5.0".parse().unwrap());
    let mut memcache_inner = GLOBAL_MEMCACHE.lock().await;
    let cache = memcache_inner.get(subreddit.to_string());
    let json = if cache.is_some() {
        let json = cache.unwrap();
        json.clone()
    } else {
        let url = format!(
            "https://www.reddit.com/r/{subreddit}/.json?limit={}&t=day",
            _NUMBER_OF_POSTS
        );
        let json_response = http_client.get(url).headers(headers).send().await;

        if let Err(_) = json_response {
            return Err(String::from("API Request Error"));
        }
        let json_response = json_response.unwrap().text().await;

        if let Err(_) = json_response {
            return Err(String::from("API Response Error"));
        }
        let json_response = json_response.unwrap();
        let json: serde_json::Value =
            serde_json::from_str(&json_response).expect("JSON Deserialization Error");
        memcache_inner.add(subreddit.to_string(), json.clone());
        json
    };
    let mut max_fail_iterator = 0;
    let mut json_segment = json["data"]["children"][0]["data"].to_owned();
    let image_link: Option<String>;

    let mut duplicate_id = true; // initial to true, makes !automation.has_image loop run
    let mut rng = rand::rng();

    if automation.has_image {
        let mut image_present = false;
        while (!image_present || duplicate_id) && max_fail_iterator < _MAX_CHECK_FAILURES {
            let randint: usize = rng.random_range(0..=_NUMBER_OF_POSTS);
            json_segment = json["data"]["children"][randint]["data"].to_owned();
            image_present = is_image_link(&json_segment["url_overridden_by_dest"].to_string());
            duplicate_id = automation.is_post_duplicate(json_segment["id"].to_string());
            max_fail_iterator += 1;
        }
        image_link = Some(
            json_segment["url_overridden_by_dest"]
                .to_string()
                .replace("\"", ""),
        )
    } else {
        image_link = None;
        while duplicate_id && max_fail_iterator < _MAX_CHECK_FAILURES {
            let randint: usize = rng.random_range(0..=_NUMBER_OF_POSTS);
            json_segment = json["data"]["children"][randint]["data"].to_owned();
            duplicate_id = automation.is_post_duplicate(json_segment["id"].to_string());
            if json["data"]["children"][randint]["data"]["subreddit"].to_string() == "null" {
                duplicate_id = true;
            }
            max_fail_iterator += 1;
        }
    }
    if max_fail_iterator >= _MAX_CHECK_FAILURES {
        println!("Something went wrong, couldn't locate an image in 150 loops.");
        dbg!(&json["data"]);
        return Err("Patience Exceeded in Check Loop".to_string());
    }
    let post = Post {
        title: format!("{} #{}", automation.display_name, automation.iterator),
        body: json_segment["title"].to_string().replace("\"", ""),
        powered_by: format!(
            "r/{}",
            json_segment["subreddit"].to_string().replace("\"", "")
        ),
        url: Some(format!(
            "https://www.reddit.com{}",
            json_segment["permalink"].to_string().replace("\"", "")
        )),
        image_link,
    };
    let env = RedditEnv::new(
        json_segment.clone(),
        automation.db_name.clone(),
        automation.get_ids().join(","),
    );
    if !env.check() {
        println!("Check failure occured, printing environment...");
        let log_method: ConsoleLog = ConsoleLog {};
        env.print(&log_method);
    }
    automation.increment();
    automation.add_id(json_segment["id"].to_string());
    return Ok(post);
}

pub async fn xkcd_handler() -> Result<Post, String> {
    let url = String::from("https://xkcd.com/info.0.json");
    let json_response = reqwest::get(&url).await;
    if let Err(_) = json_response {
        return Err(String::from("Failed to unwrap initial request."));
    }

    let json_response = json_response.unwrap().text().await;
    if let Err(_) = json_response {
        return Err(String::from("Failed to parse request response text."));
    }

    let json_response = json_response.unwrap();
    let json: serde_json::Value =
        serde_json::from_str(&json_response.as_str()).expect("JSON Deserialization Error");
    Ok(Post {
        title: format!("{} - {}", json["safe_title"], json["num"]),
        body: json["alt"].to_string(),
        powered_by: String::from("xkcd.com"),
        image_link: Some(json["img"].to_string().replace("\"", "")),
        url: Some(format!("https://xkcd.com/{}/", json["num"])),
    })
}

pub fn is_image_link(url: &str) -> bool {
    let image_extensions = ["jpg", "jpeg", "png"];

    let param_extension = url
        .trim()
        .split('.')
        .last()
        .unwrap()
        .to_lowercase()
        .replace("\"", "");

    for extension in image_extensions {
        if param_extension == extension {
            return true;
        }
    }
    return false;
}
