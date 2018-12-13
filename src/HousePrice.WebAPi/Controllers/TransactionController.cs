using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace HousePrice.WebAPi.Controllers
{
    
    
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IImporter _importer;
        private readonly IHousePriceLookup _lookup;

        public TransactionController(IImporter importer, IHousePriceLookup lookup)
        {
            _importer = importer;
            _lookup = lookup;
        }
        [Route("{postcode}/{radius}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HousePrice>>> Get(string postcode, double radius)
        {
			Log.Information($"Processing request for {postcode} within {radius} km");

            if (Directory.Exists("/data/postcodes"))
            {
                Log.Information("Directory exists");
            }
            return Ok(await _lookup.GetLookups(postcode.Replace(" ", string.Empty), radius *1000));
        }

        // POST api/values
        [HttpPost]
        public async void Post()
        {
			Log.Information("In Post");
	        
			using (var reader = new StreamReader(Request.Body))
			{
				var body = await reader.ReadToEndAsync();
				Log.Information(body);
				try
				{
					var transaction = JsonConvert.DeserializeObject<HousePrice>(body);
					await _importer.Import(transaction);
				}
				catch (Exception e)
				{
					Log.Error(e.Message);
					throw;
				}
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