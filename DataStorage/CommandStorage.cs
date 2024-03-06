using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace StarBot.Storage;

public class CommandDatabase {
    public struct command {
        ulong id;
        ulong guildId;
        command(ulong id, ulong guildId) {
            this.id = id;
            this.guildId = guildId;
        }
    }
    public List<command> commands = new();

    public async void serialize() {
        string json = JsonConvert.SerializeObject(commands);
        await File.WriteAllTextAsync(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"/commands.db"), json);
    }

    public async void populateSelf() {
        string json = await File.ReadAllTextAsync(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"/commands.db"));
        var list = JsonConvert.DeserializeObject<List<command>>(json);
        commands.Clear();
        foreach (command value in list) {
            commands.Add(value);
        }
    }
}