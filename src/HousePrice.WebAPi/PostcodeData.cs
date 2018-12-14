using System;

namespace HousePrice.WebAPi
{
    [Serializable]
    public class PostcodeData
    {
        public long Id { get; set; }
        public string Postcode {get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}