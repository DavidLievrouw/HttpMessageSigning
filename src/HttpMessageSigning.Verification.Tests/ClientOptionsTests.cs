using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClientOptionsTests {
        private readonly ClientOptions _sut;

        public ClientOptionsTests() {
            _sut = new ClientOptions();
        }

        public class Construction : ClientOptionsTests {
            [Fact]
            public void SetsDefaults() {
                _sut.Claims.Should().NotBeNull().And.BeEmpty();
                _sut.ClockSkew.Should().Be(TimeSpan.FromMinutes(1));
                _sut.NonceLifetime.Should().Be(TimeSpan.FromMinutes(5));
                _sut.RequestTargetEscaping.Should().Be(RequestTargetEscaping.RFC3986);
            }

            [Fact]
            public void DefaultIsValid() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }

        public class Validate : ClientOptionsTests {
            [Fact]
            public void WhenClockSkewIsNegative_ThrowsValidationException() {
                _sut.NonceLifetime = TimeSpan.FromSeconds(-1);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenClockSkewIsZero_ThrowsValidationException() {
                _sut.NonceLifetime = TimeSpan.Zero;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenNonceLifetimeIsNegative_ThrowsValidationException() {
                _sut.NonceLifetime = TimeSpan.FromSeconds(-1);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenNonceLifetimeIsZero_ThrowsValidationException() {
                _sut.NonceLifetime = TimeSpan.Zero;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void GivenInvalidRequestTargetEscapingOption_ThrowsValidationException() {
                _sut.RequestTargetEscaping = (RequestTargetEscaping) (-99);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }

        public class GetValidationErrors : ClientOptionsTests {
            [Fact]
            public void WhenNonceLifetimeIsNegative_IsInvalid() {
                _sut.NonceLifetime = TimeSpan.FromSeconds(-1);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.NonceLifetime));
            }

            [Fact]
            public void WhenNonceLifetimeIsZero_IsInvalid() {
                _sut.NonceLifetime = TimeSpan.Zero;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.NonceLifetime));
            }

            [Fact]
            public void WhenClockSkewIsNegative_IsInvalid() {
                _sut.ClockSkew = TimeSpan.FromSeconds(-1);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.ClockSkew));
            }

            [Fact]
            public void WhenClockSkewIsZero_IsInvalid() {
                _sut.ClockSkew = TimeSpan.Zero;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.ClockSkew));
            }

            [Fact]
            public void GivenInvalidRequestTargetEscapingOption_IsInvalid() {
                _sut.RequestTargetEscaping = (RequestTargetEscaping) (-99);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.RequestTargetEscaping));
            }

            [Fact]
            public void WhenEverythingIsValid_IsValid() {
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNull().And.BeEmpty();
            }
        }
    }
}