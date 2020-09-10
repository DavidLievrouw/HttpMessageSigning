using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Canonicalize {
    public static class Canonicalizer {
        public static async Task<string> Run(CanonicalizeOptions options) {
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .BuildServiceProvider();
            var requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();

            string interceptedSigningString = null;

            var signingSettings = new SigningSettings {
                SignatureAlgorithm = new CustomSignatureAlgorithm(options.Algorithm ?? "hs2019"),
                EnableNonce = false,
                DigestHashAlgorithm = default,
                AutomaticallyAddRecommendedHeaders = false,
                Headers = options.Headers
                    ?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => new HeaderName(h))
                    .ToArray(),
                Events = new RequestSigningEvents {
                    OnSigningStringComposed = (HttpRequestMessage requestToSign, ref string signingString) => {
                        interceptedSigningString = signingString;
                        return Task.CompletedTask;
                    }
                }
            };
            var signer = requestSignerFactory.Create(new KeyId("test"), signingSettings);

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

            await signer.Sign(options.Message, created, expires);

            return interceptedSigningString;
        }
    }
}