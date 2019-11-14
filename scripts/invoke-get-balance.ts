import Neon, { api, wallet, u, rpc, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'
import { getTxInfo } from "./tx-info";
import { getPrivateKey } from "./get-express-info";

const rpcUrl = "http://127.0.0.1:49332";

const param1 = Neon.create.contractParam("String", "getBalance");

async function mainAsync() {

    const buyerPrivateKey = await getPrivateKey("buyer"); 
    const buyerAccount = new wallet.Account(buyerPrivateKey);
    const buyerParam = sc.ContractParam.byteArray(
        u.reverseHex(buyerAccount.scriptHash), 
        null);
    const sellerPrivateKey = await getPrivateKey("seller"); 
    const sellerAccount = new wallet.Account(sellerPrivateKey);
    const sellerParam = sc.ContractParam.byteArray(
        u.reverseHex(sellerAccount.scriptHash), 
        null);

    const contractHash = await getContractHash();
    
    var client = new rpc.RPCClient(rpcUrl);

    var buyerResult = await client.invoke(
        contractHash, 
        param1,
        sc.ContractParam.array(buyerParam));

    console.log(JSON.stringify(buyerResult, null, 4));

    var sellerResult = await client.invoke(
        contractHash, 
        param1,
        sc.ContractParam.array(sellerParam));

    console.log(JSON.stringify(sellerResult, null, 4));
}

mainAsync().catch(err => { console.log(err); });
