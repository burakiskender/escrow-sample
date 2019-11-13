import * as path from "path";
import * as fs from "fs";
import { promisify } from "util";

const readFile = promisify(fs.readFile);
const rootPath = path.resolve(__dirname, '..');
const abiPath = path.join(rootPath, 'contract\\bin\\Debug\\netstandard2.0\\publish\\Escrow.abi.json');

export async function getContractHash() {
    return JSON.parse(await readFile(abiPath, "utf8")).hash.substring(2);
}