using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Serilog;

namespace HousePrice.Api.Services
{
	public interface IImporter
	{
		Task Import(string name, Stream csvStream);
	}
	public class Importer : IImporter
	{
		private readonly MongoContext _mongoContext;
		public Importer()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();

			
			_mongoContext = new MongoContext(configuration["connectionString"], "HousePrice");
		}

		public async Task Import(string name, Stream csvStream)
		{
			await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
			{
				using (var streamReader = new StreamReader(csvStream))
				{
					using (var csvReader = new CsvHelper.CsvReader(streamReader))
					{

						csvReader.Configuration.HasHeaderRecord = false;
						csvReader.Configuration.RegisterClassMap<HousePriceMap>();
						var batch = new List<HousePrice>();

						while (csvReader.Read())
						{
							try
							{
								var record = csvReader.GetRecord<HousePrice>();
								record.Postcode = record.Postcode.Replace(" ", String.Empty);
								var locationData = PostcodeLookup.GetByPostcode(record.Postcode);
								record.Location = locationData?.Latitude != null && locationData?.Longitude != null
									? new Location(locationData?.Latitude, locationData?.Longitude)
									: null;
								batch.Add(record);
							}
							catch (Exception e)
							{
								Log.Error(e.Message);
								throw;
							}

						}
						if (batch.Count % 1000 == 0)
						{
							Console.WriteLine($"{name}: {batch.Count}");

						}
						await collection.InsertManyAsync(batch);
					}

				}
			});
		}

		public async Task AddIndex()
		{
			var housePriceBuilder = Builders<HousePrice>.IndexKeys;
			var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Geo2DSphere(x=>x.Location));

			await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
			{
				await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
			});
		}

		public async Task AddPostcodeIndex()
		{
			var housePriceBuilder = Builders<HousePrice>.IndexKeys;
			var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x=>x.Postcode));

			await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
			{
				await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
			});
		}

		public async Task AddTransferDateIndex()
		{
			var housePriceBuilder = Builders<HousePrice>.IndexKeys;
			var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x=>x.TransferDate));

			await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
			{
				await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
			});

		}

		public void GetMatches(string postcode, double radius)
		{
			//var client = new MongoClient("mongodb://localhost:32768");
			//var database = client.GetDatabase("HousePrice");
			////     database.DropCollection("Transactions");
			//var collection = database.GetCollection<HousePrice>("Transactions");
			//var postcodeLocation = PostcodeLookup.GetByPostcode("CB233NY");
			//var builder = Builders<HousePrice>.Filter;
			//var filter = builder.NearSphere(x => x.Location, postcodeLocation.Longitude.Value, postcodeLocation.Latitude.Value, radius);

			//FindOptions<HousePrice> options = new FindOptions<HousePrice> { Limit = 25};
			//var stuff = collection.FindAsync(filter, options).ToListAsync();
		}
        
	}
	// ReSharper disable once ClassNeverInstantiated.Global


}