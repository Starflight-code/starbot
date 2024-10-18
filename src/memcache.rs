use crate::settings::_CACHE_LIFESPAN_HOURS;
use chrono::{DateTime, TimeDelta, Utc};
use serenity::prelude::TypeMapKey;
use std::collections::HashMap;

struct CachedObject {
    expiration: DateTime<Utc>,
    json: serde_json::Value,
}

impl CachedObject {
    fn is_expired(&self) -> bool {
        return self.expiration < Utc::now();
    }
}

pub struct Memcache {
    cache: HashMap<String, CachedObject>,
}

impl TypeMapKey for Memcache {
    type Value = Memcache;
}

impl Memcache {
    fn remove_if_expired(&mut self, key: &String) {
        let value = self.cache.get(key);
        if !value.is_none() && value.unwrap().is_expired() {
            self.cache.remove(key);
        }
    }
    pub fn new() -> Memcache {
        return Memcache {
            cache: HashMap::new(),
        };
    }
    pub fn add(&mut self, key: String, to_add: serde_json::Value) {
        self.cache.insert(
            key,
            CachedObject {
                expiration: Utc::now() + TimeDelta::hours(_CACHE_LIFESPAN_HOURS.into()),
                json: to_add,
            },
        );
    }
    pub fn get(&mut self, key: String) -> Option<&serde_json::Value> {
        {
            // keeps self in a nested context (releasing borrow), preventing an error
            self.remove_if_expired(&key);
        }
        let value = self.cache.get(&key);

        match value {
            Some(_) => {
                return Some(&value.unwrap().json);
            }
            None => {
                return None;
            }
        }
    }
}
