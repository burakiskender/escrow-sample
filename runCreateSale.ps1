dotnet publish
neo-express checkpoint restore .\checkpoints\cp2-seller-and-buyer.neo-express-checkpoint --force
cmd /c start neo-express run -s 1
start-sleep -Seconds 1
neo-express contract deploy .\contract\bin\Debug\netstandard2.0\publish\Escrow.avm genesis
start-sleep -Seconds 2
neo-express checkpoint create checkpoints\cp3-contract-deployed.neo-express-checkpoint --force --online 
npm run createSale --prefix=.\scripts\
neo-express checkpoint create checkpoints\cp4-sale-created.neo-express-checkpoint --force --online

