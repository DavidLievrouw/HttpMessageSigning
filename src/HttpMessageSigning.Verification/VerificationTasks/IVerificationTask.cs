using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal interface IVerificationTask {
        Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client);
    }
}