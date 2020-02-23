using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class StringKeyIdTests {
        public class Construction : StringKeyIdTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptyKeyId() {
                var actual = new StringKeyId();
                actual.Should().Be(StringKeyId.Empty);
            }

            [Fact]
            public void Constructor_CreatesKeyIdWithValue() {
                var actual = new StringKeyId("theValue");
                actual.Key.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_CreatesEmptyKeyId() {
                var actual = new StringKeyId(null);
                actual.Should().Be(StringKeyId.Empty);
            }
        }

        public class Equality : StringKeyIdTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new StringKeyId("abc123");
                var second = new StringKeyId(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new StringKeyId("abc123");
                var second = new StringKeyId("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new StringKeyId("abc123");
                var second = new StringKeyId("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new StringKeyId("");
                var second = new StringKeyId("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new StringKeyId("");
                var second = StringKeyId.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotKeyId_AreNotEqual() {
                var first = new StringKeyId("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : StringKeyIdTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new StringKeyId(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : StringKeyIdTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new StringKeyId(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }       
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new StringKeyId(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (StringKeyId) str;
                var expected = new StringKeyId(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptyKeyId() {
                string nullString = null;
                var actual = (StringKeyId) nullString;
                actual.Should().NotBeNull().And.BeOfType<StringKeyId>();
                actual.Should().Be(StringKeyId.Empty);
            }
        }
    }
}