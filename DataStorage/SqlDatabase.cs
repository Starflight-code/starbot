using System.Data.Entity.Infrastructure.Interception;
using System.Data.SQLite;
using Discord.WebSocket;
using StarBot;

class SqlDatabase
{
    private readonly SQLiteConnection connection;
    public SqlDatabase()
    {
        string pathToDB = "sqlDatabase.db";
        connection = new SQLiteConnection("Data Source=" + pathToDB + ";Version=3;");
        connection.Open();
    }

    public async void MigrateFromLegacy(DiscordSocketClient client)
    {
        Database data = new(client);
        var transaction = await connection.BeginTransactionAsync();
        //await transaction.SaveAsync("Migration");
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
        animechannel int,
        animemeschannel int,
        qotdchannel int,
        xkcdchannel int,
        catchannel int,
        reportchannel int
        );
        ";
        _ = command.ExecuteNonQuery();
        foreach (ulong guild in data.guilds)
        {
            command.Reset();
            command.CommandText = @"
            INSERT INTO guildData
            VALUES ($guildid, $animenumber, $animemesnumber, $catnumber, $qotdnumber, $lastanimeids, $lastqotdids, $lastanimemeids, $animechannel, $animemeschannel, $qotdchannel, $xkcdchannel, $catchannel, $reportchannel)
            ";
            command.Parameters.AddWithValue("$guildid", guild);
            command.Parameters.AddWithValue("$animenumber", data.fetchValue("animenumber", guild));
            command.Parameters.AddWithValue("$animemesnumber", data.fetchValue("animemesnumber", guild));
            command.Parameters.AddWithValue("$qotdnumber", data.fetchValue("questionnumber", guild));
            command.Parameters.AddWithValue("$catnumber", data.fetchValue("catnumber", guild));
            command.Parameters.AddWithValue("$lastanimeids", data.fetchValue("lastanimeids", guild));
            command.Parameters.AddWithValue("$lastanimemeids", data.fetchValue("lastanimemesids", guild));
            command.Parameters.AddWithValue("$lastqotdids", data.fetchValue("lastquestionofthedayids", guild));
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
}