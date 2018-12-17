using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
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
        private readonly IMongoContext _mongoContext;
        private readonly IPostcodeLookup _postcodeLookup;

        public Importer(IMongoContext context, IPostcodeLookup postcodeLookup)
        {
            _mongoContext = context;
            _postcodeLookup = postcodeLookup;
        }

        public async Task Import(HousePrice record)
        {
            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
            {
                record.Postcode = record.Postcode.Replace(" ", String.Empty);
                var locationData = _postcodeLookup.GetByPostcode(record.Postcode);
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

        public static async Task AddIndex(IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePrice>.IndexKeys;

            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Geo2DSphere(x => x.Location),
                new CreateIndexOptions()
                {
                    Name = "LocationIndex"
                });

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) =>
                {
                    if (!await IndexExists(_mongoContext, "LocationIndex"))
                    {
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddPostcodeIndex(IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePrice>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x => x.Postcode),
                new CreateIndexOptions() {Name = "PostcodeIndex"});

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) =>
                {
                    if (!await IndexExists(_mongoContext, "PostcodeIndex"))
                    {
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddTransferDateIndex(IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePrice>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Ascending(x => x.TransferDate),
                new CreateIndexOptions()
                {
                    Name = "TransferDateIndex"
                });

            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions",
                async (collection) =>
                {
                    if (!await IndexExists(_mongoContext, "TransferDateIndex"))
                    {
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        private static async Task<bool> IndexExists(IMongoContext _mongoContext, string indexName)
        {
            return await _mongoContext.ExecuteAsync<HousePrice, bool>("Transactions", async (collection) =>
            {
                var collectionManager = collection.Indexes;
                var indexes = await collectionManager.ListAsync();

                while (indexes.MoveNext())
                {
                    var currentIndex = indexes.Current;
                    foreach (var doc in currentIndex)
                    {
                        BsonValue val;
                        bool ok = doc.TryGetValue("name", out val);

                        if (ok) return true;
                    }
                }

                return false;
            });
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
}