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


        public async Task<PagedResult<HousePrice>> GetLookups(string postcode, double radius)
        {
			Console.WriteLine("Starting retrieval...");
	        var timer = Stopwatch.StartNew();
            var postcodeInfo = PostcodeLookup.GetByPostcode(postcode);
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

			        Console.WriteLine(TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString() );
			        return list;
		        }
		        catch (Exception ex)
		        {
			        int i = 0;
			        //do something;
		        }
	        }

	        return new PagedResult<HousePrice>(0, new HousePrice[0]);
        }
    }
}