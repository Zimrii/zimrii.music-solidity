using System.IO;
using System.Threading.Tasks;
using Nethereum.Web3;
using Serilog;
using Xunit;

namespace Zimrii.Solidity.Tests
{
    public class CreateAccountTest : NethereumTestsBase
    {

        public CreateAccountTest() : base(new[] { "Owned", "TokenData", "ZimcoToken" }) 
        {
            RootPath = @"..\..\..\..\..\Zimrii.Solidity\contracts\zimco\metadata";
            string curDir = AssemblyDirectory;
            var logPath = Path.Combine(curDir + RootPath, @"..\Zimco-{Date}.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(logPath)
                .CreateLogger();

        }


        [Fact]
        public async Task CreateAccountForUserTest()
        {
            var newAccount = await Web3.Personal.NewAccount.SendRequestAsync("password");
        }

    }
}
