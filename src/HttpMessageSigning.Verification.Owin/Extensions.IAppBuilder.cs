using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Extensions;
using Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class CompositionExtensions {
        /// <summary>
        ///     Register the HttpRequestSigning authentication middleware in the pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder" /> to add to.</param>
        /// <param name="options">The <see cref="SignedHttpRequestAuthenticationOptions" />.</param>
        /// <returns>The <see cref="IAppBuilder" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        public static IAppBuilder UseHttpRequestSignatureAuthentication(this IAppBuilder app, SignedHttpRequestAuthenticationOptions options) {
            if (app == null) throw new ArgumentNullException(nameof(app));
            app.Use<HttpRequestSignatureAuthenticationMiddleware>(options);
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }
}