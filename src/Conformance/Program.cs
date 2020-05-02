using System;
using System.Linq;
using CommandLine;
using Serilog;
using Serilog.Events;

namespace Conformance {
    public class Program {
        private static int Main(string[] args) {
            // Configure logger and stdout
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    outputTemplate: "{Message}",
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "Conformance.log"))
                .CreateLogger();

            // Read piped input from test suite
            var input = "";
            try {
                var _ = Console.KeyAvailable;
            }
            catch (InvalidOperationException) {
                input = Console.In.ReadToEnd();
            }

            // Workaround for bug in CommandLineParser package: Add empty argument value if argument value is dropped
            if (args.Any() && args.Last().StartsWith("--headers")) {
                args = args.Concat(new[] {""}).ToArray();
            }
            
            // Run it
            return Parser.Default.ParseArguments<CanonicalizeOptions, SignOptions, VerifyOptions>(args)
                .MapResult(
                    (CanonicalizeOptions opts) => new Canonicalizer().Run(opts, input).GetAwaiter().GetResult(),
                    (SignOptions opts) => new Signer().Run(opts, input).GetAwaiter().GetResult(),
                    (VerifyOptions opts) => new Verifier().Run(opts, input).GetAwaiter().GetResult(),
                    errs => 1);
        }
    }
}