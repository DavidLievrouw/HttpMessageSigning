using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        internal static void Main(string[] args) {
            //var summary = BenchmarkRunner.Run<SignRequestWithoutDigest>();
            var summary = BenchmarkRunner.Run<SignRequestWithDigest>();
        }

        /*internal static async Task Main(string[] args) {
            //var testCase = new SignRequestWithoutDigest();
            var testCase = new SignRequestWithDigest();
            await testCase.SignAThousandTimes();
        }*/
    }
}