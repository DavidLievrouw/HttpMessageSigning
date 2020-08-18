using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerTests {
        private readonly ISigningSettingsSanitizer _signingSettingsSanitizer;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureCreator _signatureCreator;
        private readonly SigningSettings _signingSettings;
        private readonly ISignatureHeaderEnsurer _signatureHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;
        private readonly RequestSigner _sut;

        public RequestSignerTests() {
            FakeFactory.Create(
                out _signingSettingsSanitizer,
                out _signatureCreator,
                out _authorizationHeaderParamCreator,
                out _signatureHeaderEnsurer,
                out _systemClock,
                out _logger);
            _signingSettings = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                KeyId = new KeyId("client1"),
                SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                },
                AuthorizationScheme = "UnitTestAuth"
            };
            _sut = new RequestSigner(
                _signingSettingsSanitizer,
                _signatureCreator,
                _authorizationHeaderParamCreator,
                _signingSettings,
                _signatureHeaderEnsurer,
                _systemClock,
                _logger);
        }

        public class SignWithoutTime : RequestSignerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly DateTimeOffset _timeOfSigning;

            public SignWithoutTime() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow).Returns(_timeOfSigning);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Sign(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsValidationException() {
                _signingSettings.KeyId = KeyId.Empty; // Make invalid
                Func<Task> act = () => _sut.Sign(_httpRequest);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenSanitizerMakesSettingsInvalid_ThrowsValidationException() {
                A.CallTo(() => _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(A<SigningSettings>._, _httpRequest))
                    .Invokes(call => call.GetArgument<SigningSettings>(0).Headers = null);

                Func<Task> act = () => _sut.Sign(_httpRequest);
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public async Task SanitizesHeaderNamesToInclude_BeforeCreatingSignature() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(A<SigningSettings>.That.Matches(modifiedSigningSettings), _httpRequest)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _signingSettings.Expires))
                        .MustHaveHappened());
            }
            
            [Fact]
            public async Task SignsUsingSettingsThatCanBeModifiedByEvents() {
                _signingSettings.Events.OnRequestSigning = (message, settings) => {
                    settings.Expires = TimeSpan.FromHours(3);
                    return Task.CompletedTask;
                };

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == TimeSpan.FromHours(3);
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _signingSettings.Expires))
                    .Returns(signature);
                
                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);
                
                await _sut.Sign(_httpRequest);
                
                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }
            
            [Fact]
            public async Task EnsuresSignatureHeaders_BeforeSigning() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _signingSettings.Expires))
                    .Returns(signature);

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _signatureHeaderEnsurer.EnsureHeader(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task SetsExpectedAuthorizationHeaderInRequest() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _signingSettings.Expires))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }

            [Fact]
            public async Task OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _signingSettings.Expires))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }

            [Fact]
            public async Task BeforeSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigning = A.Fake<Func<HttpRequestMessage, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigning = onRequestSigning;
                
                await _sut.Sign(_httpRequest);

                A.CallTo(onRequestSigning).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._, A<TimeSpan>._)).MustHaveHappened());
            }
            
            [Fact]
            public async Task AfterSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigned = A.Fake<Func<HttpRequestMessage, Signature, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigned = onRequestSigned;
                
                await _sut.Sign(_httpRequest);

                A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._, A<TimeSpan>._)).MustHaveHappened()
                    .Then(A.CallTo(onRequestSigned).MustHaveHappened());
            }

            [Fact]
            public void WhenEventsAreNull_DoesNotThrow() {
                _signingSettings.Events = null;
                
                Func<Task> act = () => _sut.Sign(_httpRequest);
                
                act.Should().NotThrow();
            }
        }

        public class SignWithTime : RequestSignerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly DateTimeOffset _timeOfSigning;
            private readonly TimeSpan _expires;

            public SignWithTime() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 12, 20, 14, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow).Returns(_timeOfSigning);
                _expires = TimeSpan.FromMinutes(10);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Sign(null, _timeOfSigning, _expires);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsValidationException() {
                _signingSettings.KeyId = KeyId.Empty; // Make invalid
                Func<Task> act = () => _sut.Sign(_httpRequest, _timeOfSigning, _expires);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenSanitizerMakesSettingsInvalid_ThrowsValidationException() {
                A.CallTo(() => _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(A<SigningSettings>._, _httpRequest))
                    .Invokes(call => call.GetArgument<SigningSettings>(0).Headers = null);

                Func<Task> act = () => _sut.Sign(_httpRequest, _timeOfSigning, _expires);
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public async Task SanitizesHeaderNamesToInclude_BeforeCreatingSignature() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;

                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                A.CallTo(() => _signingSettingsSanitizer.SanitizeHeaderNamesToInclude(A<SigningSettings>.That.Matches(modifiedSigningSettings), _httpRequest)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _expires))
                        .MustHaveHappened());
            }
            
            [Fact]
            public async Task SignsUsingSettingsThatCanBeModifiedByEvents() {
                _signingSettings.Events.OnRequestSigning = (message, settings) => {
                    settings.Expires = TimeSpan.FromHours(3);
                    return Task.CompletedTask;
                };

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == TimeSpan.FromHours(3);
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _expires))
                    .Returns(signature);
                
                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);
                
                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);
                
                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }
            
            [Fact]
            public async Task EnsuresSignatureHeaders_BeforeSigning() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _expires))
                    .Returns(signature);

                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                A.CallTo(() => _signatureHeaderEnsurer.EnsureHeader(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task SetsExpectedAuthorizationHeaderInRequest() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _expires))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }

            [Fact]
            public async Task OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning, _expires))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("UnitTestAuth", "signature=abc123="));
            }

            [Fact]
            public async Task BeforeSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigning = A.Fake<Func<HttpRequestMessage, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigning = onRequestSigning;
                
                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                A.CallTo(onRequestSigning).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._, A<TimeSpan>._)).MustHaveHappened());
            }
            
            [Fact]
            public async Task AfterSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigned = A.Fake<Func<HttpRequestMessage, Signature, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigned = onRequestSigned;
                
                await _sut.Sign(_httpRequest, _timeOfSigning, _expires);

                A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._, A<TimeSpan>._)).MustHaveHappened()
                    .Then(A.CallTo(onRequestSigned).MustHaveHappened());
            }

            [Fact]
            public void WhenEventsAreNull_DoesNotThrow() {
                _signingSettings.Events = null;
                
                Func<Task> act = () => _sut.Sign(_httpRequest, _timeOfSigning, _expires);
                
                act.Should().NotThrow();
            }

            [Fact]
            public void WhenSigningInTheFuture_ThrowsHttpMessageSigningException() {
                A.CallTo(() => _systemClock.UtcNow).Returns(_timeOfSigning.AddMinutes(15));
                
                Func<Task> act = () => _sut.Sign(_httpRequest, _timeOfSigning, _expires);
                
                act.Should().Throw<HttpMessageSigningException>();
            }
            
            [Fact]
            public void WhenSigningInThePast_ThrowsHttpMessageSigningException() {
                A.CallTo(() => _systemClock.UtcNow).Returns(_timeOfSigning.AddMinutes(-6));
                
                Func<Task> act = () => _sut.Sign(_httpRequest, _timeOfSigning, TimeSpan.FromMinutes(5));
                
                act.Should().Throw<HttpMessageSigningException>();
            }
        }
        
        public class Dispose : RequestSignerTests {
            private readonly ISignatureAlgorithm _signatureAlgorithm;

            public Dispose() {
                _signatureAlgorithm = A.Fake<ISignatureAlgorithm>();
                _signingSettings.SignatureAlgorithm = _signatureAlgorithm;
            }

            [Fact]
            public void DisposesOfSignatureAlgorithm() {
                _sut.Dispose();
                
                A.CallTo(() => _signatureAlgorithm.Dispose())
                    .MustHaveHappened();
            }
            
            [Fact]
            public void WhenSignatureAlgorithmIsNull_DoesNotThrow() {
                _signingSettings.SignatureAlgorithm = null;

                Action act = () => _sut.Dispose();
                
                act.Should().NotThrow();
            }
        }
    }
}