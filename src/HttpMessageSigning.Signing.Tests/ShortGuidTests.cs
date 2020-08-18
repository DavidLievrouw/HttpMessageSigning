using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class ShortGuidTests {
        public class Empty : ShortGuidTests {
            [Fact]
            public void ReturnsEmptyResult() {
                var actual = ShortGuid.Empty;
                actual.Value.Should().Be("AAAAAAAAAAAAAAAAAAAAAA");
                actual.Guid.Should().Be(Guid.Empty);
            }

            [Fact]
            public void EmptyEqualsEmpty() {
                var left = ShortGuid.Empty;
                var right = ShortGuid.Empty;
                left.Equals(right).Should().BeTrue();
                left.GetHashCode().Should().Be(right.GetHashCode());
            }
        }

        public class Construction : ShortGuidTests {
            [Fact]
            public void GivenNullString_Throws() {
                Action act = () => new ShortGuid(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void DefaultConstructor_CreatesEmpty() {
                var actual = new ShortGuid();
                actual.Equals(ShortGuid.Empty).Should().BeTrue();
                actual.GetHashCode().Should().Be(ShortGuid.Empty.GetHashCode());
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(" \t ")]
            public void GivenEmptyOrWhiteSpaceString_ThrowsFormatException(string emptyOrWhiteSpace) {
                Action act = () => new ShortGuid(emptyOrWhiteSpace);
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void GivenValidInputString_AssignsToValueProperty() {
                var stringValue = "5PKCts_THEmy8M_4519H1g";
                var sut = new ShortGuid(stringValue);
                sut.Value.Should().Be(stringValue);
            }

            [Fact]
            public void GivenValidGuid_AssignsToGuidProperty() {
                var guidValue = Guid.NewGuid();
                var sut = new ShortGuid(guidValue);
                sut.Guid.Should().Be(guidValue);
            }
        }

        public class ToStringTests : ShortGuidTests {
            [Fact]
            public void ReturnsStringValue() {
                var stringValue = "5PKCts_THEmy8M_4519H1g";
                var sut = new ShortGuid(stringValue);
                sut.ToString().Should().Be(stringValue);
            }
        }

        public class NewGuid : ShortGuidTests {
            [Fact]
            public void CreatesNonEmptyValue() {
                var actual = ShortGuid.NewGuid();
                actual.Equals(ShortGuid.Empty).Should().BeFalse();
            }

            [Fact]
            public void CreatesANewValueEachTime() {
                var first = ShortGuid.NewGuid();
                var second = ShortGuid.NewGuid();
                first.Equals(second).Should().BeFalse();
            }
        }

        public class ConversionOperators : ShortGuidTests {
            [Fact]
            public void CanRoundTripShortGuidToStringAndBack() {
                var sut = ShortGuid.NewGuid();
                var str = (string) sut;
                var sg = (ShortGuid) str;
                sg.Equals(sut).Should().BeTrue();
                sg.GetHashCode().Should().Be(sut.GetHashCode());
            }

            [Fact]
            public void CanRoundTripShortGuidToGuidAndBack() {
                var sut = ShortGuid.NewGuid();
                var guid = (Guid) sut;
                var sg = (ShortGuid) guid;
                sg.Equals(sut).Should().BeTrue();
                sg.GetHashCode().Should().Be(sut.GetHashCode());
            }

            [Fact]
            public void CanRoundTripGuidToShortGuidAndBack() {
                var guid = Guid.NewGuid();
                var sut = (ShortGuid) guid;
                var roundTripResult = (Guid) sut;
                roundTripResult.Should().Be(guid);
            }

            [Fact]
            public void CanRoundTripStringToShortGuidAndBack() {
                var str = "5PKCts_THEmy8M_4519H1g";
                var sut = (ShortGuid) str;
                var roundTripResult = (string) sut;
                roundTripResult.Should().Be(str);
            }
        }

        public class Equality : ShortGuidTests {
            public class ShortGuidEquality : Equality {
                [Fact]
                public void InstancesWithIdenticalInnerGuid_AreEqual() {
                    var guid = Guid.NewGuid();
                    var item1 = new ShortGuid(guid);
                    var item2 = new ShortGuid(guid);

                    item1.Equals(item2).Should().BeTrue();
                    item1.Equals((object) item2).Should().BeTrue();
                    (item1 == item2).Should().BeTrue();
                    (item1 != item2).Should().BeFalse();
                    item1.GetHashCode().Should().Be(item2.GetHashCode());
                }

                [Fact]
                public void InstancesWithDifferentInnerGuid_AreNotEqual() {
                    var item1 = new ShortGuid(Guid.NewGuid());
                    var item2 = new ShortGuid(Guid.NewGuid());

                    item1.Equals(item2).Should().BeFalse();
                    item1.Equals((object) item2).Should().BeFalse();
                    (item1 == item2).Should().BeFalse();
                    (item1 != item2).Should().BeTrue();
                }

                [Fact]
                public void EmptyInstances_AreEqual() {
                    var item1 = new ShortGuid();
                    var item2 = new ShortGuid();

                    item1.Equals(item2).Should().BeTrue();
                    item1.Equals((object) item2).Should().BeTrue();
                    (item1 == item2).Should().BeTrue();
                    (item1 != item2).Should().BeFalse();
                    item1.GetHashCode().Should().Be(item2.GetHashCode());
                }
            }

            public class GuidEquality : Equality {
                [Fact]
                public void InstancesWithIdenticalGuidRepresentation_AreEqual() {
                    var guid = Guid.NewGuid();
                    var sut = new ShortGuid(guid);

                    sut.Equals(guid).Should().BeTrue();
                    sut.Equals((object) guid).Should().BeTrue();
                    (sut == guid).Should().BeTrue();
                    (sut != guid).Should().BeFalse();
                }

                [Fact]
                public void InstancesWithDifferentGuidRepresentation_AreNotEqual() {
                    var guid1 = Guid.NewGuid();
                    var guid2 = Guid.NewGuid();
                    var sut = new ShortGuid(guid1);

                    sut.Equals(guid2).Should().BeFalse();
                    sut.Equals((object) guid2).Should().BeFalse();
                    (sut == guid2).Should().BeFalse();
                    (sut != guid2).Should().BeTrue();
                }
            }

            public class StringEquality : Equality {
                [Fact]
                public void InstancesWithIdenticalStringRepresentation_AreEqual() {
                    var str = "5PKCts_THEmy8M_4519H1g";
                    var sut = new ShortGuid(str);

                    sut.Equals(str).Should().BeTrue();
                    sut.Equals((object) str).Should().BeTrue();
                    (sut == str).Should().BeTrue();
                    (sut != str).Should().BeFalse();
                }

                [Fact]
                public void InstancesWithDifferentStringRepresentation_AreNotEqual() {
                    var str1 = "5PKCts_THEmy8M_4519H1g";
                    var str2 = "3TMPYxN4sU+8LZ+KDmqwhw";
                    var sut = new ShortGuid(str1);

                    sut.Equals(str2).Should().BeFalse();
                    sut.Equals((object) str2).Should().BeFalse();
                    (sut == str2).Should().BeFalse();
                    (sut != str2).Should().BeTrue();
                }

                [Fact]
                public void InstancesWithIncorrectlyCasedStringRepresentation_AreNotEqual() {
                    var str1 = "5PKCts_THEmy8M_4519H1g";
                    var str2 = "5pKCts_THEmy8M_4519H1g";
                    var sut = new ShortGuid(str1);

                    sut.Equals(str2).Should().BeFalse();
                    sut.Equals((object) str2).Should().BeFalse();
                    (sut == str2).Should().BeFalse();
                    (sut != str2).Should().BeTrue();
                }
            }
        }
    }
}