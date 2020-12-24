@echo off

echo Building Windows 64-Bit release..
echo.
dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -r win-x64
echo Done.
echo.

echo Building Windows 32-Bit release..
echo.
dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -r win-x86
echo Done.

echo.
echo.
echo Finished all builds.

timeout /t -1