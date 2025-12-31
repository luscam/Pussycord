@echo off
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -o ./dist
echo Build concluido em ./dist/Pussycord.exe
pause