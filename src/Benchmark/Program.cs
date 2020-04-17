using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        private static async Task Main(string[] args) {
            //var summary = BenchmarkRunner.Run<SignRequestWithoutDigest>();
            
            var testCase = new SignRequestWithoutDigest();
            await testCase.SignAHundredTimes();
        }
    }
}