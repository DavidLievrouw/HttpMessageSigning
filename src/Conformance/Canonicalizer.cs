using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Conformance {
    public class Canonicalizer {
        private readonly IRequestSignerFactory _requestSignerFactory;

        public Canonicalizer() {
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .BuildServiceProvider();
            _requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
        }

        public async Task<int> Run(CanonicalizeOptions options, string httpMessage) {
            var signingSettings = new SigningSettings {
                SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("s3cr37"),
                EnableNonce = false,
                DigestHashAlgorithm = default,
                AutomaticallyAddRecommendedHeaders = false,
                Headers = options.Headers
                    ?.Split(new [] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => new HeaderName(h))
                    .ToArray(),
                Events = new RequestSigningEvents {
                    OnSigningStringComposed = (message, signingString) => {
                        Log.Information(signingString);
                        return Task.CompletedTask;
                    }
                }
            };
            var signer = _requestSignerFactory.Create(new KeyId("test"), signingSettings);

            var request = HttpRequestMessageParser.Parse(httpMessage);

            var created = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(options.Created)) {
                var createdUnix = int.Parse(options.Created);
                created = DateTimeOffset.FromUnixTimeSeconds(createdUnix);
            }
            
            var expires = signingSettings.Expires;
            if (!string.IsNullOrEmpty(options.Expires)) {
                var expiresUnix = int.Parse(options.Expires);
                var expiresAbsolute = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
                expires = expiresAbsolute - created;
            }

            await signer.Sign(request, created, expires);
            
            Console.Out.Flush();
            
            return 0;
        }
    }
}