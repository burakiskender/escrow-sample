neo-express checkpoint restore .\checkpoints\cp6-ship-confirm.neo-express-checkpoint --force
cmd /c start neo-express run -s 1
start-sleep -Seconds 1
npm run receiveConfirm --prefix=.\scripts\
neo-express checkpoint create checkpoints\cp7-receive-confirm --force --online
