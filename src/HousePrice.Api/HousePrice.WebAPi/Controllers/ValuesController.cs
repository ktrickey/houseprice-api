﻿
using System.Collections.Generic;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(await _lookup.GetLookups(postcode, radius *1000));
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