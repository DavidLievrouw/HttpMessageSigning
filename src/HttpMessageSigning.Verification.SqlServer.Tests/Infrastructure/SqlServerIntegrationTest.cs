using System;
using System.Linq;
using System.Transactions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure {
    [Collection(nameof(SqlServerCollection))]
    public class SqlServerIntegrationTest : IDisposable {
        private readonly TransactionScope _transactionScope;

        public SqlServerIntegrationTest(SqlServerFixture fixture) {
            var transactionScopeEnabled = !GetType().GetCustomAttributes(typeof(DisableTransactionScopeAttribute), inherit: true).Any();

            if (transactionScopeEnabled) {
                _transactionScope = new TransactionScope(
                    TransactionScopeOption.RequiresNew,
                    new TransactionOptions {
                        IsolationLevel = IsolationLevel.ReadCommitted
                    },
                    TransactionScopeAsyncFlowOption.Enabled);
            }
        }

        public virtual void Dispose() {
            Transaction.Current?.Rollback();
            _transactionScope?.Dispose();
        }
    }
}