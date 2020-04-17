using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        /*internal static void Main(string[] args) {
            //var summary = BenchmarkRunner.Run<SignRequestWithoutDigest>();
            //var summary = BenchmarkRunner.Run<SignRequestWithDigest>();
            //var summary = BenchmarkRunner.Run<VerifyRequestWithoutDigest>();
            var summary = BenchmarkRunner.Run<VerifyRequestWithDigest>();
        }*/

        internal static async Task Main(string[] args) {
            //await new SignRequestWithoutDigest().SignABunchOfTimes();
            //await new SignRequestWithDigest().SignABunchOfTimes();
            await new VerifyRequestWithoutDigest().VerifyABunchOfTimes();
            //await new VerifyRequestWithDigest().VerifyABunchOfTimes();
        }
    }
}