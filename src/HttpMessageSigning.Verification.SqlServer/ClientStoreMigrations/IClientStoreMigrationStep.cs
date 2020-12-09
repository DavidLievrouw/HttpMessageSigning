using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    internal interface IClientStoreMigrationStep {
        Task Run();
        int Version { get; }
    }
}