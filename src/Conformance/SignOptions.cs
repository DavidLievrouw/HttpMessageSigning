using CommandLine;

namespace Conformance {
    [Verb("sign")]
    public class SignOptions : OptionsBase {
        [Option('p', "private-key", Required = false)]
        public string PrivateKey { get; set; }
        
        [Option('k', "keyId", Required = false)]
        public string KeyId { get; set; }

        [Option('t', "key-type", Required = false)]
        public string KeyType { get; set; }

        [Option('a', "algorithm", Required = false)]
        public string Algorithm { get; set; }
    }
}