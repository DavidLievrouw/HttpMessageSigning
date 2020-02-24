using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SecretTests {
        public class Construction : SecretTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptySecret() {
                var actual = new Secret();
                actual.Should().Be(Secret.Empty);
            }

            [Fact]
            public void Constructor_CreatesSecretWithValue() {
                var actual = new Secret("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptySecret() {
                var actual = new Secret(null);
                actual.Should().Be(Secret.Empty);
            }
        }

        public class Equality : SecretTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new Secret("abc123");
                var second = new Secret(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new Secret("abc123");
                var second = new Secret("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new Secret("abc123");
                var second = new Secret("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new Secret("");
                var second = new Secret("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new Secret("");
                var second = Secret.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotASecret_AreNotEqual() {
                var first = new Secret("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : SecretTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new Secret(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : SecretTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new Secret(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new Secret(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (Secret) str;
                var expected = new Secret(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptySecret() {
                string nullString = null;
                var actual = (Secret) nullString;
                actual.Should().NotBeNull().And.BeOfType<Secret>();
                actual.Should().Be(Secret.Empty);
            }
        }
    }
}