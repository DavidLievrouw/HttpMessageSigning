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
            var signer = _requestSignerFactory.Create(
                new KeyId(Guid.NewGuid().ToString()),
                new SigningSettings {
                    SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("s3cr37"),
                    EnableNonce = false,
                    DigestHashAlgorithm = default,
                    AutomaticallyAddRecommendedHeaders = false,
                    Headers = options.Headers
                        .Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(h => new HeaderName(h))
                        .ToArray(),
                    Events = new RequestSigningEvents {
                        OnSigningStringComposed = (message, signingString) => {
                            Log.Information(signingString);
                            return Task.CompletedTask;
                        }
                    }
                });

            var request = HttpRequestMessageParser.Parse(httpMessage);
            await signer.Sign(request);
            
            return 0;
        }
    }
}