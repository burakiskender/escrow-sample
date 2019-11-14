neo-express checkpoint restore .\checkpoints\cp5-buyer-deposit.neo-express-checkpoint --force
cmd /c start neo-express run -s 1
start-sleep -Seconds 1
npm run shipConfirm --prefix=.\scripts\
neo-express checkpoint create checkpoints\cp6-ship-confirm.neo-express-checkpoint --force --online
