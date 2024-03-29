import Neon, { api, wallet, u, sc } from "@cityofzion/neon-js";
import { getContractHash } from './get-contract-hash'
import { getPrivateKey } from './get-express-info'
import { rpc } from "@cityofzion/neon-core";
import { getTxInfo } from "./tx-info";

const operation = "buyerDeposit"
const rpcUrl = "http://127.0.0.1:49332";

const delay = ms => new Promise(res => setTimeout(res, ms));

async function mainAsync() {

    let txInfo = await getTxInfo();

    const buyerPrivateKey = await getPrivateKey("buyer"); 
    const buyerAccount = new wallet.Account(buyerPrivateKey);

    const contractScriptHash = await getContractHash();

    const script = Neon.create.script({
        scriptHash: contractScriptHash,
        operation: operation,
        args: [u.reverseHex(txInfo.createSaleTx),] 
    });
   
    const config = {
        api: new api.neoCli.instance(rpcUrl),
        account: buyerAccount,
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
