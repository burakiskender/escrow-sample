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
        public delegate void ErrorDelegate(string message, object[] data = null);
        public static event ErrorDelegate Error;

        public delegate void NewSaleDelegate(byte[] txid, byte[] seller, string description, ulong price);
        public static event NewSaleDelegate NewSale;

        private static readonly byte[] NeoAssetId = Helper.HexToBytes("9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc5"); //NEO Asset ID, littleEndian
        private const byte NeoPrecision = 8;
        private const ulong NeoPrecisionDivisior = 100000000;


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


        private static object CreateSale(ulong price, string description)
        {
            if (price <= 0)
            {
                Error("must set a price > 0");
                return false;
            }

            var tx = ExecutionEngine.ScriptContainer as Transaction;

            var inputs = tx.GetReferences();
            byte[] sender = null;
            foreach (var input in inputs)
            {
                if (input.AssetId.AsBigInteger() == NeoAssetId.AsBigInteger())
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
                    output.AssetId.AsBigInteger() == NeoAssetId.AsBigInteger())
                {
                    value += (ulong)output.Value;
                }
            }

            if (value % NeoPrecisionDivisior != 0)
            {
                Error("invalid NEO amount");
                return false;
            }

            value = value / NeoPrecisionDivisior;

            if (value < price * 2)
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

            StorageMap saleInfo = Storage.CurrentContext.CreateMap(nameof(Escrow));
            saleInfo.Put(info.Id, Helper.Serialize(info));

            NewSale(info.Id, info.Seller, info.Description, info.Price);
            return true;
        }

        private static object GetSale(byte[] txid)
        {
            if (txid.Length != 32)
                throw new InvalidOperationException("The parameter txid MUST be 32-byte transaction hash.");

            StorageMap saleInfo = Storage.CurrentContext.CreateMap(nameof(Escrow));
            var result = saleInfo.Get(txid);
            if (result.Length == 0) return null;
            return Helper.Deserialize(result) as SaleInfo;
        }

        private static object BuyerDeposit(object v)
        {
            throw new NotImplementedException();
        }

        private static object ConfirmShipment(object v)
        {
            throw new NotImplementedException();
        }

        private static object ConfirmReceived(object v)
        {
            throw new NotImplementedException();
        }

        private static object DeleteSale(object v)
        {
            throw new NotImplementedException();
        }
    }
}