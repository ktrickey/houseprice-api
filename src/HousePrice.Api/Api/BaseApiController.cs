using Microsoft.AspNetCore.Mvc;

namespace HousePrice.Api.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : Controller
    {
    }
}
