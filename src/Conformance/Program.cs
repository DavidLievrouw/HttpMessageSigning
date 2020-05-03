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

            /*var msg = @"POST /foo?param=value&pet=dog HTTP/1.1
Host: example.com
Zero:   
Content-Type: application/json
Digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=
Content-Length: 18
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Canonicalizer().Run(new CanonicalizeOptions {
                Headers = "zero"
            }, msg).GetAwaiter().GetResult();*/
            
            /*var msg = @"GET /basic/request HTTP/1.1
Connection: keep-alive
User-Agent: Mozilla/5.0 (Macintosh)
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Signer().Run(new SignOptions {
                KeyId = "test",
                Headers = "date",
                KeyType = "rsa",
                PrivateKey = "C:\\git\\http-signatures-test-suite\\test\\keys\\rsa.private",
                Algorithm = "unknown"
            }, msg).GetAwaiter().GetResult();*/
            
            /*var msg = @"GET /basic/request HTTP/1.1
Host: example.com
Content-Type: application/json
Digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=
Content-Length: 18
Authorization: Signature keyId=""test"",headers=""host digest"",signature=""QBWMdoeHrbefTrFPyOxjGOp0Q7Mq/w/NRVaMvzcyS+3QuhTlc4zUL8g4q8Woc5b0ynoFeDpmxMoqzgLFiPGjJRKPnVxTAEL2HKAG9JdZQdflZSBIFqoszrrvYlGOZ84JMUGQpIF0OAuKZBR1YhAvFTOcW3Et7wfbqAna6KOxi3k=""
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Verifier().Run(new VerifyOptions {
                KeyId = "test",
                Headers = "not-in-request",
                KeyType = "rsa",
                PublicKey = "C:\\git\\http-signatures-test-suite\\test\\keys\\rsa.pub",
                Algorithm = "unknown"
            }, msg).GetAwaiter().GetResult();*/
            
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