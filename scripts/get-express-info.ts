import * as path from "path";
import * as fs from "fs";
import { promisify } from "util";

const readFile = promisify(fs.readFile);
const rootPath = path.resolve(__dirname, '..');
const expressPath = path.join(rootPath, 'default.neo-express.json');

export async function getExpressData() {
    return JSON.parse(await readFile(expressPath, "utf8"));
}

export async function getPrivateKey(name : string, expressData : any = null) {
    if (!expressData) {
        expressData = await getExpressData();
    }

    var wallet = expressData.wallets.find(e => e.name === name);
    return wallet.accounts[0]["private-key"];
}
