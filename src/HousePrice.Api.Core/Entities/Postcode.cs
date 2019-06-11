using System;
using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Entities
{
    public interface IPostcodeData
    {
        long Id { get; set; }
        string Postcode { get; set; }
        ILocation Location { get; set; }
    }

    [Serializable]
    internal class PostcodeData : IPostcodeData
    {
        public long Id { get; set; }
        public string Postcode {get; set; }
        public ILocation Location { get; set; }
    }
}