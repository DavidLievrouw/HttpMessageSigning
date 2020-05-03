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

            /*var msgCanonicalize = @"GET /basic/request HTTP/1.1
Host: example.com
Content-Type: application/json
Digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=
Content-Length: 18
Authorization: Signature keyId=""test"",headers=""host digest"",signature=""G/q271XxN9vXgC4/PZj++CIA1tPRIejyrEv+Xji1JunhlVYgNVnMn13TFa47ierVyHZO+5XQV43J9m/n4gbuEwSj87x2fnp0kV82k1VaXmhpiBHaQ123jK9xToynmcIynrkj2MGCCfTotLf1Z4lCP3Eb2G3K76C+npgWNC0I44g=""
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Canonicalizer().Run(new CanonicalizeOptions {
                Headers = "host,digest"
            }, msgCanonicalize).GetAwaiter().GetResult();*/
            
            /*var msgSign = @"GET /basic/request HTTP/1.1
Host: example.com
Content-Type: application/json
Digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=
Content-Length: 18
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Signer().Run(new SignOptions {
                KeyId = "test",
                Headers = "host,digest",
                KeyType = "rsa",
                PrivateKey = "C:\\git\\http-signatures-test-suite\\test\\keys\\rsa.private",
                //Algorithm = "unknown"
            }, msgSign).GetAwaiter().GetResult();*/
            
            /*var msgVerify = @"GET /basic/request HTTP/1.1
Host: example.com
Content-Type: application/json
Digest: SHA-256=X48E9qOokqqrvdts8nOJRJN3OWDUoyWxBf7kbu9DBPE=
Content-Length: 18
Authorization: Signature keyId=""test"",headers=""host digest"",signature=""G/q271XxN9vXgC4/PZj++CIA1tPRIejyrEv+Xji1JunhlVYgNVnMn13TFa47ierVyHZO+5XQV43J9m/n4gbuEwSj87x2fnp0kV82k1VaXmhpiBHaQ123jK9xToynmcIynrkj2MGCCfTotLf1Z4lCP3Eb2G3K76C+npgWNC0I44g=""
Date: Sun, 03 May 2020 09:34:35 GMT

{""hello"": ""world""}";

            new Verifier().Run(new VerifyOptions {
                KeyId = "test",
                Headers = "host,digest",
                KeyType = "rsa",
                PublicKey = "C:\\git\\http-signatures-test-suite\\test\\keys\\rsa.pub",
                Algorithm = "hs2019"
            }, msgVerify).GetAwaiter().GetResult();*/
            
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