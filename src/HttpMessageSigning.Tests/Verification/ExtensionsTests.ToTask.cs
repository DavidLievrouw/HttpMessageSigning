using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public partial class ExtensionsTests {
        public class ToTask : AspNetCore.ExtensionsTests {
            [Fact]
            public async Task ReturnsSpecifiedValueWrappedInTask() {
                var value = "TheValue";
                var actual = value.ToTask();
                actual.Should().BeAssignableTo<Task<string>>();
                var wrappedValue = await actual;
                wrappedValue.Should().BeSameAs(value);
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public async Task CanWrapNullValues() {
                string value = null;
                var actual = value.ToTask();
                actual.Should().BeAssignableTo<Task<string>>();
                var wrappedValue = await actual;
                wrappedValue.Should().BeNull();
            }
        }
    }
}