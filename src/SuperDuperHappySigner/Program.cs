using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace SuperDuperHappySigner {
    internal class Program {
        private static async Task Main(string[] args) {
            var keyId = new KeyId("91533c57807640579af1482d94faafe3");
            
            var services = new ServiceCollection()
                .AddHttpMessageSigning().Services;

            using (var provider = services.BuildServiceProvider()) {
                var signerFactory = provider.GetRequiredService<IRequestSignerFactory>();
                var signer = signerFactory.Create(keyId, new SigningSettings {
                    SignatureAlgorithm = SignatureAlgorithm.CreateForSigning(hmacSecret: "s3cr3t")
                });
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.dalion.eu");
                await signer.Sign(httpRequest);

                var authorizationHeader = httpRequest.Headers.Authorization;
                Console.WriteLine("{0} {1}", authorizationHeader!.Scheme, authorizationHeader!.Parameter);
            }
        }
    }
}