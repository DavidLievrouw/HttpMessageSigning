using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class FakeOwinRequest : IOwinRequest {
        public Task<IFormCollection> ReadFormAsync() {
            throw new NotSupportedException();
        }

        public T Get<T>(string key) {
            throw new NotSupportedException();
        }

        public IOwinRequest Set<T>(string key, T value) {
            throw new NotSupportedException();
        }

        public IDictionary<string, object> Environment { get; }
        public IOwinContext Context { get; }
        public string Method { get; set; }
        public string Scheme { get; set; }
        public bool IsSecure { get; }
        public HostString Host { get; set; }
        public PathString PathBase { get; set; }
        public PathString Path { get; set; }
        public QueryString QueryString { get; set; }
        public IReadableStringCollection Query { get; }
        public Uri Uri { get; }
        public string Protocol { get; set; }
        public IHeaderDictionary Headers { get; } = new HeaderDictionary(new Dictionary<string, string[]>());
        public RequestCookieCollection Cookies { get; }
        public string ContentType { get; set; }
        public string CacheControl { get; set; }
        public string MediaType { get; set; }
        public string Accept { get; set; }
        public Stream Body { get; set; }
        public CancellationToken CallCancelled { get; set; }
        public string LocalIpAddress { get; set; }
        public int? LocalPort { get; set; }
        public string RemoteIpAddress { get; set; }
        public int? RemotePort { get; set; }
        public IPrincipal User { get; set; }
    }
}