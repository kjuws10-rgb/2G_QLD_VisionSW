@echo off
echo =======================
echo GitHub Push Start
echo REPO: https://github.com/kjuws10-rgb/2G_QLD_VisionSW.git
echo ==============================

cd /d C:\Users\jwkang01\Downloads\2G_QLD_VisionSW

git init
git remote remove origin 2>nul
git remote add origin https://github.com/kjuws10-rgb/2G_QLD_VisionSW.git
git fetch origin
git checkout main

git rm --cached wonik_sd_vision_align -r 2>nul

if exist wonik_sd_vision_align\.git (
    rd /s /q wonik_sd_vision_align\.git
)

git add -A

git diff --cached --quiet
if %errorlevel% == 1 (
    git commit -m "Update files"
    git push origin main
) else (
    echo No changes to commit.
)

echo ==============================
echo Push Complete
pause
