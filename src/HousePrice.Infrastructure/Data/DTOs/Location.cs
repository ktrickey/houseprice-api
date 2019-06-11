using HousePrice.Api.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace HousePrice.Infrastructure.Data.DTOs
{
    public class Location: ILocation
    {
        private readonly double _latitude;
        private readonly double _longitude;

        public Location(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }
        [BsonConstructor]
        public Location(double[] coordinates): this(coordinates[1], coordinates[0])
        {

        }

        
        public double Latitude => _latitude;
        public double Longitude => _longitude;

        // ReSharper disable once PossibleInvalidOperationException
        // ReSharper disable once PossibleInvalidOperationException
        [BsonElement("coordinates")]
        public double[] Coordinates =>  new double[] {_longitude, _latitude};
        [BsonElement("type")]
        public string Type => "Point";

    }
}