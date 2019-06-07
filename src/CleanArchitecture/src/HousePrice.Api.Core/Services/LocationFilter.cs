using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Services
{
    public class LocationFilter
    {
        private readonly ILocation _location;
        private readonly double _radius;

        public LocationFilter (ILocation location, double radius)
        {
            _location = location;
            _radius = radius;
        }


        public ILocation Location => _location;
        public double Radius => _radius;


    }
}