@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET DISTDIR=%DIR%\dist
SET PRODUCT=HttpMessageSigning

IF EXIST %DISTDIR% RD /S /Q %DISTDIR%

dotnet restore %SRCDIR%\%PRODUCT%.sln
dotnet build %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"
dotnet build %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"
dotnet build %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"
dotnet build %SRCDIR%\%PRODUCT%.Verification.AspNetCore\%PRODUCT%.Verification.AspNetCore.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"
dotnet build %SRCDIR%\%PRODUCT%.Verification.MongoDb\%PRODUCT%.Verification.MongoDb.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"
dotnet build %SRCDIR%\%PRODUCT%.Verification.Owin\%PRODUCT%.Verification.Owin.csproj --no-restore --configuration Release -p:BaseOutputPath="%DISTDIR%\\" -p:ContinuousIntegrationBuild="true"

if "%1" == "nopause" goto end
pause
:end