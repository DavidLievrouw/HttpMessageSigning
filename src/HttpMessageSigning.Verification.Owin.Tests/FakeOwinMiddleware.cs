using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    [ExcludeFromCodeCoverage]
    public class FakeOwinMiddleware : OwinMiddleware {
        public FakeOwinMiddleware() : base(null) { }
        
        public override Task Invoke(IOwinContext context) {
            return Task.CompletedTask;
        }
    }
}