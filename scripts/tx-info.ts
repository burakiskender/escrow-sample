import * as path from "path";
import * as fs from "fs";
import { promisify } from "util";

const readFile = promisify(fs.readFile);
const writeFile = promisify(fs.writeFile);
const rootPath = path.resolve(__dirname, '..');
const txInfoPat = path.join(rootPath, 'txInfo.json');

export async function getTxInfo() {
    return JSON.parse(await readFile(txInfoPat, "utf8"));
}

export async function setTxInfo(info:any) {
    await writeFile(txInfoPat, JSON.stringify(info, null, 4), "utf8");
}