using System.Threading.Tasks;
using FluentAssertions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
//using Nethereum.RPC.DebugGeth.DTOs;
using Xunit;

namespace Zimrii.Solidity.Tests
{
    public class ArtistContractTest : NethereumTestsBase
    {

        public ArtistContractTest() : base(new[] { "Owned", "ArtistContract" })
        {
        }

        [Fact]
        public async Task ArtistContractSolidityMethodsTest()
        {
            await Setup(false);

            var contractAddress = Receipts["ArtistContract"].ContractAddress;
            var contract = Web3.Eth.GetContract(Abi["ArtistContract"], contractAddress);
            var setContract = contract.GetFunction("setContract");
            var getContractHash = contract.GetFunction("getContractHash");

            var addContractEvent = contract.GetEvent("SetContract");
            var filterAll = await addContractEvent.CreateFilterAsync();
            var filterContractId = await addContractEvent.CreateFilterAsync("61BF375C77BC4C089DCAD2AC4935E600");


            var unlockResult =
                await Web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            var gas = await setContract.EstimateGasAsync(AccountAddress, null, null, "61BF375C77BC4C089DCAD2AC4935E600", "6B2M2Y8AsgTpgAmY7PhCfg==");
            var receipt1 = await setContract.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, "61BF375C77BC4C089DCAD2AC4935E600", "6B2M2Y8AsgTpgAmY7PhCfg==");

            var res = await getContractHash.CallAsync<string>("61BF375C77BC4C089DCAD2AC4935E600");
            res.Should().Be("6B2M2Y8AsgTpgAmY7PhCfg==");

            var log = await addContractEvent.GetFilterChanges<AddContractEvent>(filterAll);
            log.Count.Should().Be(1);

            var logMusicId = await addContractEvent.GetFilterChanges<AddContractEvent>(filterContractId);
            logMusicId.Count.Should().Be(1);
        }
    }

    public class AddContractEvent
    {
        [Parameter("bytes32", "contractId", 1, true)]
        public string ContractId { get; set; }

        [Parameter("bytes32", "contractHash", 2, false)]
        public string ContractHash { get; set; }
    }
}
