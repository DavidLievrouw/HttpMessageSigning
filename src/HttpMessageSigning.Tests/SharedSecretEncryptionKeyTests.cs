using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SharedSecretEncryptionKeyTests {
        public class Construction : SharedSecretEncryptionKeyTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptySharedSecretEncryptionKey() {
                var actual = new SharedSecretEncryptionKey();
                actual.Should().Be(SharedSecretEncryptionKey.Empty);
            }

            [Fact]
            public void Constructor_CreatesSharedSecretEncryptionKeyWithValue() {
                var actual = new SharedSecretEncryptionKey("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptySharedSecretEncryptionKey() {
                var actual = new SharedSecretEncryptionKey(null);
                actual.Should().Be(SharedSecretEncryptionKey.Empty);
            }
        }

        public class Equality : SharedSecretEncryptionKeyTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new SharedSecretEncryptionKey("abc123");
                var second = new SharedSecretEncryptionKey(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new SharedSecretEncryptionKey("abc123");
                var second = new SharedSecretEncryptionKey("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new SharedSecretEncryptionKey("abc123");
                var second = new SharedSecretEncryptionKey("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new SharedSecretEncryptionKey("");
                var second = new SharedSecretEncryptionKey("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new SharedSecretEncryptionKey("");
                var second = SharedSecretEncryptionKey.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotASharedSecretEncryptionKey_AreNotEqual() {
                var first = new SharedSecretEncryptionKey("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : SharedSecretEncryptionKeyTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new SharedSecretEncryptionKey(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : SharedSecretEncryptionKeyTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new SharedSecretEncryptionKey(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new SharedSecretEncryptionKey(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (SharedSecretEncryptionKey) str;
                var expected = new SharedSecretEncryptionKey(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptySharedSecretEncryptionKey() {
                string nullString = null;
                var actual = (SharedSecretEncryptionKey) nullString;
                actual.Should().NotBeNull().And.BeOfType<SharedSecretEncryptionKey>();
                actual.Should().Be(SharedSecretEncryptionKey.Empty);
            }
        }

        public class TryParse : SharedSecretEncryptionKeyTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("abc123")]
            public void ReturnsTrue(string value) {
                var actual = SharedSecretEncryptionKey.TryParse(value, out _);
                actual.Should().BeTrue();
            }
            
            [Theory]
            [InlineData(null, "")]
            [InlineData("", "")]
            [InlineData("abc123", "abc123")]
            public void Parses(string value, string expected) {
                SharedSecretEncryptionKey.TryParse(value, out var actual);
                actual.Should().Be((SharedSecretEncryptionKey) expected);
            }
        }
    }
}