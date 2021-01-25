using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    public class ClientStoreBaselinerTests : MongoIntegrationTest, IDisposable {
        private readonly ClientStoreBaseliner _sut;
        private readonly string _collectionName;

        public ClientStoreBaselinerTests(MongoSetup mongoSetup)
            : base(mongoSetup) {
            _collectionName = "clients_" + Guid.NewGuid();
            _sut = new ClientStoreBaseliner(
                new RealSystemClock(),
                new MongoDatabaseClientProvider(Database),
                new MongoDbClientStoreSettings {CollectionName = _collectionName});
        }

        public void Dispose() {
            Database.DropCollection(_collectionName);
        }

        [Fact]
        public async Task WhenThereIsNoBaseline_ReturnsNull() {
            var actual = await _sut.GetBaseline();
            actual.Should().BeNull();
        }

        [Fact]
        public async Task CanWriteInitialBaseline() {
            var step = new FakeClientStoreMigrationStep(1);
            await _sut.SetBaseline(step);

            var actual = await _sut.GetBaseline();

            actual.Should().Be(1);
        }

        [Fact]
        public async Task CanWriteSubsequentBaseline() {
            var initialBaseline = 1;
            var initialStep = new FakeClientStoreMigrationStep(initialBaseline);
            await _sut.SetBaseline(initialStep);
            var subsequentBaseline = 2;
            var subsequentStep = new FakeClientStoreMigrationStep(subsequentBaseline);
            await _sut.SetBaseline(subsequentStep);

            var actual = await _sut.GetBaseline();

            actual.Should().Be(subsequentBaseline);
        }

        [Fact]
        public async Task WhenWritingExistingBaseline_DoesNotThrow() {
            var initialBaseline = 1;
            var initialStep = new FakeClientStoreMigrationStep(initialBaseline);
            await _sut.SetBaseline(initialStep);

            var subsequentBaseline1 = 2;
            var subsequentStep1 = new FakeClientStoreMigrationStep(subsequentBaseline1);
            await _sut.SetBaseline(subsequentStep1);

            var subsequentBaseline2 = 2;
            var subsequentStep2 = new FakeClientStoreMigrationStep(subsequentBaseline2);
            Func<Task> act = () => _sut.SetBaseline(subsequentStep2);
            act.Should().NotThrow();
        }

        [Fact]
        public async Task WhenThereIsExactlyOneBaseline_ReturnsItsVersion() {
            var initialBaseline = 1;
            var initialStep = new FakeClientStoreMigrationStep(initialBaseline);
            await _sut.SetBaseline(initialStep);

            var actual = await _sut.GetBaseline();

            actual.Should().Be(initialBaseline);
        }

        [Fact]
        public async Task WhenThereAreMultipleBaselines_ReturnsLatestVersion() {
            var initialBaseline = 1;
            var initialStep = new FakeClientStoreMigrationStep(initialBaseline);
            await _sut.SetBaseline(initialStep);
            var subsequentBaseline = 2;
            var subsequentStep = new FakeClientStoreMigrationStep(subsequentBaseline);
            await _sut.SetBaseline(subsequentStep);

            var actual = await _sut.GetBaseline();

            actual.Should().Be(subsequentBaseline);
        }

        [Fact]
        public async Task WhenWritingBaselineThatIsLowerThanCurrentBaseline_ThrowsInvalidOperationException() {
            var initialBaseline = 1;
            var initialStep = new FakeClientStoreMigrationStep(initialBaseline);
            await _sut.SetBaseline(initialStep);
            var subsequentBaseline = 3;
            var subsequentStep = new FakeClientStoreMigrationStep(subsequentBaseline);
            await _sut.SetBaseline(subsequentStep);

            var lowerBaseline = 2;
            var lowerStep = new FakeClientStoreMigrationStep(lowerBaseline);
            Func<Task> act = () => _sut.SetBaseline(lowerStep);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}