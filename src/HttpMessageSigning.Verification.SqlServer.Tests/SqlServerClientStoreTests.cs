using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerClientStoreTests : SqlServerIntegrationTest {
        public SqlServerClientStoreTests(SqlServerFixture fixture)
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