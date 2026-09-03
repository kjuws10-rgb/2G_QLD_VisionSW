@echo off
setlocal

set REPO_URL=https://github.com/kjuws10-rgb/2G_QLD_VisionSW.git
set BRANCH=main
set COMMIT_MSG=auto commit

echo ==============================
echo GitHub Push Start
echo REPO: %REPO_URL%
echo ==============================

git init
git branch -M %BRANCH%

git remote remove origin 2>nul
git remote add origin %REPO_URL%

git fetch origin

git add .
git commit -m "%COMMIT_MSG%" 2>nul

git pull origin %BRANCH% --rebase

git push -u origin %BRANCH%

echo ==============================
echo Push Complete
echo ==============================
pause
