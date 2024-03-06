using Newtonsoft.Json;

namespace StarBot {
    internal class DatabaseObject {
        private Dictionary<string, string> data;
        public DatabaseObject(ulong guildID) {
            data = new Dictionary<string, string>();
            populateSelf(guildID);
        }
        public DatabaseObject(string identifier) {
            data = new Dictionary<string, string>();
            populateSelf(identifier);
        }
        public bool equals(DatabaseObject otherObject) {
            return otherObject.data.Equals(data);
        }
        public void add(string key, string value) {
            try {
                data.Add(key, value);
            } catch (ArgumentException) {
                data.Remove(key);
                data.Add(key, value);
            }

        }
        public void remove(string key) {
            data.Remove(key);
        }
        public void clear() {
            data.Clear();
        }
        public string get(string key) {
            data.TryGetValue(key, out string? value);
            if (value == null) {
                return "";
            }
            return value;
        }
        public bool doesKeyExist(string key) {
            return data.TryGetValue(key, out string? _);
        }

        public string[] getAllKeys() {
            return data.Keys.ToArray();
        }



        public bool populateSelf(ulong guildID) {
            if (!File.Exists(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"{guildID}.db"))) {
                return false;
            };

            string dbString = File.ReadAllText(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"{guildID}.db"));
            Dictionary<string, string>? db = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbString);
            if (db == null) {
                return false;
            }

            foreach (string key in db.Keys) {
                db.TryGetValue(key, out string? value);
                add(key, value == null ? "" : value); // sets null values to 0 size strings
            }
            return true;
        }
        public bool populateSelf(string identifier) {
            if (!File.Exists(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"{identifier}.db"))) {
                return false;
            };

            string dbString = File.ReadAllText(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"{identifier}.db"));
            Dictionary<string, string>? db = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbString);
            if (db == null) {
                return false;
            }

            foreach (string key in db.Keys) {
                db.TryGetValue(key, out string? value);
                add(key, value == null ? "" : value); // sets null values to 0 size strings
            }
            return true;
        }
        public bool populateSelfSerialized(string serializedJSON) {
            Dictionary<string, string>? db = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedJSON);
            if (db == null) {
                return false;
            }

            foreach (string key in db.Keys) {
                db.TryGetValue(key, out string? value);
                if (value == null) {
                    value = "";
                }
                add(key, value);
            }
            return true;
        }
        public async Task updateSelf(ulong guildID) {
            string database = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"/{guildID}.db"), database);
        }
        public async Task updateSelf(string identifier) {
            string database = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(Compatiblity.buildPath(Config.DATABASE_DIRECTORY + $"/{identifier}.db"), database);
        }

        public string getSerializedSelf() {
            return JsonConvert.SerializeObject(data);
        }
    }
}