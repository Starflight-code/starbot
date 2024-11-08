// @generated automatically by Diesel CLI.

diesel::table! {
    automation (id) {
        id -> Integer,
        name -> Text,
        handler -> Integer,
        metadata -> Text,
        has_image -> Bool,
    }
}

diesel::table! {
    guild (id) {
        id -> Integer,
        guild_id -> BigInt,
        automations -> Text,
        metadata -> Text,
    }
}

diesel::table! {
    scheduled (id) {
        id -> Integer,
        channel_id -> BigInt,
        post_id_history -> Text,
        iterator -> Integer,
        cron -> Text,
        display_name -> Text,
        settings -> Text,
        guild_id -> Integer,
        automation_id -> Integer,
    }
}

diesel::joinable!(scheduled -> automation (automation_id));
diesel::joinable!(scheduled -> guild (guild_id));

diesel::allow_tables_to_appear_in_same_query!(automation, guild, scheduled,);
