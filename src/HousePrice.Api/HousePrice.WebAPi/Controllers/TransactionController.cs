
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HousePrice.WebAPi.Controllers
{
    
    
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IImporter _importer;
        private readonly ILookup _lookup;

        public TransactionController(IImporter importer, ILookup lookup)
        {
            _importer = importer;
            _lookup = lookup;
        }
        [Route("{postcode}/{radius}")]
        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Api.Services.HousePrice>>> Get(string postcode, double radius)
        {
			Log.Information($"Processing request for {postcode} within {radius} km");

            if (Directory.Exists("/data/postcodes"))
            {
                Log.Information("Directory exists");
            }
            return Ok(await _lookup.GetLookups(postcode, radius *1000));
        }

        // POST api/values
        [HttpPost]
        public async void Post(string importName, [FromBody] string transactions)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.Write(transactions);
                    await writer.FlushAsync();
                    memoryStream.Position = 0;
                    await _importer.Import(importName, memoryStream);
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