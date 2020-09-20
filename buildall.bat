@echo off
echo Building.
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\win-x64 --self-contained false --runtime win-x64 --verbosity quiet
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\win-arm --self-contained false --runtime win-arm --verbosity quiet
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\osx-x64 --self-contained false --runtime osx-x64 --verbosity quiet
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\linux-arm --self-contained false --runtime linux-arm --verbosity quiet
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\linux-x64 --self-contained false --runtime linux-x64 --verbosity quiet
dotnet publish . --configuration Release --output 3Snipe\bin\Release\net5.0\win-x86 --self-contained false --runtime win-x86 --verbosity quiet
echo All built.
pause