using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public static partial class Extensions {
        /// <summary>
        /// Adds http message signing scheme registrations to the <see cref="AuthenticationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the registrations to.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/> to which the registrations were added.</returns>
        public static AuthenticationBuilder AddSignedRequests(this AuthenticationBuilder builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddSignedRequests(SignedHttpRequestDefaults.AuthenticationScheme, null);
        }

        /// <summary>
        /// Adds http message signing scheme registrations to the <see cref="AuthenticationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the registrations to.</param>
        /// <param name="authenticationScheme">The name of the authentication scheme.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/> to which the registrations were added.</returns>
        public static AuthenticationBuilder AddSignedRequests(this AuthenticationBuilder builder, string authenticationScheme) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddSignedRequests(authenticationScheme, null);
        }

        /// <summary>
        /// Adds http message signing scheme registrations to the <see cref="AuthenticationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the registrations to.</param>
        /// <param name="configureOptions">The action that configures the authentication options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/> to which the registrations were added.</returns>
        public static AuthenticationBuilder AddSignedRequests(this AuthenticationBuilder builder, Action<SignedRequestAuthenticationOptions> configureOptions) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddSignedRequests(SignedHttpRequestDefaults.AuthenticationScheme, configureOptions);
        }

        /// <summary>
        /// Adds http message signing scheme registrations to the <see cref="AuthenticationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the registrations to.</param>
        /// <param name="authenticationScheme">The name of the authentication scheme.</param>
        /// <param name="configureOptions">The action that configures the authentication options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/> to which the registrations were added.</returns>
        public static AuthenticationBuilder AddSignedRequests(this AuthenticationBuilder builder, string authenticationScheme, Action<SignedRequestAuthenticationOptions> configureOptions) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(authenticationScheme)) throw new ArgumentException("Value cannot be null or empty.", nameof(authenticationScheme));

            builder.Services.AddSingleton<IPostConfigureOptions<SignedRequestAuthenticationOptions>, SignedRequestAuthenticationPostConfigureOptions>();

            if (configureOptions == null) configureOptions = options => { options.Realm = "App_" + authenticationScheme; };

            return builder.AddScheme<SignedRequestAuthenticationOptions, SignedRequestAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}