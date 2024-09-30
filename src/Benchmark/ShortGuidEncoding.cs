using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Dalion.HttpMessageSigning.Signing;

namespace Benchmark;

[Config(typeof(Config))]
public class ShortGuidEncoding {
    private readonly ShortGuid _guid = ShortGuid.NewGuid();

    [Benchmark]
    public string Encode() {
        var result = ShortGuid.NewGuid();
        return result.Value;
    }

    [Benchmark]
    public string Decode() {
        var result = new ShortGuid(_guid.Value);
        return result.Value;
    }

    private class Config : ManualConfig {
        public Config() {
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}