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
            //await new SignRequestWithoutDigest().SignAThousandTimes();
            //await new SignRequestWithDigest().SignAThousandTimes();
            await new VerifyRequestWithoutDigest().VerifyAThousandTimes();
            //await new VerifyRequestWithDigest().VerifyAThousandTimes();
        }
    }
}