using Discord.WebSocket;

namespace StarBot {
    public class Database {
        Dictionary<ulong, DatabaseObject> guildDatabases; // guildID will find associated DatabaseObject
        Dictionary<string, DatabaseObject> otherDatabases;
        //DatabaseObject data;
        public List<ulong> guilds;
        public Database(DiscordSocketClient client) {
            //data = new DatabaseObject(696808297805774888);
            guildDatabases = new();
            guilds = new();
            otherDatabases = new();
            Directory.CreateDirectory(Config.DATABASE_DIRECTORY);
            foreach (SocketGuild guild in client.Guilds) {
                ulong guildID = guild.Id;
                guilds.Add(guildID);
                guildDatabases.Add(guildID, new DatabaseObject(guildID));
            }
            foreach (string path in Directory.GetFiles(Config.DATABASE_DIRECTORY)) {
                string name = Path.GetFileNameWithoutExtension(path);
                try {
                ulong guildID = ulong.Parse(name);
                if (!guilds.Contains(guildID)) {
                    otherDatabases.Add(name, new(name));
                }
                } catch {
                    otherDatabases.Add(name, new(name));
                }
            }
        }
        public void createDbIfNotExists(string dbKey) {
            if (!otherDatabases.TryGetValue(dbKey, out _)) {
                otherDatabases.Add(dbKey, new(dbKey));
            }
        }
        public void createDbIfNotExists(ulong dbKey) {
            if (!guildDatabases.TryGetValue(dbKey, out _)) {
                guildDatabases.Add(dbKey, new(dbKey));
            }
        }
        public string fetchValue(string key, ulong guildID) {
            key = Validation.preProcessValue(key);
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.get(key);
        }
        public string fetchValue(string key, string identifier) {
            key = Validation.preProcessValue(key);
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.get(key);
        }

        public string[]? getKeys(ulong guildID) {
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return default; }
            return targetDatabase.getAllKeys();
        }

        public string[]? getKeys(string identifier) {
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return default; }
            return targetDatabase.getAllKeys();
        }
        public void removeValue(string key, ulong guildID) {
            key = Validation.preProcessValue(key);
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.remove(key);
        }

        public void removeValue(string key, string identifier) {
            key = Validation.preProcessValue(key);
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
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
            guildDatabases.TryGetValue(guildID, out DatabaseObject? data);
            data.add(key, (++number).ToString());
            if (updateDB) {
                await data.updateSelf(guildID);
            }
        }

        public async Task iterateValue(string key, string identifier, bool updateDB = false) {
            key = Validation.preProcessValue(key);
            await initializeIterator(key, identifier);
            string value = fetchValue(key, identifier);
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
            otherDatabases.TryGetValue(identifier, out DatabaseObject? data);
            data.add(key, (++number).ToString());
            if (updateDB) {
                await data.updateSelf(identifier);
            }
        }

        public async Task setValue(string key, string value, ulong? guildID, bool updateDB = false) {
            if (guildID == null) { return; }
            value = Validation.preProcessValue(value);
            key = Validation.preProcessValue(key);
            guildDatabases.TryGetValue((ulong)guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.add(key, value);
            if (updateDB) {
                await targetDatabase.updateSelf((ulong)guildID);
            }
        }

        public async Task setValue(string key, string value, string? identifier, bool updateDB = false) {
            if (identifier == null) { return; }
            value = Validation.preProcessValue(value);
            key = Validation.preProcessValue(key);
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.add(key, value);
            if (updateDB) {
                await targetDatabase.updateSelf(identifier);
            }
        }

        public async Task updateDB(ulong guildID) {
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            await targetDatabase.updateSelf(guildID);
        }
        public async Task updateDB(string identifier) {
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            await targetDatabase.updateSelf(identifier);
        }


        public async Task initializeIterator(string key, ulong guildID, int startingValue = 1, bool updateDB = false) {
            key = Validation.preProcessValue(key);
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
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
        public async Task initializeIterator(string key, string identifier, int startingValue = 1, bool updateDB = false) {
            key = Validation.preProcessValue(key);
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            if (targetDatabase.doesKeyExist(key)) {
                return;
            } else {
                targetDatabase.add(key, startingValue.ToString());
                if (updateDB) {
                    await targetDatabase.updateSelf(identifier);
                }
            }
        }
        public string getSerializedDB(ulong guildID) {
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.getSerializedSelf();
        }

        public string getSerializedDB(string identifier) {
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return ""; }
            return targetDatabase.getSerializedSelf();
        }
        public void setSerializedDB(string serializedJSON, ulong guildID) {
            guildDatabases.TryGetValue(guildID, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.populateSelfSerialized(serializedJSON);
        }

        public void setSerializedDB(string serializedJSON, string identifier) {
            otherDatabases.TryGetValue(identifier, out DatabaseObject? targetDatabase);
            if (targetDatabase == null) { return; }
            targetDatabase.populateSelfSerialized(serializedJSON);
        }
    }
}