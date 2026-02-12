using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public ActionResult GetHealth()
    {
        return Ok(new { status = "healthy", service = "ticketing-api" });
    }
}
