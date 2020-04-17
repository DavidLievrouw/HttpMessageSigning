using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        private static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<SignRequestWithoutDigest>();
        }
    }
}