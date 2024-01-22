using Discord.WebSocket;

namespace StarBot {
    internal class Database {
        Dictionary<ulong, DatabaseObject> databases; // guildID will find associated DatabaseObject
        //DatabaseObject data;
        public List<ulong> guilds;
        public Database(DiscordSocketClient client) {
            //data = new DatabaseObject(696808297805774888);
            databases = new();
            guilds = new();
            Directory.CreateDirectory(Config.DATABASE_DIRECTORY);
            foreach (SocketGuild guild in client.Guilds) {
                ulong guildID = guild.Id;
                guilds.Add(guildID);
                databases.Add(guildID, new DatabaseObject(guildID));
            }
        }
        public string fetchValue(string key, ulong guildID) {
            key = Validation.preProcessValue(key);
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.get(key);
        }
        public void removeValue(string key, ulong guildID) {
            key = Validation.preProcessValue(key);
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.remove(key);
        }
        public async Task iterateValue(string key, ulong guildID, bool updateDB = false) {
            key = Validation.preProcessValue(key);
            await initializeIterator(key, guildID);
            string value = fetchValue(key, guildID);
            int number;
            try {
                number = int.Parse(value);
            } catch (FormatException e) {
                Console.WriteLine(e.Message + "\nPossible database corruption detected, fix this ASAP!");
                return;
                //throw e;
            } catch (ArgumentNullException e) {
                Console.WriteLine(e.Message + "\nPossible database corruption detected, fix this ASAP!");
                return;
                //throw e;
            }
            databases.TryGetValue(guildID, out DatabaseObject? data);
            data.add(key, (++number).ToString());
            if (updateDB) {
                await data.updateSelf(guildID);
            }
        }

        public async Task setValue(string key, string value, ulong? guildID, bool updateDB = false) {
            if (guildID == null) { return; }
            value = Validation.preProcessValue(value);
            key = Validation.preProcessValue(key);
            databases.TryGetValue((ulong)guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.add(key, value);
            if (updateDB) {
                await targetDatabase.updateSelf((ulong)guildID);
            }
        }

        public async Task updateDB(ulong guildID) {
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            await targetDatabase.updateSelf(guildID);
        }


        public async Task initializeIterator(string key, ulong guildID, int startingValue = 1, bool updateDB = false) {
            key = Validation.preProcessValue(key);
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            if (targetDatabase.doesKeyExist(key)) {
                return;
            } else {
                targetDatabase.add(key, startingValue.ToString());
                if (updateDB) {
                    await targetDatabase.updateSelf(guildID);
                }
            }
        }
        public string getSerializedDB(ulong guildID) {
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.getSerializedSelf();
        }
        public void setSerializedDB(string serializedJSON, ulong guildID) {
            databases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.populateSelf(serializedJSON);
        }
    }
}