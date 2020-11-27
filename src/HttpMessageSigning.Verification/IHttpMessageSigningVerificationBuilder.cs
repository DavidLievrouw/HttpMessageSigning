using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     A builder for configuring HTTP message signature verification.
    /// </summary>
    public interface IHttpMessageSigningVerificationBuilder {
        /// <summary>
        ///     Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}