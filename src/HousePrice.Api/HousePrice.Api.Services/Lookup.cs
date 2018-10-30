using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Serilog;

namespace HousePrice.Api.Services
{
	public class PagedResult<T>
	{
		public PagedResult(long totalRows, IEnumerable<T> results)
		{
			_totalRows = totalRows;
			_results = results;
		}
		private readonly long _totalRows;
		private readonly IEnumerable<T> _results;
		public long TotalRows => _totalRows;
		public IEnumerable<T> Results => _results;
	}
    public interface ILookup
    {
	    Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius);
    }

    public class Lookup : ILookup
    {
	    private readonly MongoContext _mongoContext;
	    private string _postcodeDataLocation;

	    public Lookup()
        {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();

			
			_mongoContext = new MongoContext(configuration["connectionString"], "HousePrice");
	        _postcodeDataLocation = configuration["postcodeDataDirectory"];
        }

	    private T2 LogAccessTime<T1, T2>(Func<T1, T2> funcToTime, T1 arg, string logString)
	    {
		    var stopWatch = Stopwatch.StartNew();
		    var result = funcToTime(arg);
		    stopWatch.Stop();
		    var elapsed = stopWatch.ElapsedMilliseconds;
		    Log.Information(string.Format(logString, elapsed));

		    return result;
	    }

	    public async Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius)
        {
			Console.WriteLine("Starting retrieval...");
	        var timer = Stopwatch.StartNew();
            var postcodeInfo = LogAccessTime(PostcodeLookup.GetByPostcode, postcode, "Postcode lookup of lat and long took {0} milliseconds");
	        if (postcodeInfo?.Longitude != null && postcodeInfo?.Latitude != null)
	        {
		        try
		        {
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

				        return new PagedResult<HousePrice>(100, await query.Sort(sort).Skip(0).Limit(25).ToListAsync());
				        
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