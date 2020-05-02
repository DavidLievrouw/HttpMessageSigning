using CommandLine;

namespace Conformance {
    public class OptionsBase {
        [Option('d', "headers", Required = false)]
        public string Headers { get; set; }

        [Option('c', "created", Required = false)]
        public string Created { get; set; }

        [Option('e', "expires", Required = false)]
        public string Expires { get; set; }
    }
}