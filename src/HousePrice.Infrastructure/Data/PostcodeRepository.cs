using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;
using RestSharp;

namespace HousePrice.Infrastructure.Data
{

    public class PostcodeRepository : IPostcodeRepository
    {
        private readonly IRestClient _restClient;

        public PostcodeRepository(IRestClient restClient, string rootUrl)
        {
            _restClient = restClient;
            _restClient.BaseUrl = new Uri(rootUrl);
        }

        public async Task<PostcodeData> GetPostcode(string postcode)
        {
            var response = await _restClient.ExecuteGetTaskAsync<PostcodeData>(new RestRequest($"{WebUtility.UrlEncode(postcode)}"));
            //           Log.Information($"Response code: {response.StatusCode}, {response.Content}");
            if (response.IsSuccessful && response.StatusCode !=HttpStatusCode.NotFound)
            {
                var data = response.Data;
                //               Log.Information($"Postcode:{data.Postcode}, lat:{data.Latitude}, long:{data.Longitude}");

                return data;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                //              Log.Information($"Postcode lookup for {postcode} not found");
                return null;
            }
         
      
            //            Log.Error($"Request failed, response code: {response.StatusCode}, {response.ErrorMessage}");
            throw new HttpRequestException("Failed to access the postcode lookup service");
  
        }
    }
}