using CommandLine;

namespace Conformance {
    public class OptionsBase {
        [Option('V', "version", Required = false)]
        public bool Version { get; set; }

        [Option('p', "private-key", Required = false)]
        public string PrivateKey { get; set; }

        [Option('u', "public-key", Required = false)]
        public string PublicKey { get; set; }

        [Option('d', "headers", Required = false)]
        public string Headers { get; set; }

        [Option('k', "keyId", Required = false)]
        public string KeyId { get; set; }

        [Option('t', "keyType", Required = false)]
        public string KeyType { get; set; }

        [Option('a', "algorithm", Required = false)]
        public string Algorithm { get; set; }

        [Option('c', "created", Required = false)]
        public string Created { get; set; }

        [Option('e', "expires", Required = false)]
        public string Expires { get; set; }
    }
}