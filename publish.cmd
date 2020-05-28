@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src
SET DISTDIR=%DIR%\dist
SET PRODUCT=HttpMessageSigning

RD /S /Q %DISTDIR%

dotnet restore %SRCDIR%\%PRODUCT%.sln

dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework netcoreapp2.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\netcoreapp2.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework netcoreapp3.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\netcoreapp3.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%\%PRODUCT%.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"

dotnet publish %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Signing\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release --framework netcoreapp2.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Signing\netcoreapp2.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release --framework netcoreapp3.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Signing\netcoreapp3.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Signing\%PRODUCT%.Signing.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Signing\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"

dotnet publish %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release --framework netcoreapp2.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification\netcoreapp2.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release --framework netcoreapp3.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification\netcoreapp3.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification\%PRODUCT%.Verification.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"

dotnet publish %SRCDIR%\%PRODUCT%.Verification.AspNetCore\%PRODUCT%.Verification.AspNetCore.csproj --no-restore --configuration Release --framework netcoreapp2.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.AspNetCore\netcoreapp2.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.AspNetCore\%PRODUCT%.Verification.AspNetCore.csproj --no-restore --configuration Release --framework netcoreapp3.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.AspNetCore\netcoreapp3.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"

dotnet publish %SRCDIR%\%PRODUCT%.Verification.MongoDb\%PRODUCT%.Verification.MongoDb.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.MongoDb\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.MongoDb\%PRODUCT%.Verification.MongoDb.csproj --no-restore --configuration Release --framework netcoreapp2.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.MongoDb\netcoreapp2.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.MongoDb\%PRODUCT%.Verification.MongoDb.csproj --no-restore --configuration Release --framework netcoreapp3.1 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.MongoDb\netcoreapp3.1" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="true"
dotnet publish %SRCDIR%\%PRODUCT%.Verification.MongoDb\%PRODUCT%.Verification.MongoDb.csproj --no-restore --configuration Release --framework netstandard2.0 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.MongoDb\netstandard2.0" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"

dotnet publish %SRCDIR%\%PRODUCT%.Verification.Owin\%PRODUCT%.Verification.Owin.csproj --no-restore --configuration Release --framework net472 /p:IsPublishing="true" /p:PublishDir="%DISTDIR%\%PRODUCT%.Verification.Owin\net472" /p:PackagePublishDir="%DISTDIR%\NuGetPackages" /p:SkipNuGetPack="false"
