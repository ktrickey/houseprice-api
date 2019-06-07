using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.ApiModels;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HousePrice.Api.Api
{
    public class TransactionController : BaseApiController
    {
        private readonly IImporter _importer;
        private readonly IHousePriceLookup _lookup;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(IImporter importer, IHousePriceLookup lookup, ILogger<TransactionController> logger)
        {
            _importer = importer;
            _lookup = lookup;
            _logger = logger;
        }
        [Route("{postcode}/{radius}/{skip?}")]
        [HttpGet]
        public async Task<ActionResult<HousePriceList>> Get(string postcode, double radius, int? skip)
        {
			_logger.LogInformation($"Processing request for {postcode} within {radius} km");

            if (Directory.Exists("/data/postcodes"))
            {
                _logger.LogInformation("Directory exists");
            }

            var results = await _lookup.GetLookups(postcode.Replace(" ", string.Empty), radius * 1000, skip ?? 0);
            return Ok(
                new HousePriceList(results)

                );
        }
//
//        // POST api/values
//        [HttpPost]
//        public async void Post()
//        {
//			Log.Information("In Post");
//
//			using (var reader = new StreamReader(Request.Body))
//			{
//				var body = await reader.ReadToEndAsync();
//				Log.Information(body);
//				try
//				{
//					var transaction = JsonConvert.DeserializeObject<HousePrice>(body);
//					await _importer.Import( new []{transaction});
//				}
//				catch (Exception e)
//				{
//					Log.Error(e.Message);
//					throw;
//				}
//			}
//        }

        [HttpPost]
        public async void Post(IEnumerable<HousePriceTransaction> transactions)
        {
            _logger.LogInformation("In Post multiple");

            try
            {
                await _importer.Import(transactions);
            }
            catch (Exception e)
            {
                //Log.Error(e.Message);
                throw;
            }
        }

        // PUT api/values/5
        [HttpPut()]
        public void Put(int id, [FromBody] string value)
        {
            _importer.Import(null, null);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}