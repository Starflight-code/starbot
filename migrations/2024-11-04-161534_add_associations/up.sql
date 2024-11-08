-- Your SQL goes here
DROP TABLE IF EXISTS `automations`;
DROP TABLE IF EXISTS `guilds`;
ALTER TABLE `scheduled` DROP COLUMN `guild`;
ALTER TABLE `scheduled` DROP COLUMN `automation`;
ALTER TABLE `scheduled` ADD COLUMN `guild_id` INTEGER NOT NULL REFERENCES `guild`(id);
ALTER TABLE `scheduled` ADD COLUMN `automation_id` INTEGER NOT NULL REFERENCES `automation`(id);

CREATE TABLE `guild`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild_id` BIGINT NOT NULL,
	`automations` TEXT NOT NULL,
	`metadata` TEXT NOT NULL
);

CREATE TABLE `automation`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`name` TEXT NOT NULL,
	`handler` INTEGER NOT NULL,
	`metadata` TEXT NOT NULL,
	`has_image` BOOL NOT NULL
);

