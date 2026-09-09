@echo off
rem Mawang HR - Desk: stop the background server (the one listening on the port in desk.config.json).
chcp 65001 >nul
cd /d "%~dp0..\.."
set PORT=4123
for /f %%p in ('node -p "require('./tools/desk/desk.config.json').port||4123"') do set PORT=%%p
powershell -NoProfile -Command "$c = Get-NetTCPConnection -LocalPort %PORT% -State Listen -ErrorAction SilentlyContinue; if ($c) { $c | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }; Write-Output '[desk] server stopped (port %PORT%)' } else { Write-Output '[desk] server was not running' }"
ping -n 3 127.0.0.1 >nul
