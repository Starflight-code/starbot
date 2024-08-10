use rand::{self, Rng};
use reqwest::header;

use crate::{memcache::Memcache, scheduler_data::ScheduledAutomation};

pub struct Post {
    pub title: String,
    pub body: String,
    pub powered_by: String,
    pub url: Option<String>,
    pub image_link: Option<String>,
}

pub async fn reddit_handler(automation: &mut ScheduledAutomation, memcache: &mut Memcache) -> Post {
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
    let cache = memcache.get(subreddit.to_string());
    let json = if cache.is_none() {
        let url = format!("https://www.reddit.com/r/{subreddit}/.json?limit=100&t=day");
        let json_response = http_client
            .get(url)
            .headers(headers)
            .send()
            .await
            .expect("API Request Error")
            .text()
            .await
            .expect("API Response Error");
        let json: serde_json::Value =
            serde_json::from_str(&json_response).expect("JSON Deserialization Error");
        memcache.add(subreddit.to_string(), json.clone());
        json
    } else {
        let json = cache.unwrap();
        json.clone()
    };
    let mut max_fail_iterator = 0;
    let mut json_segment = json["data"]["children"][0]["data"].to_owned();
    let image_link: Option<String>;

    let mut duplicate_id = false;
    let mut rng = rand::thread_rng();

    if automation.has_image {
        let mut image_present = false;
        while (!image_present || duplicate_id) && max_fail_iterator < 150 {
            let randint = rng.gen_range(0..=100);
            json_segment = json["data"]["children"][randint]["data"].to_owned();
            image_present = is_image_link(&json_segment["url_overridden_by_dest"].to_string());
            duplicate_id =
                automation.is_post_duplicate(json_segment["id"].as_str().unwrap().to_string());
            max_fail_iterator += 1;
        }
        image_link = Some(
            json_segment["url_overridden_by_dest"]
                .to_string()
                .replace("\"", ""),
        )
    } else {
        image_link = None;
        while duplicate_id && max_fail_iterator < 150 {
            let randint = rng.gen_range(0..=100);
            json_segment = json["data"]["children"][randint]["data"].to_owned();
            duplicate_id = automation.is_post_duplicate(json_segment["id"].to_string());
            max_fail_iterator += 1;
        }
    }
    if max_fail_iterator >= 150 {
        panic!("Something went wrong, couldn't locate an image in 150 loops.");
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
    automation.increment();
    automation.add_id(json_segment["id"].to_string());
    return post;
}

pub async fn xkcd_handler() -> Post {
    let url = String::from("https://xkcd.com/info.0.json");
    let json_response = reqwest::get(&url).await.unwrap().text().await.unwrap();
    let json: serde_json::Value =
        serde_json::from_str(&json_response.as_str()).expect("JSON Deserialization Error");
    Post {
        title: format!("{} - {}", json["safe_title"], json["num"]),
        body: json["alt"].to_string(),
        powered_by: String::from("xkcd.com"),
        image_link: Some(json["img"].to_string().replace("\"", "")),
        url: Some(format!("https://xkcd.com/{}/", json["num"])),
    }
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
