@echo off
setlocal

set "ROOT=%~dp0"
set "SOLUTION=Vision_Align.sln"
set "RUN_PROJECT=Vision_Align\Vision_Align.csproj"
set "FALLBACK_GIT=C:\Users\jwkang01\Documents\Codex\2026-07-08\github-pr\work\PortableGit\cmd\git.exe"

pushd "%ROOT%"
if errorlevel 1 (
    echo Failed to enter project folder: %ROOT%
    exit /b 1
)

where git >nul 2>nul
if %errorlevel%==0 (
    set "GIT=git"
) else if exist "%FALLBACK_GIT%" (
    set "GIT=%FALLBACK_GIT%"
) else (
    echo Git was not found. Install Git for Windows or update FALLBACK_GIT in this file.
    popd
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo dotnet was not found. Install the .NET SDK before running this file.
    popd
    exit /b 1
)

echo [1/4] Pulling latest source...
"%GIT%" pull --ff-only origin main
if errorlevel 1 goto fail


popd
exit /b 0

:fail
echo.
echo Failed. Review the error output above.
popd
pause
exit /b 1
