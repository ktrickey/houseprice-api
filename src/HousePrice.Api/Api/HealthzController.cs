using Microsoft.AspNetCore.Mvc;

namespace HousePrice.WebAPi.Controllers
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