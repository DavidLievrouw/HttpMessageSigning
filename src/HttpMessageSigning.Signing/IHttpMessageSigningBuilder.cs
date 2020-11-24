using System;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    ///     A builder for configuring HTTP message signing.
    /// </summary>
    public interface IHttpMessageSigningBuilder {
        /// <summary>
        ///     Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>Configure the <see cref="KeyId" /> that the server can use to identify the client application.</summary>
        /// <param name="keyId">The entity that the server can use to look up the component they need to verify the signature.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseKeyId(KeyId keyId);
        
        /// <summary>Configure the <see cref="SigningSettings" />.</summary>
        /// <param name="settings">The signing settings.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseSigningSettings(SigningSettings settings);
        
        /// <summary>Configure the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.</summary>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm);

        /// <summary>Configure the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.</summary>
        /// <param name="ecdsaFactory">The factory that creates the ECDsa key pair.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseECDsaSignatureAlgorithm(Func<ECDsa> ecdsaFactory);
        
        /// <summary>Configure the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.</summary>
        /// <param name="rsaFactory">The factory that creates the RSA key pair.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseRSASignatureAlgorithm(Func<RSA> rsaFactory);
        
        /// <summary>Configure the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.</summary>
        /// <param name="hmacSecretFactory">The factory that creates the HMAC symmetric key.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseHMACSignatureAlgorithm(Func<string> hmacSecretFactory);
        
        /// <summary>Configure the timespan after which the signature is considered expired.</summary>
        /// <param name="expires">The timespan after which the signature is considered expired.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseExpires(TimeSpan expires);
        
        /// <summary>Configure a value indicating whether a 'Nonce' value will be included in the request signature.</summary>
        /// <param name="enableNonce">True to enable the 'Nonce' feature; otherwise, false.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseNonce(bool enableNonce = true);
        
        /// <summary>Configure the hash algorithm to use to generate a Digest http header, to be able to verify that the body has not been modified.</summary>
        /// <param name="digestAlgorithm">The hash algorithm to use to generate a Digest http header, to be able to verify that the body has not been modified. Set to 'default' to disable Digest header generation.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseDigestAlgorithm(HashAlgorithmName digestAlgorithm);
        
        /// <summary>Configure the ordered list of names of request headers to include when generating the signature for the message.</summary>
        /// <param name="headers">The ordered list of names of request headers to include when generating the signature for the message. When empty, the default headers will be included, according to the spec.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseHeaders(params HeaderName[] headers);
        
        /// <summary>Configure the name of the authorization scheme for the authorization header.</summary>
        /// <param name="authorizationScheme">The name of the authorization scheme for the authorization header.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseAuthorizationScheme(string authorizationScheme);
        
        /// <summary>Configure a value indicating whether the 'algorithm' parameter should report deprecated algorithm names, instead of 'hs2019', for backwards compatibility.</summary>
        /// <param name="useDeprecatedAlgorithmParameter">True to enable the 'algorithm' parameter should report deprecated algorithm names, instead of 'hs2019', for backwards compatibility. Setting this to 'false' causes the value of the 'algorithm' parameter to be 'hs2019'.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseUseDeprecatedAlgorithmParameter(bool useDeprecatedAlgorithmParameter = true);
        
        /// <summary>Configure a value indicating whether to automatically make the headers that are recommended by the spec a part of the signing string.</summary>
        /// <param name="automaticallyAddRecommendedHeaders">True to automatically make the headers that are recommended by the spec a part of the signing string. To fully conform with the spec, this setting should be set to 'false'.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseAutomaticallyAddedRecommendedHeaders(bool automaticallyAddRecommendedHeaders = true);
        
        /// <summary>Configure the method of escaping the value of the (request-target) pseudo-header.</summary>
        /// <param name="requestTargetEscaping">The method of escaping the value of the (request-target) pseudo-header.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseRequestTargetEscaping(RequestTargetEscaping requestTargetEscaping);
        
        /// <summary>Configure hooks that are called during HTTP message signing.</summary>
        /// <param name="events">The hooks that are called during HTTP message signing.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseEvents(RequestSigningEvents events);
        
        /// <summary>Configure a hook that are called during HTTP message signing.</summary>
        /// <param name="onRequestSigning">The hook that is called during HTTP message signing.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseOnRequestSigningEvent(RequestSigningEvents.OnRequestSigningDelegate onRequestSigning);
        
        /// <summary>Configure a hook that are called during HTTP message signing.</summary>
        /// <param name="onSigningStringComposed">The hook that is called during HTTP message signing.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseOnSigningStringComposed(RequestSigningEvents.OnSigningStringComposedDelegate onSigningStringComposed);
        
        /// <summary>Configure a hook that are called during HTTP message signing.</summary>
        /// <param name="onSignatureCreated">The hook that is called during HTTP message signing.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseOnSignatureCreatedEvent(RequestSigningEvents.OnSignatureCreatedDelegate onSignatureCreated);
        
        /// <summary>Configure a hook that are called during HTTP message signing.</summary>
        /// <param name="onRequestSigned">The hook that is called during HTTP message signing.</param>
        /// <returns>A reference to the <see cref="IHttpMessageSigningBuilder" /> that can be used to continue configuration.</returns>
        IHttpMessageSigningBuilder UseOnRequestSignedEvent(RequestSigningEvents.OnRequestSignedDelegate onRequestSigned);
    }
}