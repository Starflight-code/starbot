namespace StarBot {
    internal class Database {
        DatabaseObject data;
        public Database() {
            data = new DatabaseObject();
        }
        public string fetchValue(string key) {
            key = Statics.preProcessValue(key);
            return data.get(key);
        }
        public void removeValue(string key) {
            key = Statics.preProcessValue(key);
            data.remove(key);
        }
        public async Task iterateValue(string key, bool updateDB = false) {
            key = Statics.preProcessValue(key);
            await initializeIterator(key);
            string value = fetchValue(key);
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
            data.add(key, (++number).ToString());
            if (updateDB) {
                await data.updateSelf();
            }
        }

        public async Task setValue(string key, string value, bool updateDB = false) {
            value = Statics.preProcessValue(value);
            data.add(key, value);
            if (updateDB) {
                await data.updateSelf();
            }
        }

        public async Task updateDB() {
            await data.updateSelf();
        }


        public async Task initializeIterator(string key, int startingValue = 1, bool updateDB = false) {
            key = Statics.preProcessValue(key);
            if (data.doesKeyExist(key)) {
                return;
            } else {
                data.add(key, startingValue.ToString());
                if (updateDB) {
                    await data.updateSelf();
                }
            }
        }
        public string getSerializedDB() {
            return data.getSerializedSelf();
        }
        public void setSerializedDB(string serializedJSON) {
            data.populateSelf(serializedJSON);
        }
    }
}