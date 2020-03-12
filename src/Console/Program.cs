using System.Threading.Tasks;

namespace Console {
    public class Program {
        private static async Task Main(string[] args) {
            await SampleHMAC.Run(args);
            await SampleRSA.Run(args);
            //await SampleHMACClient.Run(args);
        }
    }
}