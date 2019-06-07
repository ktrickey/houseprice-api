using Microsoft.AspNetCore.Mvc;

namespace HousePrice.Api.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthzController : ControllerBase
    {
        // GET: api/Healthz
        [HttpGet]
        public int Get()
        {
            return 1;
        }

    }
}