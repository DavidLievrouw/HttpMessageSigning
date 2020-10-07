using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal interface IClientStoreBaseliner {
        Task SetBaseline(IClientStoreMigrationStep step);
        Task<int?> GetBaseline();
    }
}