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

diesel::table! {
    ticket_type (id) {
        id -> Integer,
        guild_id -> Integer,
        display_name -> Text,
        roles_send -> Text,
        roles_viewonly -> Text,
        roles_override -> Text,
        roles_mention_on_create -> Text,
    }
}

diesel::table! {
    ticket (id) {
        id -> Integer,
        guild_id -> Integer,
        channel_id -> Text,
        ticket_type_id -> Integer,
        initiator -> Text,
        overrides -> Text,
        claimed_by -> Text,
    }
}

diesel::table! {
    message (id) {
        id -> Integer,
        ticket_id -> Integer,
        sender -> Text,
        sent_at -> BigInt,
        anonymous -> Bool,
        content -> Text,
    }
}

diesel::joinable!(scheduled -> automation (automation_id));
diesel::joinable!(scheduled -> guild (guild_id));

diesel::allow_tables_to_appear_in_same_query!(
    automation,
    guild,
    scheduled,
);
