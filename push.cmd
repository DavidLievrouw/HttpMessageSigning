@echo off
cls
SET DIR=%~dp0%
SET KEY=s3cr37
SET SOURCE=https://api.nuget.org/v3/index.json

for /f "delims=" %%x in (version.txt) do set VERSION=%%x

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.%VERSION%.nupkg -k %KEY% -s %SOURCE%

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning.Signing v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Signing.%VERSION%.nupkg -k %KEY% -s %SOURCE%

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning.Verification v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.%VERSION%.nupkg -k %KEY% -s %SOURCE%

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning.Verification.AspNetCore v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.AspNetCore.%VERSION%.nupkg -k %KEY% -s %SOURCE%

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning.Verification.MongoDb v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.MongoDb.%VERSION%.nupkg -k %KEY% -s %SOURCE%

ECHO Press [ENTER] to push the package Dalion.HttpMessageSigning.Verification.Owin v%VERSION% to nuget.org
PAUSE
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.Owin.%VERSION%.nupkg -k %KEY% -s %SOURCE%

PAUSE
