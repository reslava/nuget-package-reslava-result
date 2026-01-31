@echo off
REM Build and Package Script for REslava.Result Multi-Package Solution
REM Usage: build-packages.bat [version]
REM Example: build-packages.bat 1.0.0

setlocal enabledelayedexpansion

if "%1"=="" (
    set VERSION=1.0.0
) else (
    set VERSION=%1
)

set OUTPUT_DIR=nupkgs

echo ======================================
echo Building REslava.Result Packages
echo Version: %VERSION%
echo ======================================
echo.

REM Clean previous builds
echo Cleaning previous builds...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
if exist "%OUTPUT_DIR%" rd /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo.
echo ======================================
echo Step 1/3: Building Core Package
echo ======================================
cd SourceGenerator\Core
dotnet clean
dotnet build -c Release
if errorlevel 1 exit /b 1
dotnet pack -c Release -o "..\..\%OUTPUT_DIR%"
if errorlevel 1 exit /b 1
cd ..\..

echo.
echo ======================================
echo Step 2/3: Building SourceGenerators
echo ======================================
cd SourceGenerator
dotnet clean
dotnet build -c Release
if errorlevel 1 exit /b 1
dotnet pack -c Release -o "..\%OUTPUT_DIR%"
if errorlevel 1 exit /b 1
cd ..

echo.
echo ======================================
echo Step 3/3: Building Main Library
echo ======================================
cd src
dotnet clean
dotnet build -c Release
if errorlevel 1 exit /b 1
dotnet pack -c Release -o "..\%OUTPUT_DIR%"
if errorlevel 1 exit /b 1
cd ..

echo.
echo ======================================
echo Package Build Complete
echo ======================================
echo.
echo Created packages:
dir /b "%OUTPUT_DIR%\*.nupkg"

echo.
echo ======================================
echo Next Steps
echo ======================================
echo.
echo 1. Test locally:
echo    mkdir %USERPROFILE%\local-nuget
echo    copy %OUTPUT_DIR%\*.nupkg %USERPROFILE%\local-nuget\
echo    dotnet nuget add source %USERPROFILE%\local-nuget -n LocalFeed
echo.
echo 2. Create test project:
echo    cd TestProject
echo    dotnet add package REslava.Result --version %VERSION% --source LocalFeed
echo    dotnet add package REslava.Result.SourceGenerators --version %VERSION% --source LocalFeed
echo    dotnet build
echo.
echo 3. Publish to NuGet.org:
echo    dotnet nuget push %OUTPUT_DIR%\*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
echo.
echo Build script completed successfully!

endlocal
