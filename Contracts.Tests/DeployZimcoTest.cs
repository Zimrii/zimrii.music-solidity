using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Serilog;
using Xunit;

namespace Zimrii.Solidity.Tests
{
    public class DeployZimcoTest : NethereumTestsBase
    {

        public DeployZimcoTest() : base(new[] { "Owned", "TokenData", "ZimcoToken" }) 
        {
            RootPath = @"..\..\..\..\..\Zimrii.Solidity\contracts\zimco\metadata";
            string curDir = AssemblyDirectory;
            var logPath = Path.Combine(curDir + RootPath, @"..\Zimco-{Date}.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(logPath)
                .CreateLogger();

        }

        protected override async Task<Dictionary<string, TransactionReceipt>> DeployContract(Web3 web3, IEnumerable<string> contracts, bool isMining,
            Action<Dictionary<string, TransactionReceipt>> saveContract = null)
        {
            var receipts = new Dictionary<string, TransactionReceipt>();

            var unlockResult =
                await web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            foreach (var contract in contracts)
            {
                string deploy;
                switch (contract)
                {
                    case "TokenBase":
                    case "ZimcoToken":
                        var contractDataAddress = receipts["TokenData"].ContractAddress;
                        deploy = await web3.Eth.DeployContract.SendRequestAsync(Abi[contract], Code[contract], AccountAddress, new HexBigInteger(2000000),
                            contractDataAddress,
                            "zimco",
                            2,
                            "ZMC");
                        break;

                    default:
                        deploy = await web3.Eth.DeployContract.SendRequestAsync(Abi[contract], Code[contract], AccountAddress, new HexBigInteger(2000000));
                        break;
                }

                var receipt = await MineAndGetReceiptAsync(web3, deploy, isMining);

                receipts.Add(contract, receipt);
            }

            saveContract?.Invoke(receipts);
            return receipts;
        }

        //[Fact]
        //public async Task DeployZimcoToRinkeby()
        //{

        //    AccountAddress = "0x564f83ae16af0741ce756adf35dcf9b17874b83f";
        //    PassPhrase = "";

        //    await Setup(false, SaveZimcoDetails);
        //}

        [Fact]
        public async Task DeployZimcoToTestChain()
        {
            await Setup(true, SaveZimcoDetails);
        }

        void SaveZimcoDetails(Dictionary<string, TransactionReceipt> receipts)
        {
            foreach (var receipt in receipts)
            {
                Log.Information(receipt.Key);
                Log.Information("{@zimco}", new
                {
                    AccountAddress = AccountAddress,
                    TransactionHash = receipt.Value.TransactionHash,
                    ContractAddress = receipt.Value.ContractAddress,
                    BlockHash = receipt.Value.BlockHash,
                    CumulativeGasUsed = receipt.Value.CumulativeGasUsed.Value.ToString(),
                    GasUsed = receipt.Value.GasUsed.Value.ToString(),
                    TransactionIndex = receipt.Value.TransactionIndex.Value.ToString()
                });
            }
        }
    }
}
