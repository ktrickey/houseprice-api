
using System.Collections.Generic;
using HousePrice.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HousePrice.WebAPi.Controllers
{
    
    
    [Route("api/[controller]")]
    [ApiController]
    public class HousePriceController : ControllerBase
    {
        private readonly IImporter _importer;
        private readonly ILookup _lookup;

        public HousePriceController(IImporter importer, ILookup lookup)
        {
            _importer = importer;
            _lookup = lookup;
        }
        [Route("postcode/{postcode}")]
        // GET api/values/5
        [HttpGet]
        public ActionResult<IEnumerable<Api.Services.HousePrice>> Get(string postcode)
        {
            return Ok(_lookup.GetLookups(postcode, 10));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut()]
        public void Put(int id, [FromBody] string value)
        {
            _importer.Import(null);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}