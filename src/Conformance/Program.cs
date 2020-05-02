using System;
using CommandLine;
using Serilog;

namespace Conformance {
    public class Program {
        private static void Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "{Message}")
                .WriteTo.File("Conformance.log")
                .CreateLogger();

            var input = Console.ReadLine()?.Replace("{NEWLINE}", "\n").Trim('"');

            Parser.Default.ParseArguments<CanonicalizeOptions, SignOptions, VerifyOptions>(args)
                .MapResult(
                    (CanonicalizeOptions opts) => new Canonicalizer().Run(opts, input).GetAwaiter().GetResult(),
                    (SignOptions opts) => new Signer().Run(opts, input).GetAwaiter().GetResult(),
                    (VerifyOptions opts) => new Verifier().Run(opts, input).GetAwaiter().GetResult(),
                    errs => 1);
        }
    }
}