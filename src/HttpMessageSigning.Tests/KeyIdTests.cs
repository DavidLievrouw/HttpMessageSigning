using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class KeyIdTests {
        public class Construction : KeyIdTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptyKeyId() {
                var actual = new KeyId();
                actual.Should().Be(KeyId.Empty);
            }

            [Fact]
            public void Constructor_CreatesKeyIdWithValue() {
                var actual = new KeyId("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptyKeyId() {
                var actual = new KeyId(null);
                actual.Should().Be(KeyId.Empty);
            }
        }

        public class Equality : KeyIdTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new KeyId("abc123");
                var second = new KeyId(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new KeyId("abc123");
                var second = new KeyId("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new KeyId("abc123");
                var second = new KeyId("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new KeyId("");
                var second = new KeyId("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new KeyId("");
                var second = KeyId.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotAKeyId_AreNotEqual() {
                var first = new KeyId("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : KeyIdTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new KeyId(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : KeyIdTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new KeyId(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new KeyId(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (KeyId) str;
                var expected = new KeyId(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptyKeyId() {
                string nullString = null;
                var actual = (KeyId) nullString;
                actual.Should().NotBeNull().And.BeOfType<KeyId>();
                actual.Should().Be(KeyId.Empty);
            }
        }

        public class TryParse : KeyIdTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("abc123")]
            public void ReturnsTrue(string value) {
                var actual = KeyId.TryParse(value, out _);
                actual.Should().BeTrue();
            }
            
            [Theory]
            [InlineData(null, "")]
            [InlineData("", "")]
            [InlineData("abc123", "abc123")]
            public void Parses(string value, string expected) {
                KeyId.TryParse(value, out var actual);
                actual.Should().Be((KeyId) expected);
            }
        }
    }
}