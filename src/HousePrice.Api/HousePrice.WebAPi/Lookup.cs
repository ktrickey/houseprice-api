using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HousePrice.WebAPi;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using RestSharp;
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
	    Task<PagedResult<WebAPi.HousePrice>> GetLookups(string postcode, double radius);
    }


	[Serializable]
	public class PostcodeData
	{

		public long Id { get; set; }

		public string Postcode {get; set; }

		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}

	public static class PostcodeLookup
	{
		private static RestClient lookupClient;
		static PostcodeLookup()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();
			var target = configuration["postcodelookupservicename"];
			Log.Information($"postcode target is {target}");
			lookupClient = new RestClient(target);

		}
		public static PostcodeData GetByPostcode(string postcode)
		{
			var response = lookupClient.Get<PostcodeData>(new RestRequest($"api/postcode/{WebUtility.UrlEncode(postcode)}"));
			Log.Information($"Response code: {response.StatusCode}, {response.Content}");
			if (response.IsSuccessful && response.StatusCode !=HttpStatusCode.NotFound)
			{
				var data = response.Data;
				Log.Information($"Postcode:{data.Postcode}, lat:{data.Latitude}, long:{data.Longitude}");

				return data;
			}
			else if (response.StatusCode == HttpStatusCode.NotFound)
			{
				Log.Information($"Postcode lookup for {postcode} not found");
				return null;
			}
			else 
			{
				Log.Error($"Request failed, response code: {response.StatusCode}, {response.ErrorMessage}");
				throw new HttpRequestException("Failed to access the postcode lookup service");
			}
		}
	}

	public class Lookup : ILookup
    {
	    private readonly MongoContext _mongoContext;

	    private RestClient lookupClient;

	    public Lookup()
        {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();
	        var target = configuration["postcodelookupservicename"];
			Log.Information($"postcode target is {target}");
	        lookupClient = new RestClient(target);
	        var connection = $"mongodb://{configuration["connectionString"]}";
			Log.Information($"MongoConnection: {connection}");
	        _mongoContext = new MongoContext(connection, "HousePrice");
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



	    public async Task<PagedResult<WebAPi.HousePrice>> GetLookups(string postcode, double radius)
        {
			Log.Information("Starting retrieval postcode retrieval...");
	        
            var postcodeInfo = LogAccessTime(PostcodeLookup.GetByPostcode, postcode, "Postcode lookup of lat and long took {0} milliseconds");
	        var timer = Stopwatch.StartNew();
	        if (postcodeInfo?.Longitude != null && postcodeInfo?.Latitude != null)
	        {
		        try
		        {
					Log.Information($"Sending request to Mongo...");
			        var list = await _mongoContext.ExecuteAsync<WebAPi.HousePrice, PagedResult<WebAPi.HousePrice>>("Transactions",
				        async (activeCollection) =>
			        {
				        var locationQuery =
					        new FilterDefinitionBuilder<WebAPi.HousePrice>().GeoWithinCenterSphere(
						        tag => tag.Location,
						        postcodeInfo.Longitude.Value,
						        postcodeInfo.Latitude.Value,
						        (radius / 1000) / 6371);
				      
						var sort = new SortDefinitionBuilder<WebAPi.HousePrice>().Descending(x => x.TransferDate);
				       
				        var query = activeCollection.Find(locationQuery);

				        var result = new PagedResult<WebAPi.HousePrice>(100, await query.Sort(sort).Skip(0).Limit(25).ToListAsync());

				        Log.Information($"Request to mongo successful");

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

	        return new PagedResult<WebAPi.HousePrice>(0, new WebAPi.HousePrice[0]);
        }
    }
}