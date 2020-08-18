@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET PRODUCT=HttpMessageSigning

dotnet test %SRCDIR%\%PRODUCT%.Tests\%PRODUCT%.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Signing.Tests\%PRODUCT%.Signing.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Verification.Tests\%PRODUCT%.Verification.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Verification.AspNetCore.Tests\%PRODUCT%.Verification.AspNetCore.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Verification.Owin.Tests\%PRODUCT%.Verification.Owin.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.Verification.MongoDb.Tests\%PRODUCT%.Verification.MongoDb.Tests.csproj
dotnet test %SRCDIR%\%PRODUCT%.SystemTests\%PRODUCT%.SystemTests.csproj
dotnet test %SRCDIR%\Conformance.Tests\Conformance.Tests.csproj

pause