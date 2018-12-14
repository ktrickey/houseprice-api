using System;
using RestSharp;

namespace HousePrice.WebAPi
{
    public interface IPostcodeLookupConfig
    {
        IRestClient RestClient { get; }
    }

    public class PostcodeLookupConfig : IPostcodeLookupConfig
    {
        public PostcodeLookupConfig(IRestClient client, string postcodeServiceName)
        {
            RestClient = client;
            RestClient.BaseUrl = new Uri(postcodeServiceName);
        }
        public IRestClient RestClient { get; }
    }
}