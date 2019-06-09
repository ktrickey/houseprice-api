using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HousePrice.Infrastructure.Data
{
    public static class MongoContextExtensions
    {
        public static async Task AddIndex(this MongoContext mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;

            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Geo2DSphere(x => x.Location),
                new CreateIndexOptions()
                {
                    Name = "LocationIndex"
                });

            await mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                    mongoContext._logger.LogInformation("Checking LocationIndex");
                    if (!await IndexExists(mongoContext, "LocationIndex"))
                    {
                      //  Log.Information("Creating LocationIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddPostcodeIndex(this MongoContext mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Ascending(x => x.Postcode),
                new CreateIndexOptions() {Name = "PostcodeIndex"});

            await mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                    mongoContext._logger.LogInformation("Checking PostcodeIndex");
                    if (!await IndexExists(mongoContext, "PostcodeIndex"))
                    {
                        mongoContext._logger.LogInformation("Creating PostcodeIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddTransferDateIndex(IMongoContext mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Ascending(x => x.TransferDate),
                new CreateIndexOptions()
                {
                    Name = "TransferDateIndex"
                });

            await mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                   // Log.Information("Checking TransferDateIndex");
                    if (!await IndexExists(mongoContext, "TransferDateIndex"))
                    {
                     //   Log.Information("Creating TransferDateIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        private static async Task<bool> IndexExists(IMongoContext mongoContext, string indexName)
        {
            return await mongoContext.ExecuteAsync<HousePriceTransaction, bool>("Transactions", async (collection) =>
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
}