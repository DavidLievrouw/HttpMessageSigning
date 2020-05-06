@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET PRODUCT=HttpMessageSigning

dotnet test %SRCDIR%\%PRODUCT%.Tests\%PRODUCT%.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Verification.Owin.Tests\%PRODUCT%.Verification.Owin.Tests.csproj
dotnet test %SRCDIR%\Conformance.Tests\Conformance.Tests.csproj

pause