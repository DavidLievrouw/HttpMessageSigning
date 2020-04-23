@echo off
cls
SET DIR=%~dp0%
SET KEY=s3cr37
SET SOURCE=https://api.nuget.org/v3/index.json
SET VERSION=4.0.0

ECHO Press [ENTER] to push the packages to nuget.org
PAUSE

REM dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.%VERSION%.nupkg -k %KEY% -s %SOURCE%
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Signing.%VERSION%.nupkg -k %KEY% -s %SOURCE%
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.%VERSION%.nupkg -k %KEY% -s %SOURCE%
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.AspNetCore.%VERSION%.nupkg -k %KEY% -s %SOURCE%
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.MongoDb.%VERSION%.nupkg -k %KEY% -s %SOURCE%
dotnet nuget push %DIR%dist\NuGetPackages\Dalion.HttpMessageSigning.Verification.Owin.%VERSION%.nupkg -k %KEY% -s %SOURCE%

PAUSE
