using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class Conexion
    {
        private static IMongoDatabase _database;
        private static MongoClient _client;
        private static string _lastError = "";

        public static string ConnectionString = "mongodb://localhost:27017";
        public static string DatabaseName = "monster_university_neatbeans";

        public static string LastError => _lastError;

        public static IMongoDatabase Database
        {
            get
            {
                if (_database == null || !TestConnection())
                {
                    if (!CreateConnection())
                    {
                        throw new InvalidOperationException($"No se pudo conectar a MongoDB: {_lastError}");
                    }
                }
                return _database;
            }
        }

        // Método similar al de Java para crear conexión
        public static bool CreateConnection()
        {
            try
            {
                Console.WriteLine($"Intentando conectar a: {ConnectionString}");

                _client = new MongoClient(ConnectionString);
                Console.WriteLine("MongoClient creado");

                // Validar conexión inmediatamente (similar a Java)
                var dbNames = _client.ListDatabaseNames().ToList();
                Console.WriteLine("Ping a MongoDB exitoso");

                _database = _client.GetDatabase(DatabaseName);
                _lastError = "";

                Console.WriteLine($"Conexión exitosa a MongoDB. Base de datos: {_database.DatabaseNamespace.DatabaseName}");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Console.WriteLine($"Error al conectar a MongoDB: {ex.Message}");

                // Intentar alternativa directa
                try
                {
                    Console.WriteLine("Intentando conexión alternativa...");
                    _client = new MongoClient("mongodb://localhost:27017/?serverSelectionTimeoutMS=5000");
                    _database = _client.GetDatabase(DatabaseName);
                    _lastError = "";
                    Console.WriteLine("Conexión alternativa exitosa");
                    return true;
                }
                catch (Exception ex2)
                {
                    _lastError = ex2.Message;
                    Console.WriteLine($"Error en conexión alternativa: {ex2.Message}");
                    return false;
                }
            }
        }

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

        // Método sincrono para probar conexión (versión corregida)
        public static bool TestConnection()
        {
            try
            {
                if (_client == null)
                    return false;

                // Versión síncrona
                _client.ListDatabaseNames().FirstOrDefault();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Método asíncrono alternativo si lo necesitas
        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (_client == null)
                    return false;

                await _client.ListDatabaseNamesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Método para cerrar conexión (similar a Java)
        public static void CloseConnection()
        {
            try
            {
                // En .NET, MongoClient no tiene método Close explícito
                // Se maneja con using o se deja que el GC lo limpie
                _database = null;
                _client = null;
                Console.WriteLine("Conexión a MongoDB cerrada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar la conexión: {ex.Message}");
            }
        }
    }
}