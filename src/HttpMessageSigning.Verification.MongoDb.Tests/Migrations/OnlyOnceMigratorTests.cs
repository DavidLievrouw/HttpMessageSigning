using System;
using System.Threading;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    public class OnlyOnceMigratorTests : IDisposable {
        private readonly IBaseliner _baseliner;
        private readonly IMigrator _decorated;
        private readonly ISemaphoreFactory _semaphoreFactory;
        private readonly SemaphoreSlim _semaphore;
        private readonly OnlyOnceMigrator _sut;

        public OnlyOnceMigratorTests() {
            FakeFactory.Create(out _decorated, out _baseliner, out _semaphoreFactory);
            _semaphore = new SemaphoreSlim(1, 1);
            A.CallTo(() => _semaphoreFactory.CreateLock())
                .Returns(_semaphore);
            _sut = new OnlyOnceMigrator(_decorated, _baseliner, _semaphoreFactory);
        }

        public void Dispose() {
            _semaphore?.Dispose();
        }

        public class Migrate : OnlyOnceMigratorTests {
            private readonly int _baselineBeforeMigration;

            public Migrate() {
                _baselineBeforeMigration = 42;
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(_baselineBeforeMigration);
            }

            [Fact]
            public async Task WhenMigrationIsAlreadyRunning_DoesNotMigrate_ReturnsBaselineBeforeMigration() {
                await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(10), CancellationToken.None);

                try {
                    var actual = await _sut.Migrate();

                    actual.Should().Be(_baselineBeforeMigration);
                    A.CallTo(() => _decorated.Migrate())
                        .MustNotHaveHappened();
                }
                finally {
                    _semaphore.Release();
                }
            }

            [Fact]
            public async Task WhenMigrationHasAlreadyCompleted_DoesNotExecuteItAgain_ReturnsCurrentBaseline() {
                var baselineAfterMigration = _baselineBeforeMigration + 1;
                A.CallTo(() => _decorated.Migrate())
                    .Returns(baselineAfterMigration);

                // Perform initial migration
                var actual1 = await _sut.Migrate();
                actual1.Should().Be(baselineAfterMigration);

                A.CallTo(() => _decorated.Migrate())
                    .MustHaveHappenedOnceExactly();

                // Perform migration again
                var actual2 = await _sut.Migrate();
                actual2.Should().Be(baselineAfterMigration);

                A.CallTo(() => _decorated.Migrate())
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ReturnsResultFromDecoratedService() {
                var baselineAfterMigration = _baselineBeforeMigration + 1;
                A.CallTo(() => _decorated.Migrate())
                    .Returns(baselineAfterMigration);

                // Perform initial migration
                var actual1 = await _sut.Migrate();
                actual1.Should().Be(baselineAfterMigration);
            }
        }
    }
}