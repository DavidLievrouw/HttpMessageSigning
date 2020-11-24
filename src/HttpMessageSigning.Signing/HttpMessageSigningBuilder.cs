using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Signing {
    internal class HttpMessageSigningBuilder : IHttpMessageSigningBuilder {
        private SigningSettings _signingSettings;

        public HttpMessageSigningBuilder(IServiceCollection services) {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _signingSettings = new SigningSettings();
            Services.AddTransient(prov => new RegisteredSigningSettings(_signingSettings.KeyId, _signingSettings));
        }

        public IServiceCollection Services { get; }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseKeyId(KeyId keyId) {
            _signingSettings.KeyId = keyId;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseSigningSettings(SigningSettings settings) {
            _signingSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm) {
            _signingSettings.SignatureAlgorithm = signatureAlgorithm ?? throw new ArgumentNullException(nameof(signatureAlgorithm));
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseECDsaSignatureAlgorithm(Func<ECDsa> ecdsaFactory) {
            if (ecdsaFactory == null) throw new ArgumentNullException(nameof(ecdsaFactory));
            _signingSettings.SignatureAlgorithm = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA512, ecdsaFactory());
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseRSASignatureAlgorithm(Func<RSA> rsaFactory) {
            if (rsaFactory == null) throw new ArgumentNullException(nameof(rsaFactory));
            _signingSettings.SignatureAlgorithm = new RSASignatureAlgorithm(HashAlgorithmName.SHA512, rsaFactory());
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseHMACSignatureAlgorithm(Func<string> hmacSecretFactory) {
            if (hmacSecretFactory == null) throw new ArgumentNullException(nameof(hmacSecretFactory));
            _signingSettings.SignatureAlgorithm = new HMACSignatureAlgorithm(hmacSecretFactory(), HashAlgorithmName.SHA512);
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseExpires(TimeSpan expires) {
            _signingSettings.Expires = expires;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseNonce(bool enableNonce = true) {
            _signingSettings.EnableNonce = enableNonce;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseDigestAlgorithm(HashAlgorithmName digestAlgorithm) {
            _signingSettings.DigestHashAlgorithm = digestAlgorithm;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseHeaders(params HeaderName[] headers) {
            _signingSettings.Headers = headers ?? Array.Empty<HeaderName>();
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseAuthorizationScheme(string authorizationScheme) {
            if (string.IsNullOrEmpty(authorizationScheme)) throw new ArgumentException("Value cannot be null or empty.", nameof(authorizationScheme));
            _signingSettings.AuthorizationScheme = authorizationScheme;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseUseDeprecatedAlgorithmParameter(bool useDeprecatedAlgorithmParameter = true) {
            _signingSettings.UseDeprecatedAlgorithmParameter = useDeprecatedAlgorithmParameter;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseAutomaticallyAddedRecommendedHeaders(bool automaticallyAddRecommendedHeaders = true) {
            _signingSettings.AutomaticallyAddRecommendedHeaders = automaticallyAddRecommendedHeaders;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseRequestTargetEscaping(RequestTargetEscaping requestTargetEscaping) {
            _signingSettings.RequestTargetEscaping = requestTargetEscaping;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseEvents(RequestSigningEvents events) {
            _signingSettings.Events = events;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseOnRequestSigningEvent(RequestSigningEvents.OnRequestSigningDelegate onRequestSigning) {
            if (_signingSettings.Events == null) _signingSettings.Events = new RequestSigningEvents();
            _signingSettings.Events.OnRequestSigning = onRequestSigning;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseOnSigningStringComposed(RequestSigningEvents.OnSigningStringComposedDelegate onSigningStringComposed) {
            if (_signingSettings.Events == null) _signingSettings.Events = new RequestSigningEvents();
            _signingSettings.Events.OnSigningStringComposed = onSigningStringComposed;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseOnSignatureCreatedEvent(RequestSigningEvents.OnSignatureCreatedDelegate onSignatureCreated) {
            if (_signingSettings.Events == null) _signingSettings.Events = new RequestSigningEvents();
            _signingSettings.Events.OnSignatureCreated = onSignatureCreated;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IHttpMessageSigningBuilder UseOnRequestSignedEvent(RequestSigningEvents.OnRequestSignedDelegate onRequestSigned) {
            if (_signingSettings.Events == null) _signingSettings.Events = new RequestSigningEvents();
            _signingSettings.Events.OnRequestSigned = onRequestSigned;
            return this;
        }
    }
}