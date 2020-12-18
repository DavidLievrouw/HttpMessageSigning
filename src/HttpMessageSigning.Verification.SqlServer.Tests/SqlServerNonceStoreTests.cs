using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    [DisableTransactionScope]
    public class SqlServerNonceStoreTests : SqlServerIntegrationTest {
        public SqlServerNonceStoreTests(SqlServerFixture fixture)
            : base(fixture) { }

        [Fact]
        public void DoSomething() {
            true.Should().BeFalse();
        }
        
        [Fact]
        public void DoSomethingElse() {
            true.Should().BeFalse();
        }
    }
}