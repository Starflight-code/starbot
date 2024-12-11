-- Your SQL goes here

ALTER TABLE `guild` DROP `guild_id`;
ALTER TABLE `guild` ADD `guild_id` TEXT NOT NULL DEFAULT "-1";

ALTER TABLE `scheduled` DROP `channel_id`;
ALTER TABLE `scheduled` ADD `channel_id` TEXT NOT NULL DEFAULT "-1";
