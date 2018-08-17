using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HousePrice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
    

        // GET api/values/5
        [HttpGet("{postcode}/{radius}")]
        public ActionResult<IEnumerable<Services.HousePrice>> Get(string postcode, int radius = 0)
        {
            var service = new HousePrice.Api.Services.Importer();
            return Ok(service.Find(postcode, radius));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
