using System;
using CommandLine;
using Serilog;
using Serilog.Events;

namespace Conformance {
    public class Program {
        private static int Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    outputTemplate: "{Message}",
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "Conformance.log"))
                .CreateLogger();

            var input = "";
            try {
                var _ = Console.KeyAvailable;
            }
            catch (InvalidOperationException) {
                input = Console.In.ReadToEnd();
            }
            
            return Parser.Default.ParseArguments<CanonicalizeOptions, SignOptions, VerifyOptions>(args)
                .MapResult(
                    (CanonicalizeOptions opts) => new Canonicalizer().Run(opts, input).GetAwaiter().GetResult(),
                    (SignOptions opts) => new Signer().Run(opts, input).GetAwaiter().GetResult(),
                    (VerifyOptions opts) => new Verifier().Run(opts, input).GetAwaiter().GetResult(),
                    errs => 1);
        }
    }
}