using MongoDB.Bson.Serialization.Attributes;

namespace HousePrice.Api.Services
{
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
}