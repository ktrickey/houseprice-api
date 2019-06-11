using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Entities
{
    internal class Location: ILocation
    {
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public double Latitude { get; }
        public double Longitude { get; }
    }
}