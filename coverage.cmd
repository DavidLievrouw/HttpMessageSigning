@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src

IF EXIST %SRCDIR%\coverage-core.net472.xml /F %SRCDIR%\coverage-core.net472.xml
IF EXIST %SRCDIR%\coverage-core.netcoreapp2.1.xml DEL /F %SRCDIR%\coverage-core.netcoreapp2.1.xml
IF EXIST %SRCDIR%\coverage-core.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage-core.netcoreapp3.1.xml

IF EXIST %SRCDIR%\coverage-conformance.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage-conformance.netcoreapp3.1.xml

IF EXIST %SRCDIR%\coverage-system.netcoreapp2.1.xml DEL /F %SRCDIR%\coverage-system.netcoreapp2.1.xml
IF EXIST %SRCDIR%\coverage-system.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage-system.netcoreapp3.1.xml

IF EXIST %SRCDIR%\coverage-aspnetcore.netcoreapp2.1.xml DEL /F %SRCDIR%\coverage-aspnetcore.netcoreapp2.1.xml
IF EXIST %SRCDIR%\coverage-aspnetcore.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage-aspnetcore.netcoreapp3.1.xml

IF EXIST %SRCDIR%\coverage-mongodb.net472.xml /F %SRCDIR%\coverage-mongodb.net472.xml
IF EXIST %SRCDIR%\coverage-mongodb.netcoreapp2.1.xml DEL /F %SRCDIR%\coverage-mongodb.netcoreapp2.1.xml
IF EXIST %SRCDIR%\coverage-mongodb.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage-mongodb.netcoreapp3.1.xml

IF EXIST %SRCDIR%\coverage-owin.net472.json DEL /F %SRCDIR%\coverage-owin.net472.json

dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.net472.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.netcoreapp2.1.json" /p:MergeWith="../coverage-core.net472.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.netcoreapp3.1.json" /p:MergeWith="../coverage-core.netcoreapp2.1.json" /maxcpucount:1

dotnet test %SRCDIR%\Conformance.Tests\Conformance.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-conformance.netcoreapp3.1.json" /p:MergeWith="../coverage-core.netcoreapp3.1.json" /maxcpucount:1

dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-system.netcoreapp2.1.json" /p:MergeWith="../coverage-conformance.netcoreapp3.1.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-system.netcoreapp3.1.json" /p:MergeWith="../coverage-system.netcoreapp2.1.json" /maxcpucount:1

dotnet test %SRCDIR%\HttpMessageSigning.Verification.AspNetCore.Tests\HttpMessageSigning.Verification.AspNetCore.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-aspnetcore.netcoreapp2.1.json" /p:MergeWith="../coverage-system.netcoreapp3.1.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Verification.AspNetCore.Tests\HttpMessageSigning.Verification.AspNetCore.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-aspnetcore.netcoreapp3.1.json" /p:MergeWith="../coverage-aspnetcore.netcoreapp2.1.json" /maxcpucount:1

dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.net472.json" /p:MergeWith="../coverage-aspnetcore.netcoreapp3.1.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.netcoreapp2.1.json" /p:MergeWith="../coverage-mongodb.net472.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.netcoreapp3.1.json" /p:MergeWith="../coverage-mongodb.netcoreapp2.1.json" /maxcpucount:1

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Owin.Tests\HttpMessageSigning.Verification.Owin.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning.Verification.Owin*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage.xml" /p:MergeWith="../coverage-mongodb.netcoreapp3.1.json" /maxcpucount:1

pause