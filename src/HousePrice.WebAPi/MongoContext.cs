using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HousePrice.WebAPi
{
    public interface IMongoContext
    {
        Task<TOut> ExecuteAsync<TIn, TOut>(string collectionName,Func<IMongoCollection<TIn>, Task<TOut>> func);
        Task<TOut> ExecuteAsync<TIn, TOut>(string databaseName, string collectionName,Func<IMongoCollection<TIn>, Task<TOut>> func);
        Task<TOut> ExecuteAsync<TOut>(IMongoDatabase database, Func<IMongoDatabase, Task<TOut>> func);
        Task ExecuteActionAsync<TIn>(string collectionName,  Func<IMongoCollection<TIn>, Task> action);
        Task ExecuteActionAsync<TIn>(IMongoCollection<TIn> collection,  Func<IMongoCollection<TIn>, Task> action);
    }

    public interface IMongoConnection
    {
        string ConnectionString { get; }
        string DatabaseName { get; }

    }

    public class MongoConnection : IMongoConnection
    {
        public MongoConnection(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }
        public string ConnectionString { get; }
        public string DatabaseName { get; }
    }

    public class MongoContext : IMongoContext
    {

        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoContext(IMongoConnection connection)
        {
            _client = GetClient(connection.ConnectionString);
            _database = GetDatabase(connection.DatabaseName);
        }

        private IMongoClient GetClient(string mongoConnection)
        {
            return new MongoClient(mongoConnection);
        }

        private IMongoDatabase GetDatabase( string database, MongoDatabaseSettings settings = null)
        {
            return  _client.GetDatabase(database, settings);
        }

        private IMongoCollection<T> GetCollection<T>(string collection)
        {
            return _database.GetCollection<T>(collection);
        }

        private IMongoCollection<T> GetCollection<T>(string databaseName, string collection)
        {
            return GetDatabase(databaseName).GetCollection<T>(collection);
        }
        
        public async Task<TOut> ExecuteAsync<TIn, TOut>(string collectionName,Func<IMongoCollection<TIn>, Task<TOut>> func)
        {
            var collection = GetCollection<TIn>(collectionName);
            return await ExecuteAsync(collection, func);
        }
        
        public async Task<TOut> ExecuteAsync<TIn, TOut>(string databaseName, string collectionName,Func<IMongoCollection<TIn>, Task<TOut>> func)
        {
            var collection = GetCollection<TIn>(databaseName,collectionName);
            return await ExecuteAsync(collection, func);
        }

        private async Task<TOut> ExecuteAsync<TIn, TOut>(IMongoCollection<TIn> collection, Func<IMongoCollection<TIn>, Task<TOut>> func)
        {
            return await func(collection);
        }
        
        public async Task<TOut> ExecuteAsync<TOut>(IMongoDatabase database, Func<IMongoDatabase, Task<TOut>> func)
        {
            return await func(database);
        }

        public async Task ExecuteActionAsync<TIn>(string collectionName,  Func<IMongoCollection<TIn>, Task> action)
        {
            var collection = GetCollection<TIn>(collectionName);
            await action(collection);
        }

        public async Task ExecuteActionAsync<TIn>(IMongoCollection<TIn> collection,  Func<IMongoCollection<TIn>, Task> action)
        {
            await action(collection);
        }

    }


}
