@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src

dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Signing.Tests\HttpMessageSigning.Signing.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Tests\HttpMessageSigning.Verification.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.AspNetCore.Tests\HttpMessageSigning.Verification.AspNetCore.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Owin.Tests\HttpMessageSigning.Verification.Owin.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

REM dotnet test %SRCDIR%\HttpMessageSigning.Verification.SqlServer.Tests\HttpMessageSigning.Verification.SqlServer.Tests.csproj
REM if errorlevel 1 (
REM    echo One or more tests failed.
REM    exit /b %errorlevel%
REM )

dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\Conformance.Tests\Conformance.Tests.csproj
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

if "%1" == "nopause" goto end
pause
:end