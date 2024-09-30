using System;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.SigningString {
    public class HeaderTests {
        public class Construction : HeaderTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptyHeader() {
                var actual = new Header();
                actual.Should().Be(Header.Empty);
            }

            [Fact]
            public void Constructor_CreatesHeaderWithLowercaseName() {
                var actual = new Header("theName", "v1", "v2");
                actual.Name.Should().Be("thename");
            }

            [Fact]
            public void Constructor_CreatesHeaderWithNameAndValues() {
                var actual = new Header("theName", "v1", "v2");
                actual.Name.Should().Be("thename");
                actual.Values.Should().BeEquivalentTo("v1", "v2");
            }

            [Fact]
            public void Constructor_AcceptsSingleHeaderValue() {
                var actual = new Header("theName", "v1");
                actual.Name.Should().Be("thename");
                actual.Values.Should().BeEquivalentTo("v1");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void NullOrEmptyName_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => new Header(nullOrEmpty, "v1", "v2");
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void NullValue_CreatesInstanceWithNoValues() {
                var header = Header.Empty;
                Action act = () => header = new Header("theName", null);
                act.Should().NotThrow();
                header.Name.Should().Be("thename");
                header.Values.Should().NotBeNull().And.BeEmpty();
            }
        }

        public class Equality : HeaderTests {
            [Fact]
            public void WhenNamesAreEqual_AreEqual() {
                var first = new Header("abc123", "v1");
                var second = new Header("abc123", "v2");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenNamesAreDifferent_AreNotEqual() {
                var first = new Header("abc123", "v1");
                var second = new Header("xyz123", "v1");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenNamesAreDifferentlyCased_AreEqual() {
                var first = new Header("abc123", "v1");
                var second = new Header("aBc123", "v2");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void InstanceFromDefaultCtorAndEmpty_AreEqual() {
                var first = new Header();
                var second = Header.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotHeader_AreNotEqual() {
                var first = new Header("abc123", "v1");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ConversionToString : HeaderTests {
            [Fact]
            public void ReturnsNameAndValuesAccordingToSpec() {
                var header = new Header("theName", "v1", "v2");
                var actual = header.ToString();
                actual.Should().Be("thename: v1, v2");
            }

            [Fact]
            public void KeepsWhitespaceInName() {
                var header = new Header(" the Name \t ", "v1", "v2");
                var actual = header.ToString();
                actual.Should().Be(" the name \t : v1, v2");
            }

            [Fact]
            public void WhenThereAreNoValues_ReturnsStringAccordingToSpec() {
                var header = new Header("theName", "");
                var actual = header.ToString();
                actual.Should().Be("thename: ");
            }

            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var header = new Header("theName", "v1", "v2");
                var actual = (string)header;
                actual.Should().Be("thename: v1, v2");
            }

            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var header = new Header("theName", "v1", "v2");
                string actual = header;
                actual.Should().Be("thename: v1, v2");
            }
        }

        public class ConversionFromString : HeaderTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyString_ReturnsEmptyHeader(string nullOrEmpty) {
                var actual = (Header)nullOrEmpty;
                actual.Should().NotBeNull().And.BeOfType<Header>();
                actual.Should().BeEquivalentTo(Header.Empty);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var str = "thename: v1, v2";
                var actual = (Header)str;
                var expected = new Header("thename", "v1", "v2");
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void WhenStringDoesNotContainHeader_ThrowsFormatException() {
                var actual = Header.Empty;
                Action act = () => actual = (Header)"v1, v2";
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenHeaderIsEmpty_ThrowsFormatException() {
                var actual = Header.Empty;
                Action act = () => actual = (Header)": v1, v2";
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenThereAreNoValues_ReturnsExpectedHeaderWithoutValues() {
                var actual = Header.Empty;
                Action act = () => actual = (Header)"thename: ";
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", string.Empty));
            }

            [Fact]
            public void TrimsValues() {
                var actual = Header.Empty;
                Action act = () => actual = (Header)"thename: v1,  v2 , v 3 \t ";
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2", "v 3"));
            }

            [Fact]
            public void RemovesWhitespaceOnlyValues() {
                var actual = Header.Empty;
                Action act = () => actual = (Header)"thename: v1, , v2";
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2"));
            }
        }

        public class TryParse : HeaderTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyString_Fails(string nullOrEmpty) {
                var isParsed = Header.TryParse(nullOrEmpty, out var actual);
                isParsed.Should().BeFalse();
                actual.Should().NotBeNull().And.BeOfType<Header>();
                actual.Should().BeEquivalentTo(Header.Empty);
            }

            [Fact]
            public void CanParseValidString() {
                var str = "thename: v1, v2";
                var isParsed = Header.TryParse(str, out var actual);
                isParsed.Should().BeTrue();
                var expected = new Header("thename", "v1", "v2");
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void WhenStringDoesNotContainHeader_Fails() {
                var isParsed = Header.TryParse("v1, v2", out _);
                isParsed.Should().BeFalse();
            }

            [Fact]
            public void WhenHeaderIsEmpty_Fails() {
                var isParsed = Header.TryParse(": v1, v2", out _);
                isParsed.Should().BeFalse();
            }

            [Fact]
            public void WhenThereAreNoValues_ReturnsExpectedHeaderWithoutValues() {
                var isParsed = Header.TryParse("thename: ", out var actual);
                isParsed.Should().BeTrue();
                actual.Should().BeEquivalentTo(new Header("thename", string.Empty));
            }

            [Fact]
            public void TrimsValues() {
                var isParsed = Header.TryParse("thename: v1,  v2 , v 3 \t ", out var actual);
                isParsed.Should().BeTrue();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2", "v 3"));
            }

            [Fact]
            public void RemovesWhitespaceOnlyValues() {
                var isParsed = Header.TryParse("thename: v1, , v2", out var actual);
                isParsed.Should().BeTrue();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2"));
            }
        }

        public class Parse : HeaderTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyString_ReturnsEmptyHeader(string nullOrEmpty) {
                Header? actual = null;
                Action act = () => actual = Header.Parse(nullOrEmpty);
                act.Should().NotThrow();
                actual.Should().NotBeNull().And.Be(Header.Empty);
            }

            [Fact]
            public void IsParsableFromString() {
                var str = "thename: v1, v2";
                var actual = Header.Parse(str);
                var expected = new Header("thename", "v1", "v2");
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void WhenStringDoesNotContainHeader_ThrowsFormatException() {
                var actual = Header.Empty;
                Action act = () => actual = Header.Parse("v1, v2");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenHeaderIsEmpty_ThrowsFormatException() {
                var actual = Header.Empty;
                Action act = () => actual = Header.Parse(": v1, v2");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenThereAreNoValues_ReturnsExpectedHeaderWithoutValues() {
                var actual = Header.Empty;
                Action act = () => actual = Header.Parse("thename: ");
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", string.Empty));
            }

            [Fact]
            public void TrimsValues() {
                var actual = Header.Empty;
                Action act = () => actual = Header.Parse("thename: v1,  v2 , v 3 \t ");
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2", "v 3"));
            }

            [Fact]
            public void RemovesWhitespaceOnlyValues() {
                var actual = Header.Empty;
                Action act = () => actual = Header.Parse("thename: v1, , v2");
                act.Should().NotThrow();
                actual.Should().BeEquivalentTo(new Header("thename", "v1", "v2"));
            }
        }

        public class Append : HeaderTests {
            private Func<Header> _sut;
            private readonly StringBuilder _sb;
            private readonly Action _act;

            public Append() {
                _sut = () => new Header("theName", "v1", "v2");
                _sb = new StringBuilder();
                _act = () => _sut().Append(_sb);
            }

            [Fact]
            public void AppendsNameAndValuesAccordingToSpec() {
                _act();
                _sb.ToString().Should().Be("\nthename: v1, v2");
            }

            [Fact]
            public void AppendsNameAndValuesAccordingToSpec_WhenThereAreNoValues() {
                _sut = () => new Header("theName", "");
                _act();
                _sb.ToString().Should().Be("\nthename: ");
            }

            [Fact]
            public void AppendsNameAndValuesAccordingToSpec_WhenThereIsOnlyOneValue() {
                _sut = () => new Header("theName", "v1");
                _act();
                _sb.ToString().Should().Be("\nthename: v1");
            }
        }

        public class ToStringOverride : HeaderTests {
            private Func<Header> _sut;
            private readonly Func<string> _act;

            public ToStringOverride() {
                _sut = () => new Header("theName", "v1", "v2");
                _act = () => _sut().ToString();
            }

            [Fact]
            public void ReturnsNameAndValuesAccordingToSpec() {
                var actual = _act();

                actual.Should().Be("thename: v1, v2");
            }

            [Fact]
            public void ReturnsNameAndValuesAccordingToSpec_WhenThereAreNoValues() {
                _sut = () => new Header("theName", "");

                var actual = _act();

                actual.Should().Be("thename: ");
            }

            [Fact]
            public void ReturnsNameAndValuesAccordingToSpec_WhenThereIsOnlyOneValue() {
                _sut = () => new Header("theName", "v1");

                var actual = _act();

                actual.Should().Be("thename: v1");
            }
        }
    }
}