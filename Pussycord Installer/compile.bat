@echo off
echo Cleaning old files...
rmdir /s /q bin
rmdir /s /q obj

echo Publishing Single File to the current folder...
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o .

echo.
echo ========================================================
echo BUILD COMPLETE!
echo The file PussycordInstaller.exe is in this folder.
echo You can delete the 'filestoinstall' folder if you want,
echo because the files are now inside the EXE.
echo ========================================================
pause