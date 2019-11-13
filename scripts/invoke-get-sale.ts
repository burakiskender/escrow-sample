import Neon, { api, wallet, u, rpc, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'

const rpcUrl = "http://127.0.0.1:49332";

const param1 = Neon.create.contractParam("String", "getSale");
const param2 = sc.ContractParam.byteArray(
    "5d6b23c95739c599f0459c823774b22508c25c1e31a2913694ab67e8c0042e99", 
    null);

async function mainAsync() {
    const contractHash = await getContractHash();

    var client = new rpc.RPCClient(rpcUrl);
    var result = await client.invoke(
        contractHash, 
        param1,
        sc.ContractParam.array(param2)
      );

    console.log(JSON.stringify(result, null, 4));
}

mainAsync().catch(err => { console.log(err); });
