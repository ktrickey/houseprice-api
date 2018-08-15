using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;

namespace HousePrice.Api.Services
{
    public interface ILookup
    {
        Task<IEnumerable<HousePrice>> GetLookups(string postcode, double radius);
    }

    public class Lookup : ILookup
    {
        public Lookup()
        {
            
        }

        public async Task<IEnumerable<HousePrice>> GetLookups(string postcode, double radius)
        {
            var client = new MongoClient("mongodb://localhost:32768");
            var database = client.GetDatabase("HousePrice");
            var collection = database.GetCollection<HousePrice>("Transactions");
            

            FilterDefinition<HousePrice> filter = FilterDefinition<HousePrice>.Empty;
            FindOptions<HousePrice> options = new FindOptions<HousePrice>
            {
                BatchSize = 25,
                NoCursorTimeout = false
            };

            var housePrices = new List<HousePrice>();


            var postcodeInfo = PostcodeLookup.GetByPostcode(postcode);
            try
            {
                var point = GeoJson.Point(GeoJson.Geographic(postcodeInfo.Longitude.Value, postcodeInfo.Latitude.Value));
                var locationQuery = new FilterDefinitionBuilder<HousePrice>().Near(tag => tag.Location, point, radius); //fetch results that are within a 50 metre radius of the point we're searching.
                var query = collection.Find<HousePrice>(locationQuery).Limit(25); //Limit the query to return only the top 10 results.
                return query.ToList();
            }
            catch (Exception ex)
            {
                //do something;
            }
            return new HousePrice[0];
        }
    }
}