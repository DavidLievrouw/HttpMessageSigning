using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal interface IClientStoreMigrationStep {
        Task Run();
        int Version { get; }
    }
}