using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        internal static Task<TResult> ToTask<TResult>(this TResult result) {
            return Task.FromResult(result);
        }
    }
}