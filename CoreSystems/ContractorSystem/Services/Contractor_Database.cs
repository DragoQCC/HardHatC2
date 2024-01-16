using HardHatCore.ContractorSystem.Utilities;
using SQLite;

namespace HardHatCore.ContractorSystem.Services
{
    public class Contractor_Database
    {
        public static string ConnectionString = null;
        public static SQLiteAsyncConnection AsyncConnection = null;

        public static async Task InitDB()
        {
            try 
            {
                string baseFolderPath = HelperFunctions.GetBasePath();
                //make a folder named database at pathSplit[0] if one does not exist 
                if (!Directory.Exists(baseFolderPath + HelperFunctions.PlatPathSeperator + "database"))
                {
                    Directory.CreateDirectory(baseFolderPath + HelperFunctions.PlatPathSeperator + "database");
                }
                string databaseDirectory = baseFolderPath + "database";
                //make a db file named HardHarC2.db inside the databaseDirectory 
                if (!File.Exists(databaseDirectory + HelperFunctions.PlatPathSeperator + "HardHatContractorSystem.db"))
                {
                    File.Create(databaseDirectory + HelperFunctions.PlatPathSeperator + "HardHatContractorSystem.db").Close();
                }
                //create a connection string to the database file
                ConnectionString = databaseDirectory + HelperFunctions.PlatPathSeperator + "HardHatContractorSystem.db";
                Thread.Sleep(1000);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //create a connection to the database file
        public static async Task ConnectDb()
        {
            try
            {
                if (ConnectionString == null)
                {
                   await InitDB();
                }
                AsyncConnection ??= new SQLiteAsyncConnection(ConnectionString);
                Console.WriteLine("Connected to sqlite DB");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to sqlite DB: " + ex.Message);
            }
        }

        public static async Task CreateTable(Type tableType)
        {
            if (AsyncConnection == null)
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.CreateTableAsync(tableType);
        }

        public static async Task InsertItem(object item, Type itemType)
        {
            if (AsyncConnection == null)
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.InsertAsync(item, itemType);
            //ActivationContext.Trigger("OnDatabaseInsert",);
        }

        public static async Task UpdateItem(object item, Type itemType)
        {
            if (AsyncConnection == null)
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.UpdateAsync(item, itemType);
        }

        public static async Task DeleteItem(object item)
        {
            if (AsyncConnection == null)
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.DeleteAsync(item);
        }

        public static async Task<List<T>> GetItemsOfType<T>(Type ItemDAOType)
        {
            if (AsyncConnection == null)
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            var tableMappings = AsyncConnection.TableMappings;
            var tableMapping = tableMappings.FirstOrDefault(x => x.MappedType == ItemDAOType);
            var tableName = tableMapping.TableName;
            var query = $"SELECT * FROM {tableName}";
            var args = new object[] { };
            var items = await AsyncConnection.QueryAsync(tableMapping, query, args);
            //convert the items to the correct type and return them
            List<T> returnItems = new List<T>();
            foreach (var item in items)
            {
                // Use dynamic casting along with the implicit operator
                T convertedItem = (T)(dynamic)item;
                returnItems.Add(convertedItem);
            }
            return returnItems;
        }

        public static async Task<T> GetItemById<T>(string id)
        {
            if (AsyncConnection == null) 
            {
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            var tableMappings = AsyncConnection.TableMappings;
            var tableMapping = tableMappings.FirstOrDefault(x => x.MappedType == typeof(T));
            var foundItem = await AsyncConnection.GetAsync(id, tableMapping);
            return (T)(dynamic)foundItem;
        }
    }
}
