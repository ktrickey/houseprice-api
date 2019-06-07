using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace HousePrice.WebAPi
{
    public interface IImporter
    {
        Task Import(string name, Stream csvStream);
        Task Import(IEnumerable<HousePrice> priceRecord);
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

        public async Task Import(IEnumerable<HousePrice> records)
        {
            await _mongoContext.ExecuteActionAsync<HousePrice>("Transactions", async (collection) =>
            {
                var recordList = records.ToList();

                var locations = recordList.Select(r=>r.Postcode.Replace(" ", string.Empty)).Distinct().Select(r => _postcodeLookup.GetByPostcode(r))
                    .ToDictionary(r=>r.Postcode);

                foreach (var record in recordList)
                {
                    var locationData = locations[record.Postcode];
                    record.Location = locationData?.Latitude != null && locationData?.Longitude != null
                        ? new Location(locationData?.Latitude, locationData?.Longitude)
                        : null;
                }

                var adds = recordList.Where(r => r.Status == "A");
                var deletes = recordList.Where(r => r.Status == "D").Select(d=>d.TransactionId);

                try
                {
                    await collection.InsertManyAsync(adds);
                    await collection.DeleteManyAsync(x=>deletes.Contains(x.TransactionId));


//                    switch (record.Status)
//                    {
//                        case "A":
//                            await collection.InsertOneAsync(record);
//                            break;
//                        case "C":
//                            //await collection.UpdateOneAsync(x => x.TransactionId == record.TransactionId, record);
//                            break;
//                        case "D":
//                            await collection.DeleteOneAsync(x => x.TransactionId == record.TransactionId);
//                            break;
//                    }
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
                    Log.Information("Checking LocationIndex");
                    if (!await IndexExists(_mongoContext, "LocationIndex"))
                    {
                        Log.Information("Creating LocationIndex");
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
                    Log.Information("Checking PostcodeIndex");
                    if (!await IndexExists(_mongoContext, "PostcodeIndex"))
                    {
                        Log.Information("Creating PostcodeIndex");
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
                    Log.Information("Checking TransferDateIndex");
                    if (!await IndexExists(_mongoContext, "TransferDateIndex"))
                    {
                        Log.Information("Creating TransferDateIndex");
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