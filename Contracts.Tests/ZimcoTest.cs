using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
//using Nethereum.RPC.DebugGeth.DTOs;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Serilog;
using Xunit;

namespace Zimrii.Solidity.Tests
{
    public class ZimcoTest : NethereumTestsBase
    {

        public ZimcoTest() : base(new[] { "Owned", "TokenData", "ZimcoToken" }) 
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
        public async Task ZimcoSetupZimcoTest()
        {
            await Setup(false);

            var artistAddress = "0x0171a28b51c70037a37e940eb22101cd5f687d00";
            var toAddress = "0x13f022d72158410433cbd66f5dd8bf6d2d129924";

            ReadContractsDetails();

            var contractDataAddress = Receipts["TokenData"].ContractAddress;
            var contractZimcoAddress = Receipts["ZimcoToken"].ContractAddress;

            var contractData = Web3.Eth.GetContract(Abi["TokenData"], contractDataAddress);
            var contractZimco = Web3.Eth.GetContract(Abi["ZimcoToken"], contractZimcoAddress);

            var changeOwners = contractData.GetFunction("changeOwners");
            var setTotalSupply = contractZimco.GetFunction("setTotalSupply");
            var transfer = contractZimco.GetFunction("transfer");
            var transferFrom = contractZimco.GetFunction("transferFrom");
            var setAllowance = contractData.GetFunction("setAllowance");
            var getAllowance = contractData.GetFunction("getAllowance");
            var balanceOf = contractZimco.GetFunction("balanceOf");
            
            var unlockResult =
                await Web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            // give Zimco access to database
            var gas = await changeOwners.EstimateGasAsync(AccountAddress, null, null, contractZimcoAddress, true);
            var receipt1 = await changeOwners.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, contractZimcoAddress, true);

            unlockResult =
                await Web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            // set initial supply
            gas = await setTotalSupply.EstimateGasAsync(AccountAddress, null, null, 1000000000);
            var receipt2 = await setTotalSupply.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, 1000000000);

            // transfer all tokens
            //gas = await transfer.EstimateGasAsync(AccountAddress, null, null, artistAddress, 800000000);
            var receipt3 = await transfer.SendTransactionAndWaitForReceiptAsync(AccountAddress, new HexBigInteger(200000), null, null, artistAddress, 800000000);

            // check the balance
            var res = await balanceOf.CallAsync<int>(artistAddress);
            res.Should().Be(800000000);

            // set allowance to zimrii
            gas = await setAllowance.EstimateGasAsync(AccountAddress, null, null, artistAddress, AccountAddress, 750000);
            var receipt4 = await setAllowance.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, artistAddress, AccountAddress, 750000);

            // check the balance
            var res2 = await balanceOf.CallAsync<int>(artistAddress);
            res2 = await balanceOf.CallAsync<int>(AccountAddress);
            res2 = await getAllowance.CallAsync<int>(artistAddress, AccountAddress);

            unlockResult = await Web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            // transfer from
            gas = await transferFrom.EstimateGasAsync(AccountAddress, null, null, artistAddress, artistAddress, 75000);
            var receipt5 = await transferFrom.SendTransactionAndWaitForReceiptAsync(AccountAddress, gas, null, null, artistAddress, toAddress, 75000);

            // check the balance after transfer
            var res3 = await balanceOf.CallAsync<int>(artistAddress);
            res3 = await balanceOf.CallAsync<int>(AccountAddress);
            res3 = await balanceOf.CallAsync<int>("0x13f022d72158410433cbd66f5dd8bf6d2d129924");
        }

        protected override async Task<Dictionary<string, TransactionReceipt>> DeployContract(Web3 web3, IEnumerable<string> contracts, bool isMining, Action<Dictionary<string, TransactionReceipt>> saveContract = null)
        {
            var receipts = new Dictionary<string, TransactionReceipt>();

            var unlockResult =
                await web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, PassPhrase, 120);
            unlockResult.Should().BeTrue();

            TransactionReceipt receipt;
            foreach (var contract in contracts)
            {
                switch (contract)
                {
                    case "TokenBase":
                    case "ZimcoToken":
                        var contractDataAddress = receipts["TokenData"].ContractAddress;
                        receipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(Abi[contract], Code[contract], 
                            AccountAddress, new HexBigInteger(2000000), null, contractDataAddress, "zimco", 2, "ZMC");
                        break;

                    default:
                        receipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(Abi[contract], Code[contract], AccountAddress, new HexBigInteger(2000000), null);
                        break;
                }

                receipts.Add(contract, receipt);
            }

            saveContract?.Invoke(receipts);
            return receipts;
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

    public class TransferEvent
    {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "value", 3, false)]
        public int Value { get; set; }
    }
}
