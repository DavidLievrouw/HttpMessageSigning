@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET DISTDIR=%DIR%\dist
SET PRODUCT=HttpMessageSigning

RD /S /Q %DISTDIR%

dotnet restore %SRCDIR%\%PRODUCT%.sln
dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
