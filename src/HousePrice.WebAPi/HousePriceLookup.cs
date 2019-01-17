using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using MongoDB.Driver;
using Serilog;

namespace HousePrice.WebAPi
{
    public interface IHousePriceLookup
    {
        Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius, int skip);
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

        public async Task<PagedResult<HousePrice>> GetPagedResult(PostcodeData postcodeInfo, double radius, int skip)
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

                        var prices = await query.Sort(sort).Skip(skip).Limit(51).ToListAsync();
                        var result = new PagedResult<HousePrice>(prices.Count==51, prices.Skip(0).Take(50).ToArray() );

                        Log.Information($"Request to mongo successful, retrieved {prices.Count} records");

                        return result;

                    });



                return list;
            }
            catch (Exception ex)
            {
                Log.Information($"Error occured accessing Mongodb: {ex.Message}");
            }

            return new PagedResult<HousePrice>(false, new HousePrice[0]);;
        }

        public async Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius, int skip)
        {
            Log.Information("Starting retrieval postcode retrieval...");

            var postcodeInfo = LogAccessTime(_postcodeLookup.GetByPostcode, postcode, "Postcode lookup of lat and long took {0} milliseconds");

            if (postcodeInfo?.Longitude != null && postcodeInfo.Latitude != null)
            {
                var timer = Stopwatch.StartNew();
                var stuff =  await GetPagedResult(postcodeInfo, radius, skip);
                Log.Information($"Time to get transaction records was {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()}");
                return stuff;
            }

            return new PagedResult<HousePrice>(false, new HousePrice[0]);
        }
    }
}