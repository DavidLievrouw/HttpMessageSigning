using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Dalion.HttpMessageSigning.Utils;

namespace Benchmark;

[Config(typeof(Config))]
public class SymmetricStringProtectorBenchmarks {
    private const string Input = "lorem ipsum dolor sit amet, consectetur adipiscing elit. sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    
    private readonly string _protected;
    private readonly IStringProtector _protector = new StringProtectorFactory().CreateSymmetric("s3cr37_4g3n7");

    public SymmetricStringProtectorBenchmarks() {
        _protected = _protector.Protect(Input);
    }

    [Benchmark]
    public string Protect() {
        return _protector.Protect(Input);
    }

    [Benchmark]
    public string Unprotect() {
        return _protector.Unprotect(_protected);
    }

    private class Config : ManualConfig {
        public Config() {
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}