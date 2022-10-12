using System;

//
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


//Import Nethereum Library
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System.Numerics;
using Nethereum.Signer;

//Import Solidity Smart Contracts > C# Classes - Used to Call Smart Contracts on Blockchain
using TexasHoldem.Contracts.Chips;
using TexasHoldem.Contracts.Chips.ContractDefinition;

public class Blockchain
{
    // Blockchain Data Init
    private static string url = "http://127.0.0.1:7545";
    private static string privateKey = "8bad81821900af90b36ba45829d00204b754fafc0384db0e7f2c96ec88d5a948";
    private static Account account;
    private static Web3 web3;
    private static string contractAddress = "0x530737939625a250Bf69Cc9DA5A6cdf9cBF283d9";


    //Create web3 client and connection.
    public static void BC_Connect()
    {
        account = new Account(privateKey, 1337); //Generate Account based off private key.
        web3 = new Web3(account, url); //Create web3 client based on URL (RCP) and account.        
    }

    public static void BC_Disconnect()
    {
        web3 = null;
    }

    //Retrieve the balance of a given account.
    public async static void BC_BalanceOf()
    {
        Console.WriteLine("=== BC_BalanceOf Start ===");
        //Init contract address of Chips contract. 
        // ^ Maybe change this to an intput of the function, can then be declared globally and used if needed. ^

        //Generate function message based on generated C# class functions compiled from solidity.
        var balanceOfFunctionMessage = new ChipBalanceOfFunction()
        {
            Account = account.Address,
        };

        //Query the blockchain.
        var balanceHandler = web3.Eth.GetContractQueryHandler<ChipBalanceOfFunction>(); //Create query handler of balance call.
        var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage); //Await response from query to return balance value of account.

        //Output Balance
        Console.WriteLine($"Balance: {balance}"); //Change this to return?
        Console.WriteLine("=== BC_BalanceOf End ===");
    }

    public async static void BC_Transfer()
    {
        Console.WriteLine("=== BC_Transfer Start ===");
        try
        {
            var receiverAddress = "0x1c81629ff89e39F4ee8D3b1e2023574b2b465a44";
            var transferHandler = web3.Eth.GetContractTransactionHandler<ChipTransferFunction>();
            var transfer = new ChipTransferFunction()
            {
                From = account.Address,
                To = receiverAddress,
                Amount = 1
            };

            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine("=== BC_Transfer End ===");

    }

    private async static void BC_Mint()
    {
        //Transfer Function - TO DO

    }
    private async static void BC_Burn()
    {
        //Transfer Function - TO DO

    }

    public async static void ReadBlockChain() // BLockchain Testing Function
    {
        string keyAddress = "0x917bCF2D91dACacC4B6feD11E101fb57bFB573A1";
        var web3 = new Web3("http://127.0.0.1:7545");
        var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        Console.WriteLine($"Latest Block Number is: {latestBlockNumber}");
    }
}
