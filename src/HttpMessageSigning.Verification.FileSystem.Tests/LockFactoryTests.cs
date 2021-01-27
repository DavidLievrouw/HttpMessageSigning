using FluentAssertions;
using Nito.AsyncEx;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class LockFactoryTests {
        private readonly LockFactory _sut;

        public LockFactoryTests() {
            _sut = new LockFactory();
        }

        public class CreateLock : LockFactoryTests {
            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.CreateLock();
                actual.Should().NotBeNull().And.BeAssignableTo<AsyncReaderWriterLock>();
            }
        }
    }
}