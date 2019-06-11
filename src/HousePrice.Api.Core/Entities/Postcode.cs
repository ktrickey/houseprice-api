using System;
using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Entities
{
    public interface IPostcodeData
    {
        long Id { get; }
        string Postcode { get; }
        ILocation Location { get; }
    }

    [Serializable]
    internal class PostcodeData : IPostcodeData
    {
        public long Id { get; set; }
        public string Postcode {get; set; }
        public ILocation Location { get; set; }
    }
}