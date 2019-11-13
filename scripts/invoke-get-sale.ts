import Neon, { api, wallet, u, rpc, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'

const rpcUrl = "http://127.0.0.1:49332";

const param1 = Neon.create.contractParam("String", "getSale");
const param2 = sc.ContractParam.byteArray(
    u.reverseHex("e564116aab513b66b41c9175390a7e51f64dae9cc0f25349d696137462a2415c"), 
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
