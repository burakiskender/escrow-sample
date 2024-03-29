using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

using Helper = Neo.SmartContract.Framework.Helper;

// Note, this contract is a port of safe-remote-purchase.py 
//       originally written by Joe Stewart (aka hal0x2328)
//       https://github.com/Splyse/MCT/blob/master/safe-remote-purchase.py
// Ported from python to C# by Harry Pierson (aka DevHawk)

namespace NGDSeattle
{
    public enum SaleState
    {
        New,
        AwaitingShipment,
        ShipmentConfirmed,
    }

    public class SaleInfo
    {
        public byte[] Id;
        public byte[] Seller;
        public byte[] Buyer;
        public string Description;
        public ulong Price;
        public SaleState State;
    }

    public class Escrow : SmartContract
    {
        public delegate void ErrorDelegate(string message, object[] data = null);
        public static event ErrorDelegate Error;

        public delegate void NewSaleDelegate(byte[] txid, byte[] seller, string description, ulong price);
        public static event NewSaleDelegate NewSale;

        public delegate void SaleStateUpdatedDelegate(byte[] txid, byte[] buyer, SaleState state);
        public static event SaleStateUpdatedDelegate SaleStateUpdated;

        public delegate void SaleCompletedDelegate(byte[] txid);
        public static event SaleCompletedDelegate SaleCompleted;

        public delegate void FundsTransferredDelegate(byte[] accountId, BigInteger value);
        public static event FundsTransferredDelegate FundsTransferred;



        private static readonly byte[] NeoAssetId = Helper.HexToBytes("9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc5"); //NEO Asset ID, littleEndian
        private const byte NeoPrecision = 8;
        private const ulong NeoPrecisionDivisior = 100000000;

        const string SalesMapName = nameof(Escrow);
        const string FundsMapName = nameof(Escrow) + "Funds";

        public static object Main(string method, object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return false;
            }

            if (method == "createSale")
            {
                return CreateSale((ulong)args[0], (string)args[1]);
            }

            if (method == "getSale")
            {
                return GetSale((byte[])args[0]);
            }

            if (method == "buyerDeposit")
            {
                return BuyerDeposit((byte[])args[0]);
            }

            if (method == "confirmShipment")
            {
                return ConfirmShipment((byte[])args[0]);
            }

            if (method == "confirmReceived")
            {
                return ConfirmReceived((byte[])args[0]);
            }

            if (method == "getBalance") 
            {
                return GetBalance((byte[])args[0]); 
            }

            if (method == "migrateContract")
            {
                if (args.Length != 10)
                {
                    return false;
                }

                Contract.Migrate(
                    script: (byte[])args[1],
                    parameter_list: (byte[])args[2],
                    return_type: (byte)args[3],
                    contract_property_state: (ContractPropertyState)args[4],
                    name: (string)args[5],
                    version: (string)args[6],
                    author: (string)args[7],
                    email: (string)args[8],
                    description: (string)args[9]);
                return true;
            }

            return false;
        }

