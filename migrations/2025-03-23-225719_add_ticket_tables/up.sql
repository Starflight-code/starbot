-- Your SQL goes here



CREATE TABLE `ticket`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild_id` INTEGER NOT NULL,
	`channel_id` TEXT NOT NULL,
	`ticket_type_id` INTEGER NOT NULL,
	`initiator` TEXT NOT NULL,
	`overrides` TEXT NOT NULL,
	`claimed_by` TEXT NOT NULL
);

CREATE TABLE `ticket_type`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild_id` INTEGER NOT NULL,
	`display_name` TEXT NOT NULL,
	`roles_send` TEXT NOT NULL,
	`roles_viewonly` TEXT NOT NULL,
	`roles_override` TEXT NOT NULL,
	`roles_mention_on_create` TEXT NOT NULL
);

CREATE TABLE `message`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`ticket_id` INTEGER NOT NULL,
	`sender` TEXT NOT NULL,
	`sent_at` BIGINT NOT NULL,
	`anonymous` BOOL NOT NULL,
	`content` TEXT NOT NULL
);

