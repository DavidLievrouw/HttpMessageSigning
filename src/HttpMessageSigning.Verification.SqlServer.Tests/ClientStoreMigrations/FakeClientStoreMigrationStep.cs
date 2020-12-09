using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    public class FakeClientStoreMigrationStep : IClientStoreMigrationStep {
        private readonly Func<Task> _runAction;
        private Exception _failure;
        private readonly List<DateTimeOffset> _runs;

        public FakeClientStoreMigrationStep(int version, Func<Task> runAction = null) {
            _runAction = runAction;
            Version = version;
            _runs = new List<DateTimeOffset>();
        }

        public async Task Run() {
            _runs.Add(DateTimeOffset.UtcNow);
            if (_failure != default) throw _failure;
            var runActionTask = _runAction?.Invoke();
            if (runActionTask != null) await runActionTask;
        }

        public void SetFailure(Exception failure) {
            _failure = failure;
        }
        
        public int Version { get; }

        public IEnumerable<DateTimeOffset> Runs => _runs;
    }
}