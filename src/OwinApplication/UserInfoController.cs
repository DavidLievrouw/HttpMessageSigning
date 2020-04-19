using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace OwinApplication {
    [Authorize(Roles = "user.read")]
    public class UserInfoController : ApiController {
        public IHttpActionResult Get() {
            var authenticatedUser = User as ClaimsPrincipal;
            if (authenticatedUser == null) return Unauthorized();
            if (!authenticatedUser.Identity.IsAuthenticated) return Unauthorized();

            return Ok(
                authenticatedUser.Claims
                    .Select(c => new {
                        c.Type, c.Value
                    })
                    .ToList());
        }

        public IHttpActionResult Post(PostPayload postPayload) {
            var authenticatedUser = User as ClaimsPrincipal;
            if (authenticatedUser == null) return Unauthorized();
            if (!authenticatedUser.Identity.IsAuthenticated) return Unauthorized();

            return Ok(new {
                    RequestPayload = postPayload,
                    Claims = authenticatedUser.Claims
                        .Select(c => new {
                            c.Type, c.Value
                        })
                        .ToList()
                }
            );
        }

        public IHttpActionResult Get(string id) {
            var authenticatedUser = User as ClaimsPrincipal;
            if (authenticatedUser == null) return Unauthorized();
            if (!authenticatedUser.Identity.IsAuthenticated) return Unauthorized();

            return Ok(new {
                    UriSegment = id,
                    Claims = authenticatedUser.Claims
                        .Select(c => new {
                            c.Type, c.Value
                        })
                        .ToList()
                }
            );
        }

        public class PostPayload {
            public int Id { get; set; }
        }
    }
}