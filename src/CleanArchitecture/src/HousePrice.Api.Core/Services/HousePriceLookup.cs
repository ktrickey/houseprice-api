using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;
using HousePrice.Api.Core.SharedKernel;

namespace HousePrice.Api.Core.Services
{
    public interface IHousePriceLookup
    {
        Task<PagedResult<HousePriceTransaction>> GetLookups(string postcode, double radius, int skip);
    }

    public class HousePriceLookup : IHousePriceLookup
    {

        private readonly IRepository _mongoContext;
        private readonly IPostcodeLookup _postcodeLookup;

        public HousePriceLookup(IHousePriceLookupConfig config)
        {
            _mongoContext = config.MongoContext;
            _postcodeLookup = config.PostcodeLookup;
        }

//        private T2 LogAccessTime<T1, T2>( Func<T1, T2> funcToTime, T1 arg, string logString)
//        {
//            var stopWatch = Stopwatch.StartNew();
//            var result = funcToTime(arg);
//            stopWatch.Stop();
//            var elapsed = stopWatch.ElapsedMilliseconds;
//            //Log.Information(string.Format(logString, elapsed.ToString()));
//
//            return result;
//        }

        public async Task<PagedResult<HousePriceTransaction>> GetPagedResult( PostcodeData postcodeInfo, double radius, int skip)
        {
            try
            {
                //Log.Information("Sending request to Mongo...");

               var location = new LocationFilter(new Location(postcodeInfo.Latitude, postcodeInfo.Longitude), radius );

               return await _mongoContext.FindWithinArea<HousePriceTransaction>(location, x => x.TransferDate, skip);

            }
            catch (Exception ex)
            {
              //  Log.Information($"Error occured accessing Mongodb: {ex.Message}");
            }

            return new PagedResult<HousePriceTransaction>(false, new HousePriceTransaction[0]);;
        }

        public async Task<PagedResult<HousePriceTransaction>> GetLookups(string postcode, double radius, int skip)
        {
           // Log.Information("Starting retrieval postcode retrieval...");

            var postcodeInfo = await _postcodeLookup.GetByPostcode( postcode);

            if (postcodeInfo?.Longitude != null && postcodeInfo.Latitude != null)
            {
                var timer = Stopwatch.StartNew();
                var stuff =  await GetPagedResult(postcodeInfo, radius, skip);
             //   Log.Information($"Time to get transaction records was {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()}");
                return stuff;
            }

            return new PagedResult<HousePriceTransaction>(false, new HousePriceTransaction[0]);
        }
    }
}