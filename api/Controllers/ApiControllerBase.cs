using Microsoft.AspNetCore.Mvc;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
