using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HousePrice.Infrastructure.Data
{
    public static class MongoContextExtensions
    {
        public static async Task AddIndex(this IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;

            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Geo2DSphere(x => x.Location),
                new CreateIndexOptions()
                {
                    Name = "LocationIndex"
                });

            await _mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                    //Log.Information("Checking LocationIndex");
                    if (!await IndexExists(_mongoContext, "LocationIndex"))
                    {
                      //  Log.Information("Creating LocationIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddPostcodeIndex(this IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Ascending(x => x.Postcode),
                new CreateIndexOptions() {Name = "PostcodeIndex"});

            await _mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                    //Log.Information("Checking PostcodeIndex");
                    if (!await IndexExists(_mongoContext, "PostcodeIndex"))
                    {
                        //Log.Information("Creating PostcodeIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        public static async Task AddTransferDateIndex(IMongoContext _mongoContext)
        {
            var housePriceBuilder = Builders<HousePriceTransaction>.IndexKeys;
            var indexModel = new CreateIndexModel<HousePriceTransaction>(housePriceBuilder.Ascending(x => x.TransferDate),
                new CreateIndexOptions()
                {
                    Name = "TransferDateIndex"
                });

            await _mongoContext.ExecuteActionAsync<HousePriceTransaction>("Transactions",
                async (collection) =>
                {
                   // Log.Information("Checking TransferDateIndex");
                    if (!await IndexExists(_mongoContext, "TransferDateIndex"))
                    {
                     //   Log.Information("Creating TransferDateIndex");
                        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    }
                });
        }

        private static async Task<bool> IndexExists(IMongoContext _mongoContext, string indexName)
        {
            return await _mongoContext.ExecuteAsync<HousePriceTransaction, bool>("Transactions", async (collection) =>
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