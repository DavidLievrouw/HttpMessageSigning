using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        /*internal static void Main(string[] args) {
            //var summary1 = BenchmarkRunner.Run<SignRequestWithoutDigest>();
            //var summary2 = BenchmarkRunner.Run<SignRequestWithDigest>();
            //var summary3 = BenchmarkRunner.Run<VerifyRequestWithoutDigest>();
            //var summary4 = BenchmarkRunner.Run<VerifyRequestWithDigest>();
        }*/

        /*internal static async Task Main(string[] args) {
            await new SignRequestWithoutDigest().SignABunchOfTimes();
            await new SignRequestWithDigest().SignABunchOfTimes();
            await new VerifyRequestWithoutDigest().VerifyABunchOfTimes();
            await new VerifyRequestWithDigest().VerifyABunchOfTimes();
        }*/
        
        internal static void Main(string[] args) {
            var summary1 = BenchmarkRunner.Run<SymmetricStringProtectorBenchmarks>();
        }
    }
}