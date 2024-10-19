// @generated automatically by Diesel CLI.

diesel::table! {
    automations (id) {
        id -> Integer,
        name -> Text,
        handler -> Integer,
        metadata -> Text,
        has_image -> Bool,
    }
}

diesel::table! {
    guilds (id) {
        id -> Integer,
        guild_id -> BigInt,
        automations -> Text,
        metadata -> Text,
    }
}

diesel::table! {
    scheduled (id) {
        id -> Integer,
        guild -> Integer,
        channel_id -> BigInt,
        automation -> Integer,
        post_id_history -> Text,
        iterator -> Integer,
        cron -> Text,
        display_name -> Text,
        settings -> Text,
    }
}

diesel::allow_tables_to_appear_in_same_query!(automations, guilds, scheduled,);
