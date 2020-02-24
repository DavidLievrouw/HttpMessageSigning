using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HeaderNameTests {
        public class Construction : HeaderNameTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptyHeaderName() {
                var actual = new HeaderName();
                actual.Should().Be(HeaderName.Empty);
            }

            [Fact]
            public void Constructor_CreatesHeaderNameWithValue() {
                var actual = new HeaderName("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptyHeaderName() {
                var actual = new HeaderName(null);
                actual.Should().Be(HeaderName.Empty);
            }
        }

        public class Equality : HeaderNameTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new HeaderName("abc123");
                var second = new HeaderName(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new HeaderName("abc123");
                var second = new HeaderName("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreEqual() {
                var first = new HeaderName("abc123");
                var second = new HeaderName("aBc123");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new HeaderName("");
                var second = new HeaderName("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new HeaderName("");
                var second = HeaderName.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotHeaderName_AreNotEqual() {
                var first = new HeaderName("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : HeaderNameTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new HeaderName(value).ToString();
                actual.Should().Be(value);
            }
            
            [Fact]
            public void ConvertsNameToLowercase() {
                var value = "TheValue-Again";
                var actual = new HeaderName(value).ToString();
                actual.Should().Be("thevalue-again");
            }
        }

        public class ConversionOperatorsForString : HeaderNameTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new HeaderName(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new HeaderName(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (HeaderName) str;
                var expected = new HeaderName(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptyHeaderName() {
                string nullString = null;
                var actual = (HeaderName) nullString;
                actual.Should().NotBeNull().And.BeOfType<HeaderName>();
                actual.Should().Be(HeaderName.Empty);
            }
        }
    }
}