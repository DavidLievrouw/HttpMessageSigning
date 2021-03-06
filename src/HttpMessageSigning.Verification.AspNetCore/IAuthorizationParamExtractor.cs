using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal interface IAuthorizationHeaderExtractor {
        AuthenticationHeaderValue Extract(HttpRequest request);
    }
}