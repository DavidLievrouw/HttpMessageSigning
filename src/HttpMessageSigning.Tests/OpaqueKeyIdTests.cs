using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class OpaqueKeyIdTests {
        public class Construction : OpaqueKeyIdTests {
            [Fact]
            public void DefaultConstructor_CreatesEmptyOpaqueKeyId() {
                var actual = new OpaqueKeyId();
                actual.Should().Be(OpaqueKeyId.Empty);
            }

            [Fact]
            public void Constructor_CreatesOpaqueKeyIdWithValue() {
                var actual = new OpaqueKeyId("theValue");
                actual.Value.Should().Be("theValue");
            }

            [Fact]
            public void NullValue_ReturnsEmptyOpaqueKeyId() {
                var actual = new OpaqueKeyId(null);
                actual.Should().Be(OpaqueKeyId.Empty);
            }
        }

        public class Equality : OpaqueKeyIdTests {
            [Fact]
            public void WhenValuesAreEqual_AreEqual() {
                var first = new OpaqueKeyId("abc123");
                var second = new OpaqueKeyId(first);

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValuesAreDifferent_AreNotEqual() {
                var first = new OpaqueKeyId("abc123");
                var second = new OpaqueKeyId("xyz123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreDifferentlyCased_AreNotEqual() {
                var first = new OpaqueKeyId("abc123");
                var second = new OpaqueKeyId("aBc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenValuesAreEmpty_AreEqual() {
                var first = new OpaqueKeyId("");
                var second = new OpaqueKeyId("");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenValueIsEmpty_IsEqualToEmpty() {
                var first = new OpaqueKeyId("");
                var second = OpaqueKeyId.Empty;

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void GivenOtherObjectIsNotOpaqueKeyId_AreNotEqual() {
                var first = new OpaqueKeyId("abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }
        }

        public class ToStringRepresentation : OpaqueKeyIdTests {
            [Fact]
            public void ReturnsValue() {
                var value = Guid.NewGuid().ToString();
                var actual = new OpaqueKeyId(value).ToString();
                actual.Should().Be(value);
            }
        }

        public class ConversionOperatorsForString : OpaqueKeyIdTests {
            [Fact]
            public void IsExplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new OpaqueKeyId(value);
                var actual = (string) obj;
                actual.Should().Be(value);
            }    
            
            [Fact]
            public void IsImplicitlyConvertibleToString() {
                var value = Guid.NewGuid().ToString();
                var obj = new OpaqueKeyId(value);
                string actual = obj;
                actual.Should().Be(value);
            }

            [Fact]
            public void IsExplicitlyConvertibleFromString() {
                var value = Guid.NewGuid().ToString();
                var str = value;
                var actual = (OpaqueKeyId) str;
                var expected = new OpaqueKeyId(value);
                actual.Should().Be(expected);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullString_ReturnsEmptyOpaqueKeyId() {
                string nullString = null;
                var actual = (OpaqueKeyId) nullString;
                actual.Should().NotBeNull().And.BeOfType<OpaqueKeyId>();
                actual.Should().Be(OpaqueKeyId.Empty);
            }
        }
    }
}