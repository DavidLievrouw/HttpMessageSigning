using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    internal class ClientStoreMigrator : IClientStoreMigrator {
        private readonly IClientStoreBaseliner _baseliner;
        private readonly IEnumerable<IClientStoreMigrationStep> _migrationSteps;

        public ClientStoreMigrator(IEnumerable<IClientStoreMigrationStep> migrationSteps, IClientStoreBaseliner baseliner) {
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

        private async Task<IEnumerable<IClientStoreMigrationStep>> GetStepsToExecute() {
            var baseline = await _baseliner.GetBaseline();
            return _migrationSteps
                .OrderBy(_ => _.Version)
                .Where(_ => !baseline.HasValue || _.Version > baseline.Value);
        }
    }
}