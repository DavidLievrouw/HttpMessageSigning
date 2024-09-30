using BenchmarkDotNet.Running;

namespace Benchmark {
    internal class Program {
        internal static void Main(string[] args) {
            var summary1 = BenchmarkRunner.Run<SignRequestWithoutDigest>();
        }
    }
}