using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal interface IBaseliner {
        Task SetBaseline(IMigrationStep step);
        Task<int?> GetBaseline();
    }
}