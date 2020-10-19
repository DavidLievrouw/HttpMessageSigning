@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=%DIR%\src

IF EXIST %SRCDIR%\coverage.xml DEL /F %SRCDIR%\coverage.xml
IF EXIST "%SRCDIR%\coverage-*.json" DEL /F %SRCDIR%\coverage-*.json

dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.json" /p:MergeWith="../coverage-core.net472.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Tests\HttpMessageSigning.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-core.json" /p:MergeWith="../coverage-core.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Signing.Tests\HttpMessageSigning.Signing.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-signing.json" /p:MergeWith="../coverage-core.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Signing.Tests\HttpMessageSigning.Signing.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-signing.json" /p:MergeWith="../coverage-signing.net472.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Signing.Tests\HttpMessageSigning.Signing.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-signing.json" /p:MergeWith="../coverage-signing.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Tests\HttpMessageSigning.Verification.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-verification.json" /p:MergeWith="../coverage-signing.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Tests\HttpMessageSigning.Verification.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-verification.json" /p:MergeWith="../coverage-verification.net472.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Tests\HttpMessageSigning.Verification.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-verification.json" /p:MergeWith="../coverage-verification.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.json" /p:MergeWith="../coverage-verification.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.json" /p:MergeWith="../coverage-mongodb.net472.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.MongoDb.Tests\HttpMessageSigning.Verification.MongoDb.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-mongodb.json" /p:MergeWith="../coverage-mongodb.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.AspNetCore.Tests\HttpMessageSigning.Verification.AspNetCore.Tests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-aspnetcore.json" /p:MergeWith="../coverage-mongodb.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.AspNetCore.Tests\HttpMessageSigning.Verification.AspNetCore.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-aspnetcore.json" /p:MergeWith="../coverage-aspnetcore.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.Verification.Owin.Tests\HttpMessageSigning.Verification.Owin.Tests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning.Verification.Owin*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-owin.json" /p:MergeWith="../coverage-aspnetcore.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj --framework net472 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-system.json" /p:MergeWith="../coverage-owin.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj --framework netcoreapp2.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-system.json" /p:MergeWith="../coverage-system.net472.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\HttpMessageSigning.SystemTests\HttpMessageSigning.SystemTests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=json /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage-system.json" /p:MergeWith="../coverage-system.netcoreapp2.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

dotnet test %SRCDIR%\Conformance.Tests\Conformance.Tests.csproj --framework netcoreapp3.1 /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:UseSourceLink=true /p:Include="[Dalion.HttpMessageSigning*]*" /p:Exclude=\"[*Test*]*,[Dalion.HttpMessageSigning.TestUtils]*\" /p:ExcludeByAttribute=\"Obsolete,ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverage,ExcludeFromCodeCoverageAttribute\" /p:CoverletOutput="../coverage.xml" /p:MergeWith="../coverage-system.netcoreapp3.1.json" /maxcpucount:1
if errorlevel 1 (
   echo One or more tests failed.
   exit /b %errorlevel%
)

if "%1" == "nopause" goto end
pause
:end
