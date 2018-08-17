using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace HousePrice.Api.Services
{
    public interface ILookup
    {
        Task<IEnumerable<HousePrice>> GetLookups(string postcode, double radius);
    }

    public class Lookup : ILookup
    {
	    private readonly MongoContext _mongoContext;
        public Lookup()
        {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");

			var configuration = builder.Build();

			
			_mongoContext = new MongoContext(configuration["connectionString"], "HousePrice");
        }

        public async Task<IEnumerable<HousePrice>> GetLookups(string postcode, double radius)
        {

            var postcodeInfo = PostcodeLookup.GetByPostcode(postcode);
	        if (postcodeInfo?.Longitude != null && postcodeInfo?.Latitude != null)
	        {
		        try
		        {
			        var point = GeoJson.Point(GeoJson.Geographic(postcodeInfo.Longitude.Value,
				        postcodeInfo.Latitude.Value));
			        return await _mongoContext.ExecuteAsync<HousePrice, IEnumerable<HousePrice>>("Transactions", async (activeCollection) =>
			        {
				        var locationQuery =
					        new FilterDefinitionBuilder<HousePrice>().NearSphere(tag => tag.Location, point,
						        radius);
				        var proj = Builders<HousePrice>.Projection.ToBsonDocument();
						var sort = new SortDefinitionBuilder<HousePrice>().Descending(x=>x.TransferDate);
						var options = new FindOptions<HousePrice>()
						{
							BatchSize = 25,
							Skip = 0,
							Limit = 25,
							Projection = proj,
							Sort = sort

						};
				       
				        var query = await activeCollection.FindAsync(locationQuery, options);

				        

				        return await query.ToListAsync();
			        });

		        }
		        catch (Exception ex)
		        {
			        int i = 0;
			        //do something;
		        }
	        }

	        return new HousePrice[0];
        }
    }
}