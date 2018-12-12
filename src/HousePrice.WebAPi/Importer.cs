using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Serilog;

namespace HousePrice.WebAPi
{
    public interface IImporter
    {
        Task Import(string name, Stream csvStream);
        Task Import(HousePrice priceRecord);
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


            _mongoContext = new MongoContext($"mongodb://{configuration["connectionString"]}", "HousePrice");
        }

        public async Task Import(HousePrice record)
        {
            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
            {
                record.Postcode = record.Postcode.Replace(" ", String.Empty);
                var locationData = PostcodeLookup.GetByPostcode(record.Postcode);
                record.Location = locationData?.Latitude != null && locationData?.Longitude != null
                    ? new Location(locationData?.Latitude, locationData?.Longitude)
                    : null;

                try
                {
                    await collection.InsertOneAsync(record);
                }
                catch (MongoException ex)
                {
                    Log.Warning(ex.Message);
                    throw;
                }
            });
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
            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Geo2DSphere(x => x.Location));

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) => { await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false); });
        }

        public async Task AddPostcodeIndex()
        {
            var housePriceBuilder = Builders<HousePrice>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x => x.Postcode));

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) => { await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false); });
        }

        public async Task AddTransferDateIndex()
        {
            var housePriceBuilder = Builders<HousePrice>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x => x.TransferDate));

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) => { await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false); });
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
}