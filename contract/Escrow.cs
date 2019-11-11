using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

using Helper = Neo.SmartContract.Framework.Helper;

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
        // public delegate void ErrorDelegate(string message);
        // public static event ErrorDelegate Error;

        public delegate void NewSaleDelegate(byte[] txid, byte[] seller, string description, ulong price);
        public static event NewSaleDelegate NewSale;

        public static object Main(string method, object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return false;
            }

            switch (method)
            {
                case "createSale":
                    return CreateSale((ulong)args[0], (string)args[1]);
                case "buyerDeposit":
                    return BuyerDeposit(args[0]);
                case "confirmShipment":
                    return ConfirmShipment(args[0]);
                case "confirmReceived":
                    return ConfirmReceived(args[0]);
                case "getSale":
                    return GetSale(args[0]);
                case "deleteSale":
                    return DeleteSale(args[0]);
            }

            return false;
        }

        private static object DeleteSale(object v)
        {
            throw new NotImplementedException();
        }

        private static object GetSale(object v)
        {
            throw new NotImplementedException();
        }

        private static object ConfirmReceived(object v)
        {
            throw new NotImplementedException();
        }

        private static object ConfirmShipment(object v)
        {
            throw new NotImplementedException();
        }

        private static object BuyerDeposit(object v)
        {
            throw new NotImplementedException();
        }

        private static readonly byte[] AssetId = Helper.HexToBytes("9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc5"); //NEO Asset ID, littleEndian


        private static object CreateSale(ulong price, string description)
        {
            if (price <= 0)
                throw new Exception("must set a price > 0");

            var tx = ExecutionEngine.ScriptContainer as Transaction;

            var inputs = tx.GetReferences();
            byte[] sender = null;
            foreach (var input in inputs)
            {
                if (input.AssetId.AsBigInteger() == AssetId.AsBigInteger())
                    sender = sender ?? input.ScriptHash;

                //Escrow address as inputs is not allowed
                if (input.ScriptHash.AsBigInteger() == ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                    return false;
            }

            var outputs = tx.GetOutputs();
            ulong value = 0;
            foreach (var output in outputs)
            {
                if (output.ScriptHash == ExecutionEngine.ExecutingScriptHash &&
                    output.AssetId.AsBigInteger() == AssetId.AsBigInteger())
                {
                    value += (ulong)output.Value;
                }
            }

            if (value < price * 2)
                throw new Exception("seller deposit must be 2x price");

            var info = new SaleInfo()
            {
                Id = tx.Hash,
                Seller = sender,
                Description = description,
                Price = price,
                State = SaleState.New
            };


            StorageMap txInfo = Storage.CurrentContext.CreateMap(nameof(txInfo));
            txInfo.Put(info.Id, Helper.Serialize(info));

            NewSale(info.Id, info.Seller, info.Description, info.Price);
            return info.Id;
        }
    }
}