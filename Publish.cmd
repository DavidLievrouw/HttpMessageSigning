@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET DISTDIR=%DIR%\dist
SET PRODUCT=HttpMessageSigning

RD /S /Q %DISTDIR%

dotnet restore %SRCDIR%\%PRODUCT%.sln
dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
dotnet publish %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Signing\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
dotnet publish %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.AuthenticationScheme\%PRODUCT%.Verification.AuthenticationScheme.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.AuthenticationScheme\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.Owin\%PRODUCT%.Verification.Owin.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.Owin\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages"
