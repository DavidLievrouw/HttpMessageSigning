using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication {
    [ApiController]
    [Route("userinfo")]
    [Authorize]
    public class UserInfoController : Controller {
        [HttpGet("")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetUserClaims() {
            return Ok(User.Claims.Select(c => new {
                c.Type, c.Value
            }));
        }
    }
}