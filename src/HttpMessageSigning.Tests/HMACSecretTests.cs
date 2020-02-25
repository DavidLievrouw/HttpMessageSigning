using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HMACSecretTests {
        public class Construction : HMACSecretTests {
            [Fact]
            public void Constructor_CreatesSecretWithValue() {
                var actual = new HMACSecret("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptySecret() {
                var actual = new HMACSecret(null);
                actual.Should().Be(HMACSecret.Empty);
            }
        }

        public class Equality : HMACSecretTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new HMACSecret("abc123");
                var second = new HMACSecret("abc123");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new HMACSecret("abc123");
                var second = new HMACSecret("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new HMACSecret("abc123");
                var second = new HMACSecret("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new HMACSecret("");
                var second = new HMACSecret("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new HMACSecret("");
                var second = HMACSecret.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotASecret_AreNotEqual() {
                var first = new HMACSecret("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new HMACSecret("abc123");
                var second = new InheritedHMACSecret("abc123");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedHMACSecret : HMACSecret {
                public InheritedHMACSecret(string value) : base(value) { }
            }
        }

        public class ToStringRepresentation : HMACSecretTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new HMACSecret(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : HMACSecretTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new HMACSecret(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new HMACSecret(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (HMACSecret) str;
                var expected = new HMACSecret(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptySecret() {
                string nullString = null;
                var actual = (HMACSecret) nullString;
                actual.Should().NotBeNull().And.BeOfType<HMACSecret>();
                actual.Should().Be(HMACSecret.Empty);
            }
        }
    }
}