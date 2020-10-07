using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal interface IMigrationStep {
        Task Run();
        int Version { get; }
    }
}