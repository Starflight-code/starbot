using Newtonsoft.Json;

namespace StarBot {
    internal class DatabaseObject {
        private Dictionary<string, string> data;
        public DatabaseObject() {
            data = new Dictionary<string, string>();
            populateSelf();
        }
        public bool equals(DatabaseObject otherObject) {
            return otherObject.data.Equals(data);
        }
        public void add(string key, string value) {
            try {
                data.Add(key, value);
            }
            catch (ArgumentException) {
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

        public bool populateSelf() {
            string[] lines;
            try { lines = File.ReadAllLines(Statics.buildPath(Directory.GetCurrentDirectory() + "\\database.db")); }
            catch (FileNotFoundException) {
                return false;
            }
            string databaseJson = "";
            for (int i = 0; i < lines.Length; i++) {
                if (i != 0) {
                    databaseJson += "\n";
                }
                databaseJson += lines[i];
            }
            Dictionary<string, string>? db = JsonConvert.DeserializeObject<Dictionary<string, string>>(databaseJson);
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
        public bool populateSelf(string serializedJSON) {
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
        public async Task updateSelf() {
            string database = JsonConvert.SerializeObject(data);
            await File.WriteAllLinesAsync(Statics.buildPath(Directory.GetCurrentDirectory() + "\\database.db"), database.Split('\n'));
        }

        public string getSerializedSelf() {
            return JsonConvert.SerializeObject(data);
        }
    }
}
