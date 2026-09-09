@echo off
rem Mawang HR - Desk launcher (Windows). Double-click = clock in.
rem 1) git pull (sync from the MacBook)  2) start the local server (it opens the browser)
chcp 65001 >nul
title desk
cd /d "%~dp0..\.."
echo [desk] git pull ...
git pull --ff-only
if errorlevel 1 echo [desk] pull failed (offline or conflict) - continuing anyway.
echo.
node "tools\desk\server.js"
pause
