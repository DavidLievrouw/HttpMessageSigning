using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class HeaderDictionaryTests {
        private readonly HeaderDictionary _sut;

        public HeaderDictionaryTests() {
            _sut = new HeaderDictionary(new Dictionary<string, StringValues> {
                {"h1", "v1"},
                {"h2", new StringValues(new[] {"v2", "v3"})},
                {"h3", ""}
            });
        }

        public class Constructor : HeaderDictionaryTests {
            [Fact]
            public void GivenDictionaryWithItems_WrapsInCaseInsensitiveDictionary() {
                var sut = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.InvariantCulture) {
                    {"h1", "v1"},
                    {"h2", new StringValues(new[] {"v2", "v3"})},
                    {"h3", ""}
                });

                var item = sut["H1"];

                item.Should().Equal(new StringValues(new[] {"v2"}));
            }
        }

        public class Indexer : HeaderDictionaryTests {
            [Fact]
            public void WhenRequestedHeaderDoesNotExist_ReturnsEmpty() {
                var item = _sut["IDontExist"];
                item.Should().Equal(StringValues.Empty);
            }

            [Fact]
            public void WhenRequestedItemExists_ReturnsThatItem() {
                var item = _sut["h1"];
                item.Should().Equal(new StringValues(new[] {"v1"}));
            }

            [Fact]
            public void ResolvesItemCaseInsensitively() {
                var item = _sut["H1"];
                item.Should().Equal(new StringValues(new[] {"v1"}));
            }

            [Fact]
            public void CanAddItem() {
                _sut["SomethingElse"] = new StringValues(new[] {"some_value"});
                var item = _sut["SomethingElse"];
                item.Should().Equal(new StringValues(new[] {"some_value"}));
            }

            [Fact]
            public void CanOverwriteItem() {
                _sut["h1"] = new StringValues(new[] {"some_value"});
                var item = _sut["h1"];
                item.Should().Equal(new StringValues(new[] {"some_value"}));
            }

            [Fact]
            public void SetsItemCaseInsensitively() {
                _sut["H1"] = new StringValues(new[] {"some_value"});
                var item = _sut["h1"];
                item.Should().Equal(new StringValues(new[] {"some_value"}));
            }
        }

        public class Contains : HeaderDictionaryTests {
            [Fact]
            public void WhenTheItemExists_ReturnsTrue() {
                var actual = _sut.Contains("h2");
                actual.Should().BeTrue();
            }

            [Fact]
            public void WhenTheItemExists_ButIncorrectlyCased_ReturnsTrue() {
                var actual = _sut.Contains("H2");
                actual.Should().BeTrue();
            }

            [Fact]
            public void WhenTheItemDoesNotExist_ReturnsFalse() {
                var actual = _sut.Contains("something_else");
                actual.Should().BeFalse();
            }
        }
        
        public class Remove : HeaderDictionaryTests {
            [Fact]
            public void WhenTheItemExists_RemovesTheItem() {
                var countBefore = _sut.Count;
                _sut.Remove("h2");
                var countAfter = _sut.Count;
                countAfter.Should().Be(countBefore - 1);
            }
            
            [Fact]
            public void WhenTheItemExists_ReturnsTrue() {
                var actual = _sut.Remove("h2");
                actual.Should().BeTrue();
            }

            [Fact]
            public void WhenTheItemExists_ButIncorrectlyCased_ReturnsTrue() {
                var actual = _sut.Remove("H2");
                actual.Should().BeTrue();
            }

            [Fact]
            public void WhenTheItemDoesNotExist_ReturnsFalse() {
                var actual = _sut.Remove("something_else");
                actual.Should().BeFalse();
            }
        }
        
        public class GetValues : HeaderDictionaryTests {
            [Fact]
            public void WhenRequestedHeaderDoesNotExist_ReturnsEmpty() {
                var item = _sut.GetValues("IDontExist");
                item.Should().Equal(StringValues.Empty);
            }

            [Fact]
            public void WhenRequestedItemExists_ReturnsThatItem() {
                var item = _sut.GetValues("h1");
                item.Should().Equal(new StringValues(new[] {"v1"}));
            }

            [Fact]
            public void ResolvesItemCaseInsensitively() {
                var item = _sut.GetValues("H1");
                item.Should().Equal(new StringValues(new[] {"v1"}));
            }
        }

        public class Clear : HeaderDictionaryTests {
            [Fact]
            public void ClearsItems() {
                _sut.Count.Should().BeGreaterThan(0);

                _sut.Clear();

                _sut.Count.Should().Be(0);
            }
        }

        public class Add : HeaderDictionaryTests {
            [Fact]
            public void CanAddItem() {
                _sut.Add("SomethingElse", new StringValues(new[] {"some_value"}));
                var item = _sut["SomethingElse"];
                item.Should().Equal(new StringValues(new[] {"some_value"}));
            }

            [Fact]
            public void WhenItemAlreadyExists_ThrowsArgumentException() {
                Action act = () => _sut.Add("h1", new StringValues(new[] {"some_value"}));
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Count : HeaderDictionaryTests {
            [Fact]
            public void ReturnsNumberOfItems() {
                _sut.Count.Should().Be(3);
            }
        }

        public class TryGetValues : HeaderDictionaryTests {
            [Fact]
            public void WhenRequestedHeaderDoesNotExist_ReturnsEmpty() {
                var succeeded = _sut.TryGetValues("IDontExist", out var values);
                succeeded.Should().BeFalse();
                values.Should().Equal(StringValues.Empty);
            }

            [Fact]
            public void WhenRequestedItemExists_ReturnsThatItem() {
                var succeeded = _sut.TryGetValues("h1", out var values);
                succeeded.Should().BeTrue();
                values.Should().Equal(new StringValues(new[] {"v1"}));
            }

            [Fact]
            public void ResolvesItemCaseInsensitively() {
                var succeeded = _sut.TryGetValues("H1", out var values);
                succeeded.Should().BeTrue();
                values.Should().Equal(new StringValues(new[] {"v1"}));
            }
        }
    }
}