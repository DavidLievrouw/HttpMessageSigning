using CommandLine;

namespace Conformance {
    [Verb("verify")]
    public class VerifyOptions : OptionsBase {
        [Option('u', "public-key", Required = false)]
        public string PublicKey { get; set; }

        [Option('k', "keyId", Required = false)]
        public string KeyId { get; set; }

        [Option('t', "key-type", Required = false)]
        public string KeyType { get; set; }

        [Option('a', "algorithm", Required = false)]
        public string Algorithm { get; set; }
    }
}