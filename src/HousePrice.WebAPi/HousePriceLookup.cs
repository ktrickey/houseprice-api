using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using MongoDB.Driver;
using Serilog;

namespace HousePrice.WebAPi
{
    public interface IHousePriceLookup
    {
        Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius);
    }

    public class HousePriceLookup : IHousePriceLookup
    {
        private readonly IMongoContext _mongoContext;
        private readonly IPostcodeLookup _postcodeLookup;

        public HousePriceLookup(IHousePriceLookupConfig config)
        {
            _mongoContext = config.MongoContext;
            _postcodeLookup = config.PostcodeLookup;
        }

        private T2 LogAccessTime<T1, T2>( Func<T1, T2> funcToTime, T1 arg, string logString)
        {
            var stopWatch = Stopwatch.StartNew();
            var result = funcToTime(arg);
            stopWatch.Stop();
            var elapsed = stopWatch.ElapsedMilliseconds;
            Log.Information(string.Format(logString, elapsed.ToString()));

            return result;
        }

        public async Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius)
        {
            Log.Information("Starting retrieval postcode retrieval...");

            var postcodeInfo = LogAccessTime(_postcodeLookup.GetByPostcode, postcode, "Postcode lookup of lat and long took {0} milliseconds");
            var timer = Stopwatch.StartNew();
            if (postcodeInfo?.Longitude != null && postcodeInfo.Latitude != null)
            {
                try
                {
                    Log.Information("Sending request to Mongo...");
                    var list = await _mongoContext.ExecuteAsync<HousePrice, PagedResult<HousePrice>>("Transactions",
                        async (activeCollection) =>
                    {
                        var locationQuery =
                            new FilterDefinitionBuilder<HousePrice>().GeoWithinCenterSphere(
                                tag => tag.Location,
                                postcodeInfo.Longitude.Value,
                                postcodeInfo.Latitude.Value,
                                (radius / 1000) / 6371);

                        var sort = new SortDefinitionBuilder<HousePrice>().Descending(x => x.TransferDate);

                        var query = activeCollection.Find(locationQuery);

                        var result = new PagedResult<HousePrice>(100, await query.Sort(sort).ToListAsync());

                        Log.Information("Request to mongo successful");

                        return result;

                    });

                    timer.Stop();

                    Log.Information($"Time to get transaction records was {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()}");
                    return list;
                }
                catch (Exception ex)
                {
                    Log.Information($"Error occured accessing Mongodb: {ex.Message}");
                }
            }

            return new PagedResult<HousePrice>(0, new HousePrice[0]);
        }
    }
}