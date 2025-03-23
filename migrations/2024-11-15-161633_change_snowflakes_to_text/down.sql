-- This file should undo anything in `up.sql`

ALTER TABLE `guild` DROP `guild_id`;
ALTER TABLE `guild` ADD `guild_id` BIGINT NOT NULL DEFAULT 0;

ALTER TABLE `scheduled` DROP `channel_id`;
ALTER TABLE `scheduled` ADD `channel_id` BIGINT NOT NULL DEFAULT 0;