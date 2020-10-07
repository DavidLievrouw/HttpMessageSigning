using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal interface IMigrator {
        Task<int> Migrate();
    }
}