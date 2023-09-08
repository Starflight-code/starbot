namespace StarBot
{
    internal class Database
    {
        DatabaseObject data;
        public Database()
        {
            data = new DatabaseObject();
        }
        public string fetchValue(string key)
        {
            key = Statics.preProcessValue(key);
            return data.get(key);
            /*
            string toFetch = Directory.GetCurrentDirectory() + "\\values\\" + key;
            toFetch = Statics.buildPath(toFetch);
            if (System.IO.File.Exists(toFetch)) {
                return await System.IO.File.ReadAllTextAsync(toFetch);
            } else {
                return "";
            }*/
        }
        public void removeValue(string key)
        {
            key = Statics.preProcessValue(key);
            data.remove(key);
        }
        public async Task iterateValue(string key, bool updateDB = false)
        {
            key = Statics.preProcessValue(key);
            await initializeIterator(key);
            string value = fetchValue(key);
            /*string toFetch = Directory.GetCurrentDirectory() + "\\values\\" + key;
            toFetch = Statics.buildPath(toFetch);
            if (System.IO.File.Exists(toFetch)) {
                string text = await System.IO.File.ReadAllTextAsync(toFetch);
                int number = 0;
            */
            int number;
            try
            {
                number = int.Parse(value);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message + "\nPossible database corruption detected, fix this ASAP!");
                return;
                //throw e;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message + "\nPossible database corruption detected, fix this ASAP!");
                return;
                //throw e;
            }
            data.add(key, (++number).ToString());
            if (updateDB)
            {
                await data.updateSelf();
            }
            //await System.IO.File.WriteAllTextAsync(toFetch, (++number).ToString());
        }

        public async Task setValue(string key, string value, bool updateDB = false)
        {
            value = Statics.preProcessValue(value);
            data.add(key, value);
            if (updateDB)
            {
                await data.updateSelf();
            }
            /*string toFetch = Directory.GetCurrentDirectory() + "\\values\\" + value;
            toFetch = Statics.buildPath(toFetch);
            if (System.IO.File.Exists(toFetch)) {
                await System.IO.File.WriteAllTextAsync(toFetch, setTo);
            } else {
                var file = System.IO.File.Create(toFetch);
                file.Dispose();
                await System.IO.File.WriteAllTextAsync(toFetch, setTo);
            }*/
        }

        public async Task updateDB()
        {
            await data.updateSelf();
        }


        public async Task initializeIterator(string key, int startingValue = 1, bool updateDB = false)
        {
            key = Statics.preProcessValue(key);
            if (data.doesKeyExist(key))
            {
                return;
            }
            else
            {
                data.add(key, startingValue.ToString());
                if (updateDB)
                {
                    await data.updateSelf();
                }
            }

            /*string toFetch = Directory.GetCurrentDirectory() + "\\values\\" + value;
            toFetch = Statics.buildPath(toFetch);
            if (System.IO.File.Exists(toFetch)) {
                return;
            } else {
                var file = System.IO.File.Create(toFetch);
                file.Dispose();
                await System.IO.File.WriteAllTextAsync(toFetch, startingValue.ToString());
            }*/
        }
        public string getSerializedDB()
        {
            return data.getSerializedSelf();
        }
        public void setSerializedDB(string serializedJSON)
        {
            data.populateSelf(serializedJSON);
        }
    }
}
