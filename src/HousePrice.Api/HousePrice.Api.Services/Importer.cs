using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HousePrice.Api.Services
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Importer
    {
	    public IEnumerable<HousePrice> Find(string postcode, double radius)
	    {
		    //var client = new MongoClient("mongodb://localhost:32768");
		    //var database = client.GetDatabase("HousePrice");
		    ////     database.DropCollection("Transactions");
		    //var collection = database.GetCollection<HousePrice>("Transactions");

		    return new MongoRunner().RunInMongo<Task<IEnumerable<HousePrice>>, HousePrice>(async collection =>
		    {
			    var location = PostcodeLookup.GetByPostcode(postcode.ToUpper());

			    var builder = Builders<HousePrice>.Filter;
			    if (location?.Longitude != null && location?.Latitude != null)
			    {
				    var sort = Builders<HousePrice>.Sort.Descending(x=>x.TransferDate);
				    var options = new FindOptions<HousePrice>
				    {
					    BatchSize = 25,
					    Skip = 0,
					    Limit = 25
				    };

				    var filter = builder.NearSphere(x => x.Location, location.Longitude.Value,
					    location.Latitude.Value, radius);

				    return await collection.FindAsync(filter, options).Result.ToListAsync();
			    }
				return new HousePrice[0];
		    }).Result.OrderByDescending(x=>x.TransferDate);




	    }
        public async Task Import(Stream csvStream)
        {
            using (var streamReader = new StreamReader(csvStream))
            {
                using (var csvReader = new CsvHelper.CsvReader(streamReader))
                {

                    csvReader.Configuration.HasHeaderRecord = false;
                    csvReader.Configuration.RegisterClassMap<HousePriceMap>();
                    var client = new MongoClient("mongodb://localhost:32768");
                    var database = client.GetDatabase("HousePrice");
                    //database.DropCollection("Transactions");
                    var collection = database.GetCollection<HousePrice>("Transactions");
			
                    var batch = new List<HousePrice>();

                    while (csvReader.Read())
                    {
                        var record = csvReader.GetRecord<HousePrice>();
                        record.Postcode = record.Postcode.Replace(" ", String.Empty);
                        var locationData = PostcodeLookup.GetByPostcode(record.Postcode);
                        record.Location = locationData?.Latitude!=null && locationData?.Longitude!=null?new Location(locationData?.Latitude, locationData?.Longitude): null;
                        batch.Add(record);
                        
                        if (batch.Count == 1000)
                        {
                            await collection.InsertManyAsync(batch);
                            batch.Clear();
                        }

          
                    }
                    await collection.InsertManyAsync(batch);
   
                }
            }
        }

        public async Task AddIndex()
        {

	        await new MongoRunner().RunInMongoAsync<HousePrice>(async collection =>
	        {
		        var housePriceBuilder = Builders<HousePrice>.IndexKeys;
		        var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Geo2DSphere(x => x.Location));
		        await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
	        });


        }
        
    }
}