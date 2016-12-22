echo off
rem ensure msbuild is in your path (call vcvarsall.bat or use this file from VS developer prompt or MSBuild prompt)
set SOURCE_DIR=%~dp0\..\src
set OUTPUT_DIR=%SOURCE_DIR%\output
cd %SOURCE_DIR%
msbuild RemoteTech-Complete.sln /target:Clean /p:PreBuildEvent="" /p:PostBuildEvent="" /p:Configuration=Release /p:OutputPath=%OUTPUT_DIR% /target:Build
cd %~dp0
