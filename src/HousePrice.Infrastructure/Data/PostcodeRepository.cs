using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace HousePrice.Infrastructure.Data
{

    public class PostcodeRepository : IPostcodeRepository
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<PostcodeRepository> _logger;

        public PostcodeRepository(IRestClient restClient, string rootUrl, ILogger<PostcodeRepository> logger)
        {
            _restClient = restClient;
            _logger = logger;
            _restClient.BaseUrl = new Uri(rootUrl);
        }

        public async Task<IPostcodeData> GetPostcode(string postcode)
        {
            var response = await _restClient.ExecuteGetTaskAsync<IPostcodeData>(
                new RestRequest($"{WebUtility.UrlEncode(postcode)}"));
            _logger.LogInformation($"Response code: {response.StatusCode}, {response.Content}");
            if (response.IsSuccessful && response.StatusCode !=HttpStatusCode.NotFound)
            {
                var data = response.Data;
                _logger.LogInformation($"Postcode:{data.Postcode}, lat:{data.Location.Latitude}, long:{data.Location.Longitude}");

                return data;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"Postcode lookup for {postcode} not found");
                return null;
            }
         
      
            //            Log.Error($"Request failed, response code: {response.StatusCode}, {response.ErrorMessage}");
            throw new HttpRequestException("Failed to access the postcode lookup service");
  
        }
    }
}