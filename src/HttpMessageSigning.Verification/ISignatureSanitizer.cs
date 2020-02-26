using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureSanitizer {
        Task<Signature> Sanitize(Signature signature, Client client);
    }
}