using System;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClaimsPrincipalFactoryTests {
        private readonly ClaimsPrincipalFactory _sut;

        public ClaimsPrincipalFactoryTests() {
            _sut = new ClaimsPrincipalFactory();
        }

        public class CreateForClient : ClaimsPrincipalFactoryTests {
            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForClient(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenClientHasNullClaims_OnlyReturnsDefaultClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    null);
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "id1"),
                    new Claim("ver", typeof(IRequestSignatureVerifier).Assembly.GetName().Version.ToString(2))
                };
                actual.Claims.Should().BeEquivalentTo(expectedClaims, options => options.Including(c => c.Type).Including(c => c.Value));
            }
            
            [Fact]
            public void WhenClientHasNoClaims_OnlyReturnsDefaultClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "id1"),
                    new Claim("ver", typeof(IRequestSignatureVerifier).Assembly.GetName().Version.ToString(2))
                };
                actual.Claims.Should().BeEquivalentTo(expectedClaims, options => options.Including(c => c.Type).Including(c => c.Value));
            }

            [Fact]
            public void WhenClientHasAdditionalClaims_ReturnsDefaultAndAdditionalClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "id1"),
                    new Claim("ver", typeof(IRequestSignatureVerifier).Assembly.GetName().Version.ToString(2)),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2")
                };
                actual.Claims.Should().BeEquivalentTo(expectedClaims, options => options.Including(c => c.Type).Including(c => c.Value));
            }

            [Fact]
            public void CreatesIdentityWithExpectedNameAndRoleClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                actual.Identity.Should().BeAssignableTo<ClaimsIdentity>();
                actual.Identity.As<ClaimsIdentity>().NameClaimType.Should().Be(Constants.ClaimTypes.AppId);
                actual.Identity.As<ClaimsIdentity>().RoleClaimType.Should().Be(Constants.ClaimTypes.Role);
            }
            
            [Fact]
            public void CreatesIdentityForExpectedAuthenticationType() {
                var client = new Client(
                    (KeyId)"id1", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                actual.Identity.Should().BeAssignableTo<ClaimsIdentity>();
                actual.Identity.As<ClaimsIdentity>().AuthenticationType.Should().Be(Constants.AuthenticationSchemes.SignedHttpRequest);
            }
        }
    }
}