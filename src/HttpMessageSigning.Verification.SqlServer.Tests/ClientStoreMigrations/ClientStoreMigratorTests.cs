using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    public class ClientStoreMigratorTests {
        private readonly IClientStoreBaseliner _baseliner;
        private readonly IEnumerable<FakeClientStoreMigrationStep> _migrationSteps;
        private readonly ClientStoreMigrator _sut;

        public ClientStoreMigratorTests() {
            FakeFactory.Create(out _baseliner);
            _migrationSteps = new[] {
                new FakeClientStoreMigrationStep(2, () => Task.Delay(TimeSpan.FromMilliseconds(100))),
                new FakeClientStoreMigrationStep(4, () => Task.Delay(TimeSpan.FromMilliseconds(100))),
                new FakeClientStoreMigrationStep(3, () => Task.Delay(TimeSpan.FromMilliseconds(100))),
            };
            _sut = new ClientStoreMigrator(_migrationSteps, _baseliner);
        }

        public class Migrate : ClientStoreMigratorTests {
            [Fact]
            public async Task WhenThereIsNoBaselineYet_RunsAllSteps() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(Task.FromResult<int?>(null));

                var actual = await _sut.Migrate();

                actual.Should().Be(4);
                foreach (var migrationStep in _migrationSteps) {
                    migrationStep.Runs.Count().Should().Be(1);
                }
            }

            [Fact]
            public async Task WhenThereIsNoBaselineYet_AndThereAreNoSteps_RunsZero() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(Task.FromResult<int?>(null));

                var sut = new ClientStoreMigrator(Enumerable.Empty<IClientStoreMigrationStep>(), _baseliner);

                var actual = await sut.Migrate();

                actual.Should().Be(0);
            }

            [Fact]
            public async Task RunsStepsInOrder() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(Task.FromResult<int?>(null));

                await _sut.Migrate();

                var actualOrder = _migrationSteps.OrderBy(_ => _.Runs.Single());
                var expectedOrder = _migrationSteps.OrderBy(_ => _.Version);

                actualOrder.Should().BeEquivalentTo(expectedOrder, options => options.WithStrictOrdering());
            }

            [Fact]
            public async Task OnlyExecutesNewerVersionSteps() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(2);

                await _sut.Migrate();

                var expectedMigrations = _migrationSteps.Where(_ => _.Version > 2);
                foreach (var migrationStep in expectedMigrations) {
                    migrationStep.Runs.Count().Should().Be(1);
                }
            }

            [Fact]
            public async Task ReturnsLatestAppliedVersion() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(2);

                var actual = await _sut.Migrate();

                actual.Should().Be(4);
            }

            [Fact]
            public void WhenAVersionStepFails_Throws_AndDoesNotTryOtherSteps() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(1);

                var failingStep = _migrationSteps.Single(_ => _.Version == 3);
                var failure = new MongoException("Failed!");
                failingStep.SetFailure(failure);

                Func<Task> action = () => _sut.Migrate();

                action.Should().Throw<MongoException>().Where(_ => _ == failure);

                var expectedUnexecutedSteps = _migrationSteps.Where(_ => _.Version > 3);
                foreach (var migrationStep in expectedUnexecutedSteps) {
                    migrationStep.Runs.Count().Should().Be(0);
                    A.CallTo(() => _baseliner.SetBaseline(migrationStep))
                        .MustNotHaveHappened();
                }
            }

            [Fact]
            public void WhenAVersionStepFails_UpdatesBaselineToLastSucceededStep() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(1);

                var failingStep = _migrationSteps.Single(_ => _.Version == 3);
                var failure = new MongoException("Failed!");
                failingStep.SetFailure(failure);

                Func<Task> action = () => _sut.Migrate();

                action.Should().Throw<MongoException>();

                var expectedExecutedStep = _migrationSteps.Single(_ => _.Version == 2);
                A.CallTo(() => _baseliner.SetBaseline(expectedExecutedStep))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenNoNewerVersionsAreFound_DoesNothing_ReturnsCurrentBaseline() {
                A.CallTo(() => _baseliner.GetBaseline())
                    .Returns(4);

                var actual = await _sut.Migrate();

                actual.Should().Be(4);
                foreach (var migrationStep in _migrationSteps) {
                    migrationStep.Runs.Count().Should().Be(0);
                }
            }
        }
    }
}