using System;
using HousePrice.Api.Services;
using RestSharp;

namespace HousePrice.WebAPi
{
    public interface IHousePriceLookupConfig
    {
        IMongoContext MongoContext { get; }
        IPostcodeLookup PostcodeLookup { get; }
    }

    public class HousePriceLookupConfig : IHousePriceLookupConfig
    {
        public HousePriceLookupConfig(IMongoContext mongoContext, IPostcodeLookup postcodeLookup)
        {
            MongoContext = mongoContext;
            PostcodeLookup = postcodeLookup;
        }
        public IRestClient RestClient { get; }
        public IMongoContext MongoContext { get; }
        public IPostcodeLookup PostcodeLookup { get; }
    }
}