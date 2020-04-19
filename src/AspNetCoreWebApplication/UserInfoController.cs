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

        [HttpPost("")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult PostSomething(PostPayload postPayload) {
            return Ok(new {
                RequestPayload = postPayload,
                Claims = User.Claims.Select(c => new {
                    c.Type, c.Value
                })
            });
        }

        [HttpGet("{*anything}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetAnything(string anything) {
            return Ok(new {
                UriSegment = anything,
                Claims = User.Claims.Select(c => new {
                    c.Type, c.Value
                })
            });
        }

        public class PostPayload {
            public int Id { get; set; }
        }
    }
}