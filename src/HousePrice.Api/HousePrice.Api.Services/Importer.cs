using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Polly;

namespace HousePrice.Api.Services
{
    public static class PostcodeLookup
    {
        [Serializable]
        public class PostcodeData
        {
            private string _postcode;

            public long Id { get; set; }

            public string Postcode
            {
                get => _postcode;
                set => _postcode = value.Replace(" ", string.Empty);
            }

            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }

        private static Dictionary<string, PostcodeData> postcodeLookup;
        private static bool lookupComplete = false;

        public static bool IsReady => lookupComplete;

        static PostcodeLookup()
        {
            Task.Run(() =>
            {
                using (var streamReader =
                    new StreamReader(new FileStream(@"c:\mongo\ukpostcodes.csv", FileMode.Open, FileAccess.Read)))
                {
                    using (var csvReader = new CsvHelper.CsvReader(streamReader))
                    {
                        csvReader.Configuration.BufferSize = 131072;
                        var recs = csvReader.GetRecords<PostcodeData>();
                        var timer = Stopwatch.StartNew();
                        postcodeLookup = recs.ToDictionary(p => p.Postcode);
                        lookupComplete = true;
                        timer.Stop();
                        Console.WriteLine(timer.ElapsedMilliseconds);

                    }
                }
            });
        }

        public static async Task<PostcodeData> GetByPostcode(string postcode)
        {
            while (!lookupComplete){};

            var found = postcodeLookup.TryGetValue(postcode, out PostcodeData postcodeData);
            return found ? postcodeData:null;
        }
    }

    public class Location
    {
        private readonly double? _latitude;
        private readonly double? _longitude;
        private readonly bool _isValid;

        public Location(double? latitude, double? longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            _isValid = _latitude.HasValue && _longitude.HasValue;
        }
        [BsonConstructor]
        public Location(double[] coordinates): this(coordinates[1], coordinates[0])
        {
            
        }

        // ReSharper disable once PossibleInvalidOperationException
        // ReSharper disable once PossibleInvalidOperationException
        [BsonElement("coordinates")]
        public double[] Coordinates =>  _isValid? new double[] {_longitude.Value, _latitude.Value}:null;
        [BsonElement("type")]
        public string Type => "Point";

    }
    public class HousePrice
    {
        [BsonId]
        public string TransactionId { get; set; }
        public double Price { get; set; }
        public DateTime TransferDate { get; set; }
        public string Postcode { get; set; }
        public string PropertyType { get; set; }
        public string IsNew { get; set; }
        public string Duration { get; set; }
        public string PAON { get; set; }
        public string SAON { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public string CategoryType { get; set; }
        public string Status { get; set; }
        public Location Location { get; set; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class HousePriceMap : ClassMap<HousePrice>
    {
        public HousePriceMap()
        {
            Map( m => m.TransactionId ).Index(0);
            Map( m => m.Price ).Index(1);
            Map( m => m.TransferDate ).Index(2);
            Map( m => m.Postcode ).Index(3);
            Map( m => m.PropertyType ).Index(4);
            Map( m => m.IsNew ).Index(5);
            Map( m => m.Duration ).Index(6);
            Map( m => m.PAON ).Index(7);
            Map( m => m.SAON ).Index(8);
            Map( m => m.Street ).Index(9);
            Map( m => m.Locality ).Index(10);
            Map( m => m.City ).Index(11);
            Map( m => m.District ).Index(12);
            Map( m => m.County ).Index(13);
            Map( m => m.CategoryType ).Index(14);
            Map( m => m.Status ).Index(15);
        }
    }

    public interface IImporter
    {
        Task Import(Stream csvStream);
    }

    public class Importer : IImporter
    {
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
                        var locationData = await PostcodeLookup.GetByPostcode(record.Postcode);
                        record.Location = locationData?.Latitude != null && locationData?.Longitude != null
                            ? new Location(locationData?.Latitude, locationData?.Longitude)
                            : null;
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
            var client = new MongoClient("mongodb://localhost:32768");
            var database = client.GetDatabase("HousePrice");
            //     database.DropCollection("Transactions");
            var collection = database.GetCollection<HousePrice>("Transactions");

                                var housePriceBuilder = Builders<HousePrice>.IndexKeys;
                    var indexModel = new CreateIndexModel<HousePrice>(housePriceBuilder.Geo2DSphere(x=>x.Location));
                    await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
        }

        public void GetMatches(string postcode, double radius)
        {
            //var client = new MongoClient("mongodb://localhost:32768");
            //var database = client.GetDatabase("HousePrice");
            ////     database.DropCollection("Transactions");
            //var collection = database.GetCollection<HousePrice>("Transactions");
            //var postcodeLocation = PostcodeLookup.GetByPostcode("CB233NY");
            //var builder = Builders<HousePrice>.Filter;
            //var filter = builder.NearSphere(x => x.Location, postcodeLocation.Longitude.Value, postcodeLocation.Latitude.Value, radius);

            //FindOptions<HousePrice> options = new FindOptions<HousePrice> { Limit = 25};
            //var stuff = collection.FindAsync(filter, options).ToListAsync();
        }
        
    }
}