import Neon, { api, wallet, u } from "@cityofzion/neon-js";
import * as path from "path";
import * as fs from "fs";
import { promisify } from "util";

const readFile = promisify(fs.readFile);
const rootPath = path.resolve(__dirname, '..');
const abiPath = path.join(rootPath, 'contract\\bin\\Debug\\netstandard2.0\\publish\\Escrow.abi.json');

const operation = "createSale"
const rpcUrl = "http://127.0.0.1:49332";
const testUserPrivateKey = "79ccf764755760a2b17c17cd7303ab339bb2fcd38af57219e962d55c10a8fb9f";
const testUserAccount = new wallet.Account(testUserPrivateKey);

async function mainAsync() {

    const contractScriptHash = JSON.parse(await readFile(abiPath, "utf8")).hash.substring(2);

    const script = Neon.create.script({
        scriptHash: contractScriptHash,
        operation: operation,
        args: [u.str2hexstring("createSale"), [100, u.str2hexstring("My Best Widget")]] 
    });
   
    const config = {
        api: new api.neoCli.instance(rpcUrl),
        account: testUserAccount,
        script: script,
        intents: api.makeIntent({ NEO: 100 }, contractScriptHash)
     };

     var result = await Neon.doInvoke(config);
     console.log("\n\n--- Response ---");
     console.log(result.response);
 }

 mainAsync().catch(err => { console.log(err); });
