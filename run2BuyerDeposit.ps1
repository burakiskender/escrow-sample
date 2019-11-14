neo-express checkpoint restore .\checkpoints\cp4-sale-created.neo-express-checkpoint --force
cmd /c start neo-express run -s 1
start-sleep -Seconds 1
npm run buyerDeposit --prefix=.\scripts\
neo-express checkpoint create checkpoints\cp5-buyer-deposit --force --online
