import Neon, { api, wallet, u, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'
import { getPrivateKey } from './get-express-info'
import { rpc } from "@cityofzion/neon-core";

const operation = "createSale"
const rpcUrl = "http://127.0.0.1:49332";

const delay = ms => new Promise(res => setTimeout(res, ms));

async function mainAsync() {

    const testUserPrivateKey = await getPrivateKey("seller"); 
    const testUserAccount = new wallet.Account(testUserPrivateKey);

    const contractScriptHash = await getContractHash();

    const script = Neon.create.script({
        scriptHash: contractScriptHash,
        operation: operation,
        args: [100, u.str2hexstring("My Best Widget")] 
    });
   
    const config = {
        api: new api.neoCli.instance(rpcUrl),
        account: testUserAccount,
        script: script,
        intents: api.makeIntent({ NEO: 200 }, contractScriptHash)
     };

     var result = await Neon.doInvoke(config);
     console.log("\n\n--- Response ---");
     console.log(result.response);

     await delay(2000);

     var applog = await rpc.queryRPC(rpcUrl, {
        method: "getapplicationlog",
        params: [result.response.txid]
     });
     console.log("\n\n--- AppLog ---");
     console.log(JSON.stringify(applog, null, 4));
 }

 mainAsync().catch(err => { console.log(err); });
