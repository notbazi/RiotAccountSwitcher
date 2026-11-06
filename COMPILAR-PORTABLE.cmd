@echo off
setlocal
set DOTNET_CLI_TELEMETRY_OPTOUT=1
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: Se necesita el SDK de .NET 10.
  echo https://dotnet.microsoft.com/download/dotnet/10.0
  pause
  exit /b 1
)
echo.
echo Compilando RiotSwitcherAccount (portable)...
dotnet publish "src\RiotSwitcherAccount.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o "dist\portable"
if errorlevel 1 (
  echo ERROR: No se pudo compilar.
  pause
  exit /b 1
)
if not exist "dist\portable\RiotSwitcherAccount.exe" (
  echo ERROR: No se genero el archivo esperado.
  pause
  exit /b 1
)
echo.
echo Correcto. Ejecutable:
echo %~dp0dist\portable\RiotSwitcherAccount.exe
echo.
echo Las cuentas se guardan en AppData, NO en el ejecutable.
pause
