@echo off
rem Mawang HR - Desk launcher (Windows). Double-click = clock in.
rem 1) git pull (sync from the MacBook)  2) start the server in a hidden window (log: tools\desk\desk.log)
rem    The server opens the browser by itself. If it is already running, only the browser opens.
chcp 65001 >nul
title desk
cd /d "%~dp0..\.."
echo [desk] git pull ...
git pull --ff-only
if errorlevel 1 echo [desk] pull failed (offline or conflict) - continuing anyway.
echo [desk] starting server in the background (no window) ...
wscript.exe "%~dp0start-hidden.vbs"
set PORT=4123
for /f %%p in ('node -p "require('./tools/desk/desk.config.json').port||4123"') do set PORT=%%p
ping -n 3 127.0.0.1 >nul
echo [desk] opening http://localhost:%PORT% ...
start "" "http://localhost:%PORT%"
echo [desk] done. This window closes in 2 seconds. To stop the server: tools\desk\출근부-끄기.bat
ping -n 3 127.0.0.1 >nul
