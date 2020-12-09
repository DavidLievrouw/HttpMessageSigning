using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    internal interface IClientStoreMigrator {
        Task<int> Migrate();
    }
}