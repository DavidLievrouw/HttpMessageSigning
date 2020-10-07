using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal interface IClientStoreMigrator {
        Task<int> Migrate();
    }
}