        private static byte[] GetSender(Transaction tx)
        {
            var inputs = tx.GetReferences();
            byte[] sender = null;
            foreach (var input in inputs)
            {
                if (input.AssetId.AsBigInteger() == NeoAssetId.AsBigInteger())
                    sender = sender ?? input.ScriptHash;

                //Escrow address as inputs is not allowed
                if (input.ScriptHash.AsBigInteger() == ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                    return null;
            }

            return sender;
        }

        private static ulong GetOutputValue(Transaction tx)
        {
            var outputs = tx.GetOutputs();
            ulong value = 0;
            foreach (var output in outputs)
            {
                if (output.ScriptHash == ExecutionEngine.ExecutingScriptHash &&
                    output.AssetId.AsBigInteger() == NeoAssetId.AsBigInteger())
                {
                    value += (ulong)output.Value;
                }
            }
            return value;
        }

        public static object CreateSale(ulong price, string description)
        {
            if (price <= 0)
            {
                Error("must set a price > 0");
                return false;
            }

            var tx = ExecutionEngine.ScriptContainer as Transaction;
            var sender = GetSender(tx);
            if (sender == null)
            {
                Error("invalid sender");
                return false;
            }

            var value = GetOutputValue(tx);

            if (value != price * NeoPrecisionDivisior * 2)
            {
                Error("seller deposit must be 2x price", new object[] { value, price });
                return false;
            }

            var info = new SaleInfo()
            {
                Id = tx.Hash,
                Seller = sender,
                Description = description,
                Price = price,
                State = SaleState.New
            };

            StorageMap saleInfo = Storage.CurrentContext.CreateMap(SalesMapName);
            saleInfo.Put(info.Id, Helper.Serialize(info));

            NewSale(info.Id, info.Seller, info.Description, info.Price);
            return true;
        }

        public static SaleInfo GetSale(byte[] txid)
        {
            if (txid.Length != 32)
                throw new InvalidOperationException("The parameter txid MUST be 32-byte transaction hash.");

            StorageMap saleInfo = Storage.CurrentContext.CreateMap(SalesMapName);
            var result = saleInfo.Get(txid);
            if (result.Length == 0) return null;
            return Helper.Deserialize(result) as SaleInfo;
        }

        public static object BuyerDeposit(byte[] txid)
        {
            var info = GetSale(txid);
            if (info.State != SaleState.New)
            {
                Error("sale state incorrect", new object[] { info.State });
                return false;
            }

            var tx = ExecutionEngine.ScriptContainer as Transaction;
            var sender = GetSender(tx);
            if (sender == null)
            {
                Error("invalid sender");
                return false;
            }
            var value = GetOutputValue(tx);

            if (value != info.Price * NeoPrecisionDivisior * 2)
            {
                Error("buyer deposit must be 2x price", new object[] { value, info });
                return false;
            }

            info.Buyer = sender;
            info.State = SaleState.AwaitingShipment;

            StorageMap saleInfoMap = Storage.CurrentContext.CreateMap(SalesMapName);
            saleInfoMap.Put(info.Id, Helper.Serialize(info));

            SaleStateUpdated(info.Id, info.Buyer, info.State);
            return true;
        }

        public static bool ConfirmShipment(byte[] txid)
        {
            var info = GetSale(txid);
            if (info.State != SaleState.AwaitingShipment)
            {
                Error("sale state incorrect", new object[] { info.State });
                return false;
            }

            if (info.Buyer == null)
            {
                Error("buyer not specified");
                return false;
            }

            if (!Runtime.CheckWitness(info.Seller))
            {
                Error("must be seller to confirm shipment", new object[] { info.Seller });
                return false;
            }

            info.State = SaleState.ShipmentConfirmed;

            StorageMap saleInfoMap = Storage.CurrentContext.CreateMap(SalesMapName);
            saleInfoMap.Put(info.Id, Helper.Serialize(info));

            SaleStateUpdated(info.Id, null, info.State);
            return true;
        }

        public static bool ConfirmReceived(byte[] txid)
        {
            var info = GetSale(txid);
            if (info.State != SaleState.ShipmentConfirmed)
            {
                Error("sale state incorrect", new object[] { info.State });
                return false;
            }

            if (!Runtime.CheckWitness(info.Buyer))
            {
                Error("must be buyer to confirm shipment", new object[] { info.Buyer });
                return false;
            }

            var price = info.Price * NeoPrecisionDivisior;

            StorageMap fundsMap = Storage.CurrentContext.CreateMap(FundsMapName);
            var sellerAmount = fundsMap.Get(info.Seller).AsBigInteger();
            fundsMap.Put(info.Seller, sellerAmount + price * 3);
            var buyerAmount = fundsMap.Get(info.Buyer).AsBigInteger();
            fundsMap.Put(info.Buyer, buyerAmount + price);

            StorageMap saleInfoMap = Storage.CurrentContext.CreateMap(SalesMapName);
            saleInfoMap.Delete(info.Id);

            SaleCompleted(info.Id);
            FundsTransferred(info.Seller, info.Price * 3);
            FundsTransferred(info.Buyer, info.Price);

            return true;
        }

        public static BigInteger GetBalance(byte[] account)
        {
            if (account.Length != 20)
                throw new InvalidOperationException("The parameter account SHOULD be 20-byte addresses.");
            StorageMap fundsMap = Storage.CurrentContext.CreateMap(FundsMapName);
            return fundsMap.Get(account).AsBigInteger();
        }
    }
}