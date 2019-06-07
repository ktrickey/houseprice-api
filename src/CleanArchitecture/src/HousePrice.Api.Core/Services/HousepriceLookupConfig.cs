using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Services
{
    public interface IHousePriceLookupConfig
    {
        IRepository MongoContext { get; }
        IPostcodeLookup PostcodeLookup { get; }
    }

    public class HousePriceLookupConfig : IHousePriceLookupConfig
    {
        public HousePriceLookupConfig(IRepository mongoContext, IPostcodeLookup postcodeLookup)
        {
            MongoContext = mongoContext;
            PostcodeLookup = postcodeLookup;
        }
        public IRepository MongoContext { get; }
        public IPostcodeLookup PostcodeLookup { get; }
    }
}