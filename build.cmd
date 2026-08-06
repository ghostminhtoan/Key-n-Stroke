@echo off
setlocal

set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"

echo Restoring packages...
dotnet restore KeyNStroke\KeyNStroke.csproj -r win --packages packages
if %ERRORLEVEL% neq 0 (
    echo Restore failed!
    exit /b %ERRORLEVEL%
)

echo Building Key-n-Stroke Release...
"%MSBUILD_PATH%" KeyNStroke.sln /p:Configuration=Release
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

echo Build Succeeded!
echo Starting application...
start "" "KeyNStroke\bin\Release\Key-n-Stroke.exe"
explorer "KeyNStroke\bin\Release"
