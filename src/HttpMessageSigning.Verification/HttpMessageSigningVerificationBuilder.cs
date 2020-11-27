using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    internal class HttpMessageSigningVerificationBuilder : IHttpMessageSigningVerificationBuilder {
        public HttpMessageSigningVerificationBuilder(IServiceCollection services) {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}