@echo off
setlocal EnableExtensions

set "REPO=https://github.com/kjuws10-rgb/2G_QLD_VisionSW.git"
set "ROOT=%~dp0"
set "COMMIT_MESSAGE=%~1"
if not defined COMMIT_MESSAGE set "COMMIT_MESSAGE=Update files"

echo ==============================
echo GitHub Push Start
echo REPO: %REPO%
echo ==============================

pushd "%ROOT%"
if errorlevel 1 (
    echo ERROR: Failed to enter repository folder: %ROOT%
    goto fail_no_popd
)

where git >nul 2>nul
if errorlevel 1 (
    echo ERROR: Git was not found.
    goto fail
)

if not exist ".git\" (
    echo ERROR: This folder is not a Git working tree.
    echo Clone the repository first, then run this file from the repository root.
    goto fail
)

git remote get-url origin >nul 2>nul
if errorlevel 1 (
    git remote add origin "%REPO%"
    if errorlevel 1 goto fail
)

for /f "delims=" %%B in ('git branch --show-current') do set "CURRENT_BRANCH=%%B"
if /I not "%CURRENT_BRANCH%"=="main" (
    echo ERROR: Run this script while the main branch is checked out.
    echo Current branch: %CURRENT_BRANCH%
    goto fail
)

echo [1/7] Fetching origin...
git fetch origin
if errorlevel 1 goto fail

echo [2/7] Updating local main...
git pull --ff-only origin main
if errorlevel 1 goto fail

echo [3/7] Staging changes...
git add -A -- .
if errorlevel 1 goto fail

git diff --cached --quiet
set "DIFF_EXIT=%ERRORLEVEL%"
if "%DIFF_EXIT%"=="0" goto no_changes
if not "%DIFF_EXIT%"=="1" goto fail

for /f %%T in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "STAMP=%%T"
if not defined STAMP (
    echo ERROR: Failed to create a branch timestamp.
    goto fail
)
set "WORK_BRANCH=update/local-%STAMP%"

echo [4/7] Creating work branch %WORK_BRANCH%...
git switch -c "%WORK_BRANCH%"
if errorlevel 1 goto fail

echo [5/7] Committing and pushing the work branch...
git commit -m "%COMMIT_MESSAGE%"
if errorlevel 1 goto fail
git push -u origin "%WORK_BRANCH%"
if errorlevel 1 goto fail

echo [6/7] Merging the work branch into main...
git switch main
if errorlevel 1 goto fail
git pull --ff-only origin main
if errorlevel 1 goto fail
git merge --no-ff "%WORK_BRANCH%" -m "Merge branch '%WORK_BRANCH%'"
if errorlevel 1 goto fail

echo [7/7] Pushing main...
git push origin main
if errorlevel 1 goto fail

echo Work branch: %WORK_BRANCH%
goto success

:no_changes
echo No changes to commit.
goto success

:fail
echo.
echo ==============================
echo Push Failed
echo Review the error above. Git status follows:
git status --short --branch
popd
pause
exit /b 1

:fail_no_popd
echo.
echo ==============================
echo Push Failed
pause
exit /b 1

:success
echo ==============================
echo Push Complete
popd
pause
exit /b 0
