using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        //internal static void Main(string[] args) {
        //    var summary = BenchmarkRunner.Run<SignRequestWithoutDigest>();
        //}

        internal static async Task Main(string[] args) {
            var testCase = new SignRequestWithoutDigest();
            await testCase.SignAThousandTimes();
        }
    }
}