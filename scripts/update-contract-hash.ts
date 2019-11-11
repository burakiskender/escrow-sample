import { promisify } from "util";
import * as fs from "fs";
import * as path from "path";
import { parse, stringify } from 'comment-json';

async function mainAsync() {

    const readFile = promisify(fs.readFile);
    const writeFile = promisify(fs.writeFile);

    const rootPath = path.resolve(__dirname, '..');

    const abiPath = path.join(rootPath, 'contract\\bin\\Debug\\netstandard2.0\\publish\\Escrow.abi.json');
    const abiHash = JSON.parse(await readFile(abiPath, "utf8")).hash.substring(2);

    const launchPath = path.join(rootPath, '.vscode\\launch.json');
    const launch = parse(await readFile(launchPath, "utf8"));
    
    const launchHash = launch.configurations[0].utxo.outputs[0].address;
    if (abiHash !== launchHash) {
        launch.configurations[0].utxo.outputs[0].address = abiHash;
        var launchJson = stringify(launch, null, 4);
        await writeFile(launchPath, launchJson, 'utf8');
        console.log(`updated ${abiHash}`);
    } 
}

mainAsync().catch(err => { console.log(err); });
