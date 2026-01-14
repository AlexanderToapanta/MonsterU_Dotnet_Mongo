using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class Conexion
    {
        private static IMongoDatabase _database;
        private static MongoClient _client;

        // Cadena de conexión para MongoDB
        public static string ConnectionString = "mongodb://localhost:27017";

        // Nombre de la base de datos
        public static string DatabaseName = "monster_university_neatbeans";

        // Propiedad para obtener la base de datos (patrón singleton)
        public static IMongoDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    _client = new MongoClient(ConnectionString);
                    _database = _client.GetDatabase(DatabaseName);
                }
                return _database;
            }
        }

        // Método para obtener una colección específica
        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

        // Método para cambiar la base de datos en tiempo de ejecución
        public static void SetDatabase(string databaseName)
        {
            _client = new MongoClient(ConnectionString);
            _database = _client.GetDatabase(databaseName);
        }

        // Método para probar la conexión
        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _client?.ListDatabaseNamesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}