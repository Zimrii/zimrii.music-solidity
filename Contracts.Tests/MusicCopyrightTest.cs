using System.Threading.Tasks;
using FluentAssertions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Xunit;

namespace Zimrii.Solidity.Tests
{
    public class MusicCopyrightTest : NethereumTestsBase
    {

        public MusicCopyrightTest() : base(new[] { "Owned", "MusicCopyright" })
        {
        }

        [Fact]
        public async Task MusicCopyrightSolTest()
        {
            await Setup(false);

            var contractAddress = Receipts["MusicCopyright"].ContractAddress;
            var contract = Web3.Eth.GetContract(Abi["MusicCopyright"], contractAddress);

            var addCopyrightEvent = contract.GetEvent("SetCopyright");
            var filterAll = await addCopyrightEvent.CreateFilterAsync();
            var filterMusicId = await addCopyrightEvent.CreateFilterAsync("51BF375C77BC4C089DCAD2AC4935E600");

            var setCopyright = contract.GetFunction("setCopyright");
            var getCopyrightId = contract.GetFunction("getCopyrightId");
            var getCopyrightHash = contract.GetFunction("getCopyrightHash");

            var unlockResult =
                await Web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            var gas = await setCopyright.EstimateGasAsync(AccountAddress, null, null, "61BF375C77BC4C089DCAD2AC4935E600", "3345D89C498A4EB79DB670F46F25EF00", "6B2M2Y8AsgTpgAmY7PhCfg==");
            var receipt1 = await setCopyright.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, 
                "51BF375C77BC4C089DCAD2AC4935E600", "3345D89C498A4EB79DB670F46F25EF00", "1B2M2Y8AsgTpgAmY7PhCfg==");

            var log = await addCopyrightEvent.GetFilterChanges<AddCopyrightEvent>(filterAll);
            log.Count.Should().Be(1);

            var logMusicId = await addCopyrightEvent.GetFilterChanges<AddCopyrightEvent>(filterMusicId);
            logMusicId.Count.Should().Be(1);

            var res1 = await getCopyrightId.CallAsync<string>("51BF375C77BC4C089DCAD2AC4935E600");
            res1.Should().Be("3345D89C498A4EB79DB670F46F25EF00");

            var res2 = await getCopyrightHash.CallAsync<string>("51BF375C77BC4C089DCAD2AC4935E600");
            res2.Should().Be("1B2M2Y8AsgTpgAmY7PhCfg==");

        }
    }



    public class AddCopyrightEvent
    {
        [Parameter("bytes32", "musicId", 1, true)]
        public string MusicId { get; set; }

        [Parameter("bytes32", "copyrightId", 2, false)]
        public string CopyrightId { get; set; }
    }
}
