-- Your SQL goes here
DROP TABLE IF EXISTS `posts`;
CREATE TABLE `scheduled`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild` INTEGER NOT NULL,
	`channel_id` BIGINT NOT NULL,
	`automation` INTEGER NOT NULL,
	`post_id_history` TEXT NOT NULL,
	`iterator` INTEGER NOT NULL,
	`cron` TEXT NOT NULL,
	`display_name` TEXT NOT NULL,
	`settings` TEXT NOT NULL
);

CREATE TABLE `guilds`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild_id` BIGINT NOT NULL,
	`automations` TEXT NOT NULL,
	`metadata` TEXT NOT NULL
);

CREATE TABLE `automations`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`name` TEXT NOT NULL,
	`handler` INTEGER NOT NULL,
	`metadata` TEXT NOT NULL,
	`has_image` BOOL NOT NULL
);

