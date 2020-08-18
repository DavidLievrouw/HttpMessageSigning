using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    public interface IRequestSignatureVerificationOrchestrator : IDisposable {
        Task<RequestSignatureVerificationResult> VerifySignature(HttpRequestForVerification request);
    }
}