using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal class Migrator : IMigrator {
        private readonly IBaseliner _baseliner;
        private readonly IEnumerable<IMigrationStep> _migrationSteps;

        public Migrator(IEnumerable<IMigrationStep> migrationSteps, IBaseliner baseliner) {
            _migrationSteps = migrationSteps ?? throw new ArgumentNullException(nameof(migrationSteps));
            _baseliner = baseliner ?? throw new ArgumentNullException(nameof(baseliner));
        }

        public async Task<int> Migrate() {
            var lastVersion = await _baseliner.GetBaseline() ?? 0;

            var stepsToExecute = await GetStepsToExecute();

            foreach (var migrationStep in stepsToExecute) {
                await migrationStep.Run();
                await _baseliner.SetBaseline(migrationStep);
                lastVersion = migrationStep.Version;
            }

            return lastVersion;
        }

        private async Task<IEnumerable<IMigrationStep>> GetStepsToExecute() {
            var baseline = await _baseliner.GetBaseline();
            return _migrationSteps
                .OrderBy(_ => _.Version)
                .Where(_ => !baseline.HasValue || _.Version > baseline.Value);
        }
    }
}