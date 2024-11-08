-- This file should undo anything in `up.sql`
CREATE TABLE `automations`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`name` TEXT NOT NULL,
	`handler` INTEGER NOT NULL,
	`metadata` TEXT NOT NULL,
	`has_image` BOOL NOT NULL
);

CREATE TABLE `guilds`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`guild_id` BIGINT NOT NULL,
	`automations` TEXT NOT NULL,
	`metadata` TEXT NOT NULL
);

ALTER TABLE `scheduled` DROP COLUMN `guild_id`;
ALTER TABLE `scheduled` DROP COLUMN `automation_id`;
ALTER TABLE `scheduled` ADD COLUMN `guild` INTEGER NOT NULL;
ALTER TABLE `scheduled` ADD COLUMN `automation` INTEGER NOT NULL;

DROP TABLE IF EXISTS `guild`;
DROP TABLE IF EXISTS `automation`;
