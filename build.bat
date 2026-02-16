@echo off
echo ================================================
echo Keyboard Layout Fixer - Build Script
echo ================================================
echo.

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo .NET SDK detected
echo.

echo Cleaning previous builds...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo.

echo Restoring dependencies...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore dependencies
    pause
    exit /b 1
)
echo.

echo Building Release version...
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo ================================================
echo Build completed successfully!
echo ================================================
echo.
echo Executable location: bin\Release\net8.0-windows\KeyboardLayoutFixer.exe
echo.

echo Would you like to create a standalone executable? (Y/N)
set /p standalone=

if /i "%standalone%"=="Y" (
    echo.
    echo Creating standalone single-file executable...
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
    if errorlevel 1 (
        echo ERROR: Publish failed
        pause
        exit /b 1
    )
    echo.
    echo ================================================
    echo Standalone executable created!
    echo ================================================
    echo.
    echo Location: bin\Release\net8.0-windows\win-x64\publish\KeyboardLayoutFixer.exe
    echo.
    echo This file can be distributed and run on any Windows 10+ machine
    echo without requiring .NET to be installed.
)

echo.
pause
