using System.Threading;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    public class SemaphoreFactoryTests {
        private readonly SemaphoreFactory _sut;

        public SemaphoreFactoryTests() {
            _sut = new SemaphoreFactory();
        }

        public class CreateLock : SemaphoreFactoryTests {
            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.CreateLock();
                actual.Should().NotBeNull().And.BeAssignableTo<SemaphoreSlim>();
            }
        }
    }
}