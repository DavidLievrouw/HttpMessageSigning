@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src

IF EXIST %SRCDIR%\coverage-owin.net472.json DEL /F %SRCDIR%\coverage-owin.net472.json
IF EXIST %SRCDIR%\coverage.netcoreapp3.1.xml DEL /F %SRCDIR%\coverage.netcoreapp3.1.xml

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Owin.Tests\HttpMessageSigning.Verification.Owin.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning.Verification.Owin*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-owin.net472.json" /maxcpucount:1
dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude="[*.Tests]*" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage.xml" /p:MergeWith="../coverage-owin.net472.json" /maxcpucount:1

pause