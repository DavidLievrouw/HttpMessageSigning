using System;
using System.Security.Claims;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClaimsPrincipalFactoryTests {
        private readonly ClaimsPrincipalFactory _sut;
        private readonly string _version;

        public ClaimsPrincipalFactoryTests() {
            _version = "2.0";
            _sut = new ClaimsPrincipalFactory(_version);
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
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    null);
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "Unit test app"),
                    new Claim("ver", _version)
                };
                actual.Claims.Should().BeEquivalentTo(expectedClaims, options => options.Including(c => c.Type).Including(c => c.Value));
            }
            
            [Fact]
            public void WhenClientHasNoClaims_OnlyReturnsDefaultClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "Unit test app"),
                    new Claim("ver", _version)
                };
                actual.Claims.Should().BeEquivalentTo(expectedClaims, options => options.Including(c => c.Type).Including(c => c.Value));
            }

            [Fact]
            public void WhenClientHasAdditionalClaims_ReturnsDefaultAndAdditionalClaims() {
                var client = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                var expectedClaims = new[] {
                    new Claim("appid", "id1"),
                    new Claim("name", "Unit test app"),
                    new Claim("ver", _version),
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
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                actual.Identity.Should().BeAssignableTo<ClaimsIdentity>();
                actual.Identity.As<ClaimsIdentity>().NameClaimType.Should().Be(SignedHttpRequestClaimTypes.Name);
                actual.Identity.As<ClaimsIdentity>().RoleClaimType.Should().Be(SignedHttpRequestClaimTypes.Role);
            }
            
            [Fact]
            public void CreatesIdentityForExpectedAuthenticationType() {
                var client = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    new Claim("c1", "v1"),
                    new Claim("c1", "v2"),
                    new Claim("c2", "v2"));
                
                var actual = _sut.CreateForClient(client);

                actual.Identity.Should().BeAssignableTo<ClaimsIdentity>();
                actual.Identity.As<ClaimsIdentity>().AuthenticationType.Should().Be(SignedHttpRequestDefaults.AuthenticationScheme);
            }
        }
    }
}