-- This file should undo anything in `up.sql`
CREATE TABLE `posts`(
	`id` INTEGER NOT NULL PRIMARY KEY,
	`title` TEXT NOT NULL,
	`body` TEXT NOT NULL,
	`published` BOOL NOT NULL
);

DROP TABLE IF EXISTS `scheduled`;
DROP TABLE IF EXISTS `guilds`;
DROP TABLE IF EXISTS `automations`;
