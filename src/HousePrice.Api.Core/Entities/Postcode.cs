using System;
using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Entities
{
    [Serializable]
    public class PostcodeData: ILocation
    {
        public long Id { get; set; }
        public string Postcode {get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}