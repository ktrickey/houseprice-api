using System.Net;
using System.Net.Http;
using HousePrice.WebAPi;
using RestSharp;
using Serilog;

namespace HousePrice.Api.Services
{
    public interface IPostcodeLookup
    {
        PostcodeData GetByPostcode(string postcode);
    }

    public class PostcodeLookup : IPostcodeLookup
    {
        private IRestClient lookupClient;
        public PostcodeLookup(IPostcodeLookupConfig config)
        {
            lookupClient = config.RestClient;
        }
        public PostcodeData GetByPostcode(string postcode)
        {
            var response = lookupClient.Get<PostcodeData>(new RestRequest($"api/postcode/{WebUtility.UrlEncode(postcode)}"));
            Log.Information($"Response code: {response.StatusCode}, {response.Content}");
            if (response.IsSuccessful && response.StatusCode !=HttpStatusCode.NotFound)
            {
                var data = response.Data;
                Log.Information($"Postcode:{data.Postcode}, lat:{data.Latitude}, long:{data.Longitude}");

                return data;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Log.Information($"Postcode lookup for {postcode} not found");
                return null;
            }
            else
            {
                Log.Error($"Request failed, response code: {response.StatusCode}, {response.ErrorMessage}");
                throw new HttpRequestException("Failed to access the postcode lookup service");
            }
        }
    }
}