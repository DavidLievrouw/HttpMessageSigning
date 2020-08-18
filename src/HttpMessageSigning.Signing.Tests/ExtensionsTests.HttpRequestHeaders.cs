using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public partial class ExtensionsTests {
        public class HttpRequestHeaders : ExtensionsTests {
            private readonly System.Net.Http.Headers.HttpRequestHeaders _headers;

            public HttpRequestHeaders() {
                _headers = new HttpRequestMessage().Headers;
                _headers.Add("existing_simple_header", "existing_value");
                _headers.Add("existing_complex_header", new[] {"existing_value_01", "existing_value_02"});
            }

            public class AddSingle : HttpRequestHeaders {
                [Fact]
                public void GivenNullHeaders_ThrowsArgumentNullException() {
                    // ReSharper disable once InvokeAsExtensionMethod
                    Action act = () => Extensions.Set((System.Net.Http.Headers.HttpRequestHeaders)null, "header01", "value01");

                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void WhenHeaderAlreadyExists_ReplacesValue() {
                    _headers.Set("existing_simple_header", "value01");

                    _headers.GetValues("existing_simple_header").Should().BeEquivalentTo("value01");
                }

                [Fact]
                public void WhenHeaderDoesNotExist_AddsValue() {
                    _headers.Set("i_dont_exist", "value01");

                    _headers.GetValues("i_dont_exist").Should().BeEquivalentTo("value01");
                }

                [Theory]
                [InlineData(null)]
                [InlineData("")]
                public void GivenNullOrEmptyName_ThrowsArgumentException(string nullOrEmpty) {
                    Action act = () => _headers.Set(nullOrEmpty, "value01");

                    act.Should().Throw<ArgumentException>();
                }

                [Theory]
                [InlineData(null)]
                [InlineData("")]
                public void GivenNullOrEmptyValue_SetsValueToEmpty(string nullOrEmpty) {
                    _headers.Set("i_dont_exist", nullOrEmpty);

                    _headers.GetValues("i_dont_exist").Should().BeEquivalentTo("");
                }
            }

            public class AddMultiple : HttpRequestHeaders {
                [Fact]
                public void GivenNullHeaders_ThrowsArgumentNullException() {
                    // ReSharper disable once InvokeAsExtensionMethod
                    Action act = () => Extensions.Set((System.Net.Http.Headers.HttpRequestHeaders)null, "header01", new[] {"value01"});

                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void WhenHeaderAlreadyExists_ReplacesValues() {
                    _headers.Set("existing_complex_header", "value01", "value02");

                    _headers.GetValues("existing_complex_header").Should().BeEquivalentTo("value01", "value02");
                }

                [Fact]
                public void WhenHeaderAlreadyExists_AndItsCurrentlyASingleValue_ReplacesValues() {
                    _headers.Set("existing_simple_header", "value01");

                    _headers.Set("existing_simple_header", "value01", "value02");

                    _headers.GetValues("existing_simple_header").Should().BeEquivalentTo("value01", "value02");
                }

                [Fact]
                public void WhenHeaderDoesNotExist_AddsValues() {
                    _headers.Set("i_dont_exist", new List<string>(new[] {"value01", "value02"}));

                    _headers.GetValues("i_dont_exist").Should().BeEquivalentTo("value01", "value02");
                }

                [Theory]
                [InlineData(null)]
                [InlineData("")]
                public void GivenNullOrEmptyName_ThrowsArgumentException(string nullOrEmpty) {
                    Action act = () => _headers.Set(nullOrEmpty, "value01", "value02");

                    act.Should().Throw<ArgumentException>();
                }

                [Theory]
                [InlineData(null)]
                [InlineData("")]
                public void GivenNullOrEmptyValues_SetsValueToEmpty(string nullOrEmpty) {
                    _headers.Set("existing_complex_header", new[] {nullOrEmpty});

                    _headers.GetValues("existing_complex_header").Should().BeEquivalentTo("");
                }

                [Fact]
                public void GivenNoValues_RemovesHeader() {
                    _headers.Set("existing_complex_header", Enumerable.Empty<string>());

                    _headers.Contains("existing_complex_header").Should().BeFalse();
                }
            }

            public class RestrictedHeaders : HttpRequestHeaders {
                [Fact]
                public void CanAddDateHeaderWithValidValue() {
                    Action act = () => _headers.Set("Date", "Sat, 02 May 2020 16:35:45 GMT");
                    act.Should().NotThrow();

                    _headers.Date.Should().Be(new DateTimeOffset(2020, 5, 2, 16, 35, 45, TimeSpan.Zero));
                }

                [Fact]
                public void CannotAddDateHeaderWithInvalidValue() {
                    Action act = () => _headers.Set("Date", "{nonsense}");

                    act.Should().Throw<FormatException>();
                }
            }
        }
    }
}