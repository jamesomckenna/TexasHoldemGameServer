using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using TexasHoldem.Contracts.Chips.ContractDefinition;

namespace TexasHoldem.Contracts.Chips
{
    public partial class ChipsService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, ChipsDeployment chipsDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<ChipsDeployment>().SendRequestAndWaitForReceiptAsync(chipsDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, ChipsDeployment chipsDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<ChipsDeployment>().SendRequestAsync(chipsDeployment);
        }

        public static async Task<ChipsService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, ChipsDeployment chipsDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, chipsDeployment, cancellationTokenSource);
            return new ChipsService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public ChipsService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<string> ChipBalanceOfRequestAsync(ChipBalanceOfFunction chipBalanceOfFunction)
        {
             return ContractHandler.SendRequestAsync(chipBalanceOfFunction);
        }

        public Task<TransactionReceipt> ChipBalanceOfRequestAndWaitForReceiptAsync(ChipBalanceOfFunction chipBalanceOfFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipBalanceOfFunction, cancellationToken);
        }

        public Task<string> ChipBalanceOfRequestAsync(string account)
        {
            var chipBalanceOfFunction = new ChipBalanceOfFunction();
                chipBalanceOfFunction.Account = account;
            
             return ContractHandler.SendRequestAsync(chipBalanceOfFunction);
        }

        public Task<TransactionReceipt> ChipBalanceOfRequestAndWaitForReceiptAsync(string account, CancellationTokenSource cancellationToken = null)
        {
            var chipBalanceOfFunction = new ChipBalanceOfFunction();
                chipBalanceOfFunction.Account = account;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipBalanceOfFunction, cancellationToken);
        }

        public Task<string> ChipBurnRequestAsync(ChipBurnFunction chipBurnFunction)
        {
             return ContractHandler.SendRequestAsync(chipBurnFunction);
        }

        public Task<TransactionReceipt> ChipBurnRequestAndWaitForReceiptAsync(ChipBurnFunction chipBurnFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipBurnFunction, cancellationToken);
        }

        public Task<string> ChipBurnRequestAsync(BigInteger amount)
        {
            var chipBurnFunction = new ChipBurnFunction();
                chipBurnFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(chipBurnFunction);
        }

        public Task<TransactionReceipt> ChipBurnRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var chipBurnFunction = new ChipBurnFunction();
                chipBurnFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipBurnFunction, cancellationToken);
        }

        public Task<string> ChipMintRequestAsync(ChipMintFunction chipMintFunction)
        {
             return ContractHandler.SendRequestAsync(chipMintFunction);
        }

        public Task<TransactionReceipt> ChipMintRequestAndWaitForReceiptAsync(ChipMintFunction chipMintFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipMintFunction, cancellationToken);
        }

        public Task<string> ChipMintRequestAsync(string to, BigInteger amount)
        {
            var chipMintFunction = new ChipMintFunction();
                chipMintFunction.To = to;
                chipMintFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(chipMintFunction);
        }

        public Task<TransactionReceipt> ChipMintRequestAndWaitForReceiptAsync(string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var chipMintFunction = new ChipMintFunction();
                chipMintFunction.To = to;
                chipMintFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipMintFunction, cancellationToken);
        }

        public Task<string> ChipTransferRequestAsync(ChipTransferFunction chipTransferFunction)
        {
             return ContractHandler.SendRequestAsync(chipTransferFunction);
        }

        public Task<TransactionReceipt> ChipTransferRequestAndWaitForReceiptAsync(ChipTransferFunction chipTransferFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipTransferFunction, cancellationToken);
        }

        public Task<string> ChipTransferRequestAsync(string from, string to, BigInteger amount)
        {
            var chipTransferFunction = new ChipTransferFunction();
                chipTransferFunction.From = from;
                chipTransferFunction.To = to;
                chipTransferFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(chipTransferFunction);
        }

        public Task<TransactionReceipt> ChipTransferRequestAndWaitForReceiptAsync(string from, string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var chipTransferFunction = new ChipTransferFunction();
                chipTransferFunction.From = from;
                chipTransferFunction.To = to;
                chipTransferFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipTransferFunction, cancellationToken);
        }

        public Task<string> ChipTransferFromRequestAsync(ChipTransferFromFunction chipTransferFromFunction)
        {
             return ContractHandler.SendRequestAsync(chipTransferFromFunction);
        }

        public Task<TransactionReceipt> ChipTransferFromRequestAndWaitForReceiptAsync(ChipTransferFromFunction chipTransferFromFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipTransferFromFunction, cancellationToken);
        }

        public Task<string> ChipTransferFromRequestAsync(string sender, string recipient, BigInteger amount)
        {
            var chipTransferFromFunction = new ChipTransferFromFunction();
                chipTransferFromFunction.Sender = sender;
                chipTransferFromFunction.Recipient = recipient;
                chipTransferFromFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(chipTransferFromFunction);
        }

        public Task<TransactionReceipt> ChipTransferFromRequestAndWaitForReceiptAsync(string sender, string recipient, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var chipTransferFromFunction = new ChipTransferFromFunction();
                chipTransferFromFunction.Sender = sender;
                chipTransferFromFunction.Recipient = recipient;
                chipTransferFromFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(chipTransferFromFunction, cancellationToken);
        }

        public Task<string> AdminQueryAsync(AdminFunction adminFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(adminFunction, blockParameter);
        }

        
        public Task<string> AdminQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> AllowanceQueryAsync(AllowanceFunction allowanceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);
        }

        
        public Task<BigInteger> AllowanceQueryAsync(string owner, string spender, BlockParameter blockParameter = null)
        {
            var allowanceFunction = new AllowanceFunction();
                allowanceFunction.Owner = owner;
                allowanceFunction.Spender = spender;
            
            return ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);
        }

        public Task<string> ApproveRequestAsync(ApproveFunction approveFunction)
        {
             return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(ApproveFunction approveFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<string> ApproveRequestAsync(string spender, BigInteger amount)
        {
            var approveFunction = new ApproveFunction();
                approveFunction.Spender = spender;
                approveFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(string spender, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var approveFunction = new ApproveFunction();
                approveFunction.Spender = spender;
                approveFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<BigInteger> BalanceOfQueryAsync(BalanceOfFunction balanceOfFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        
        public Task<BigInteger> BalanceOfQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var balanceOfFunction = new BalanceOfFunction();
                balanceOfFunction.Account = account;
            
            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<byte> DecimalsQueryAsync(DecimalsFunction decimalsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DecimalsFunction, byte>(decimalsFunction, blockParameter);
        }

        
        public Task<byte> DecimalsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DecimalsFunction, byte>(null, blockParameter);
        }

        public Task<string> DecreaseAllowanceRequestAsync(DecreaseAllowanceFunction decreaseAllowanceFunction)
        {
             return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        }

        public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(DecreaseAllowanceFunction decreaseAllowanceFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);
        }

        public Task<string> DecreaseAllowanceRequestAsync(string spender, BigInteger subtractedValue)
        {
            var decreaseAllowanceFunction = new DecreaseAllowanceFunction();
                decreaseAllowanceFunction.Spender = spender;
                decreaseAllowanceFunction.SubtractedValue = subtractedValue;
            
             return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        }

        public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger subtractedValue, CancellationTokenSource cancellationToken = null)
        {
            var decreaseAllowanceFunction = new DecreaseAllowanceFunction();
                decreaseAllowanceFunction.Spender = spender;
                decreaseAllowanceFunction.SubtractedValue = subtractedValue;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);
        }

        public Task<string> IncreaseAllowanceRequestAsync(IncreaseAllowanceFunction increaseAllowanceFunction)
        {
             return ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        }

        public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(IncreaseAllowanceFunction increaseAllowanceFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);
        }

        public Task<string> IncreaseAllowanceRequestAsync(string spender, BigInteger addedValue)
        {
            var increaseAllowanceFunction = new IncreaseAllowanceFunction();
                increaseAllowanceFunction.Spender = spender;
                increaseAllowanceFunction.AddedValue = addedValue;
            
             return ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        }

        public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger addedValue, CancellationTokenSource cancellationToken = null)
        {
            var increaseAllowanceFunction = new IncreaseAllowanceFunction();
                increaseAllowanceFunction.Spender = spender;
                increaseAllowanceFunction.AddedValue = addedValue;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);
        }

        public Task<string> NameQueryAsync(NameFunction nameFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameter);
        }

        
        public Task<string> NameQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NameFunction, string>(null, blockParameter);
        }

        public Task<string> SymbolQueryAsync(SymbolFunction symbolFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SymbolFunction, string>(symbolFunction, blockParameter);
        }

        
        public Task<string> SymbolQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SymbolFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> TotalSupplyQueryAsync(TotalSupplyFunction totalSupplyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameter);
        }

        
        public Task<BigInteger> TotalSupplyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> TransferRequestAsync(TransferFunction transferFunction)
        {
             return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(TransferFunction transferFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<string> TransferRequestAsync(string recipient, BigInteger amount)
        {
            var transferFunction = new TransferFunction();
                transferFunction.Recipient = recipient;
                transferFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(string recipient, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var transferFunction = new TransferFunction();
                transferFunction.Recipient = recipient;
                transferFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<string> TransferFromRequestAsync(TransferFromFunction transferFromFunction)
        {
             return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(TransferFromFunction transferFromFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
        }

        public Task<string> TransferFromRequestAsync(string sender, string recipient, BigInteger amount)
        {
            var transferFromFunction = new TransferFromFunction();
                transferFromFunction.Sender = sender;
                transferFromFunction.Recipient = recipient;
                transferFromFunction.Amount = amount;
            
             return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(string sender, string recipient, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var transferFromFunction = new TransferFromFunction();
                transferFromFunction.Sender = sender;
                transferFromFunction.Recipient = recipient;
                transferFromFunction.Amount = amount;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
        }
    }
}
