echo off
rem ensure msbuild is in your path (call vcvarsall.bat or use this file from VS developer prompt or MSBuild prompt)


rem setup env. variables
set SOURCE_DIR=%~dp0\..\src
set DOWNLOAD_FOLDER=%SOURCE_DIR%\download
set ASSEMBLY_FOLDER=%SOURCE_DIR%\KSP_Assemblies

cd %SOURCE_DIR%

rem download required KSP DLLs
curl -fsS -o%DOWNLOAD_FOLDER%\dlls.7z https://d237kiopfuf7h0.cloudfront.net/download/KSPDLL_1_3_1.zip

rem extract them
cd %DOWNLOAD_FOLDER% & dir
rem 7z.exe e -p%DLL_ARCH_PASS% -o%ASSEMBLY_FOLDER% %DOWNLOAD_FOLDER%\dlls.7z
7z.exe e -phelloworld -o%ASSEMBLY_FOLDER% %DOWNLOAD_FOLDER%\dlls.7z
cd %SOURCE_DIR% & dir

msbuild RemoteTech-Complete.sln /target:Clean /p:PreBuildEvent="" /p:PostBuildEvent="" /p:Configuration=Release /target:Build

rem delete KSP downloaded assembly files
del %ASSEMBLY_FOLDER%\Assembly-CSharp.dll
del %ASSEMBLY_FOLDER%\Assembly-CSharp-firstpass.dll
del %ASSEMBLY_FOLDER%\UnityEngine.dll
del %ASSEMBLY_FOLDER%\UnityEngine.UI.dll

cd %~dp0
