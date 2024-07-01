using System.Data;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Dataflow;
using Discord;
using Discord.Audio.Streams;
using Discord.WebSocket;
using StarBot;


public class SqlDatabase {
    private readonly SQLiteConnection connection;
    public SqlDatabase(DiscordSocketClient client) {
        bool needsMigration = false;
        string pathToDB = "sqlDatabase.db";
        if (!File.Exists(pathToDB)) {
            needsMigration = true;
        }
        connection = new SQLiteConnection("Data Source=" + pathToDB + ";Version=3;");
        connection.Open();
        if (needsMigration) {
            MigrateFromLegacy(client);
        }
    }

    public async void MigrateFromLegacy(DiscordSocketClient client) {
        Database data = new(client);
        var transaction = await connection.BeginTransactionAsync();
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE guildData (
        guildid int primary key,
        animenumber int,
        animemesnumber int,
        catnumber int,
        qotdnumber int,
        lastanimeids text,
        lastqotdids text,
        lastanimemeids text,
        lastcatids text,
        animechannel int,
        animemeschannel int,
        qotdchannel int,
        xkcdchannel int,
        catchannel int,
        reportchannel int
        );
        ";
        _ = command.ExecuteNonQuery();
        foreach (ulong guild in data.guilds) {
            command.Reset();
            command.CommandText = @"
            INSERT INTO guildData
            VALUES ($guildid, $animenumber, $animemesnumber, $catnumber, $qotdnumber, $lastanimeids, $lastqotdids, $lastcatids, $lastanimemeids, $animechannel, $animemeschannel, $qotdchannel, $xkcdchannel, $catchannel, $reportchannel)
            ";
            command.Parameters.AddWithValue("$guildid", guild);
            command.Parameters.AddWithValue("$animenumber", data.fetchValue("animenumber", guild));
            command.Parameters.AddWithValue("$animemesnumber", data.fetchValue("animemesnumber", guild));
            command.Parameters.AddWithValue("$qotdnumber", data.fetchValue("questionnumber", guild));
            command.Parameters.AddWithValue("$catnumber", data.fetchValue("catnumber", guild));
            command.Parameters.AddWithValue("$lastanimeids", data.fetchValue("lastanimeids", guild));
            command.Parameters.AddWithValue("$lastanimemeids", data.fetchValue("lastanimemesids", guild));
            command.Parameters.AddWithValue("$lastqotdids", data.fetchValue("lastquestionofthedayids", guild));
            command.Parameters.AddWithValue("$lastcatids", data.fetchValue("lastcatids", guild));
            command.Parameters.AddWithValue("$animechannel", data.fetchValue("anime channel", guild));
            command.Parameters.AddWithValue("$catchannel", data.fetchValue("cat channel", guild));
            command.Parameters.AddWithValue("$animemeschannel", data.fetchValue("animemes channel", guild));
            command.Parameters.AddWithValue("$xkcdchannel", data.fetchValue("xkcd channel", guild));
            command.Parameters.AddWithValue("$qotdchannel", data.fetchValue("qotd channel", guild));
            command.Parameters.AddWithValue("$reportchannel", data.fetchValue("report channel", guild));
            await command.ExecuteNonQueryAsync();
        }
        await transaction.CommitAsync();
    }
    public async Task<T?> readFromDB<T>(string entry, ulong guildID) {
        entry = entry.ToLower();
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = @$"
        SELECT {entry} FROM guildData WHERE guildid=$guildid
        ";
        command.Parameters.AddWithValue("$guildid", guildID);
        T? output = default;
        using (var reader = await command.ExecuteReaderAsync()) {
            while (reader.Read()) {
                if (reader.GetFieldType(0) == typeof(T)) {
                    output = await reader.GetFieldValueAsync<T>(0);
                } else {
                    return default;
                }
            }
        }
        return output;
    }

    public async void writeToDB<T>(string entry, T toWrite, ulong guildID) {
        entry = entry.ToLower();
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = @$"
        UPDATE guildData SET {entry}=$updatedValue WHERE guildid=$guildID
        ";
        command.Parameters.AddWithValue("$guildID", guildID);
        command.Parameters.AddWithValue("$updatedValue", toWrite);
        await command.ExecuteNonQueryAsync();
    }

    public async void iterateValue(string entry, ulong guildID) {
        entry = entry.ToLower();
        using (var transaction = await connection.BeginTransactionAsync()) {
            int value = await readFromDB<int>(entry, guildID);
            writeToDB(entry, (++value).ToString(), guildID);

            await transaction.CommitAsync();
        }
    }

    public async void addGuild(ulong guildID) {
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO guildData
            VALUES ($guildid, $animenumber, $animemesnumber, $catnumber, $qotdnumber, $lastanimeids, $lastqotdids, $lastanimemeids, $animechannel, $animemeschannel, $qotdchannel, $xkcdchannel, $catchannel, $reportchannel)
            ";
        command.Parameters.AddWithValue("$guildid", guildID);
        command.Parameters.AddWithValue("$animenumber", 0);
        command.Parameters.AddWithValue("$animemesnumber", 0);
        command.Parameters.AddWithValue("$qotdnumber", 0);
        command.Parameters.AddWithValue("$catnumber", 0);
        command.Parameters.AddWithValue("$lastanimeids", "");
        command.Parameters.AddWithValue("$lastanimemeids", "");
        command.Parameters.AddWithValue("$lastqotdids", "");
        command.Parameters.AddWithValue("$animechannel", 0);
        command.Parameters.AddWithValue("$catchannel", 0);
        command.Parameters.AddWithValue("$animemeschannel", 0);
        command.Parameters.AddWithValue("$xkcdchannel", 0);
        command.Parameters.AddWithValue("$qotdchannel", 0);
        command.Parameters.AddWithValue("$reportchannel", 0);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<ulong>> getGuilds() {
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT guildid FROM guildData
            ";
        List<ulong> guildIds = new();
        using (var reader = await command.ExecuteReaderAsync()) {
            while (reader.Read()) {
                guildIds.Add((ulong)reader.GetInt64(0));
            }
        }
        return guildIds;
    }
}