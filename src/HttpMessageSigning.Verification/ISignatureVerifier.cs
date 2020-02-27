using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureVerifier {
        Task<Exception> VerifySignature(HttpRequestForSigning signedRequest, Signature signature, Client client);
    }
}