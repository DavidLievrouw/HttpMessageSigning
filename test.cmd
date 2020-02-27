@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET PRODUCT=HttpMessageSigning

dotnet test %SRCDIR%\%PRODUCT%.Tests\%PRODUCT%.Tests.csproj

pause