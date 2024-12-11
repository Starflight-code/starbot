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
        automations -> Text,
        metadata -> Text,
        guild_id -> Text,
    }
}

diesel::table! {
    scheduled (id) {
        id -> Integer,
        post_id_history -> Text,
        iterator -> Integer,
        cron -> Text,
        display_name -> Text,
        settings -> Text,
        guild_id -> Integer,
        automation_id -> Integer,
        channel_id -> Text,
    }
}

diesel::joinable!(scheduled -> automation (automation_id));
diesel::joinable!(scheduled -> guild (guild_id));

diesel::allow_tables_to_appear_in_same_query!(
    automation,
    guild,
    scheduled,
);
