using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class Base64ConverterTests {
        private readonly Base64Converter _sut;

        public Base64ConverterTests() {
            _sut = new Base64Converter();
        }

        public class FromBase64 : Base64ConverterTests {
            private readonly string _str;
            private readonly byte[] _bytes;

            public FromBase64() {
                _str = "YmFzZTY0IGVuY29kZWQgc3RyaW5n";
                _bytes = new byte[] {0x62, 0x61, 0x73, 0x65, 0x36, 0x34, 0x20, 0x65, 0x6E, 0x63, 0x6F, 0x64, 0x65, 0x64, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67};
            }

            [Fact]
            public void GivenNullString_ThrowsArgumentNullException() {
                Action act = () => _sut.FromBase64(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenEmptyString_ReturnsExpectedResult() {
                byte[] bytes = null;
                Action act = () => bytes = _sut.FromBase64("");
                act.Should().NotThrow();
                bytes.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void ReturnsExpectedBytes() {
                byte[] bytes = null;
                Action act = () => bytes = _sut.FromBase64(_str);
                act.Should().NotThrow();
                bytes.Should().BeEquivalentTo(_bytes);
            }
        }

        public class ToBase64 : Base64ConverterTests {
            private readonly string _str;
            private readonly byte[] _bytes;

            public ToBase64() {
                _str = "YmFzZTY0IGVuY29kZWQgc3RyaW5n";
                _bytes = new byte[] {0x62, 0x61, 0x73, 0x65, 0x36, 0x34, 0x20, 0x65, 0x6E, 0x63, 0x6F, 0x64, 0x65, 0x64, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67};
            }

            [Fact]
            public void GivenNullBytes_ThrowsArgumentNullException() {
                Action act = () => _sut.ToBase64(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenEmptyBytes_ReturnsExpectedString() {
                string str = null;
                Action act = () => str = _sut.ToBase64(Array.Empty<byte>());
                act.Should().NotThrow();
                str.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void ReturnsExpectedString() {
                string str = null;
                Action act = () => str = _sut.ToBase64(_bytes);
                act.Should().NotThrow();
                str.Should().Be(_str);
            }
        }
    }
}