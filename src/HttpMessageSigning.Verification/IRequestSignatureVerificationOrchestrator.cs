using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface IRequestSignatureVerificationOrchestrator : IDisposable {
        Task<RequestSignatureVerificationResult> VerifySignature(HttpRequestForSigning request);
    }
}