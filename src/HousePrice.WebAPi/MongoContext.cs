using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HousePrice.WebAPi
{
	public class MongoContext
	{

		private readonly IMongoClient _client;
		private readonly IMongoDatabase _database;

		public MongoContext(string connectionString, string databaseName)
		{
			_client = GetClient(connectionString);
			_database = GetDatabase(databaseName);
		}


		private IMongoClient GetClient(string mongoConnection)
		{
			return new MongoClient(mongoConnection);
		}

		private IMongoDatabase GetDatabase( string database, MongoDatabaseSettings settings = null)
		{
			return  _client.GetDatabase(database, settings);
		}

		public IMongoCollection<T> GetCollection<T>(string collection)
		{
			return _database.GetCollection<T>(collection);
		}
		public IMongoCollection<T> GetCollection<T>(string databaseName, string collection)
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
		public async Task<TOut> ExecuteAsync<TIn, TOut>(IMongoCollection<TIn> collection, Func<IMongoCollection<TIn>, Task<TOut>> func)
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
