import Neon, { api, wallet, u, rpc, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'

const rpcUrl = "http://127.0.0.1:49332";

const param1 = Neon.create.contractParam("String", "getSale");
const param2 = sc.ContractParam.byteArray(
    u.reverseHex('da6b28532f8d4811aa675d26dec041f6c21166c63c9c49964009236941f43721'), 
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
