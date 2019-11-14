import Neon, { api, wallet, u, rpc, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'
import { getTxInfo } from "./tx-info";

const rpcUrl = "http://127.0.0.1:49332";

const param1 = Neon.create.contractParam("String", "getSale");

async function mainAsync() {
    const contractHash = await getContractHash();
    let txInfo = await getTxInfo();
    
    const param2 = sc.ContractParam.byteArray(
        u.reverseHex(txInfo.createSaleTx), 
        null);
    
    var client = new rpc.RPCClient(rpcUrl);
    var result = await client.invoke(
        contractHash, 
        param1,
        sc.ContractParam.array(param2)
      );

    console.log(JSON.stringify(result, null, 4));
}

mainAsync().catch(err => { console.log(err); });
