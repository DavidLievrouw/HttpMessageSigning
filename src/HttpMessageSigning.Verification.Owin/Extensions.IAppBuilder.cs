using System;
using Microsoft.Owin.Extensions;
using Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public static partial class Extensions {
        public static IAppBuilder UseHttpRequestSignatureAuthentication(this IAppBuilder app, SignedHttpRequestAuthenticationOptions options) {
            if (app == null) throw new ArgumentNullException(nameof(app));
            app.Use<HttpRequestSignatureAuthenticationMiddleware>(options);
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }
}