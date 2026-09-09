#!/bin/bash
# 마왕성 인사팀 · 출근부 — 백그라운드 서버 끄기 (맥)
cd "$(dirname "$0")/../.." || exit 1
PORT=$(node -p "require('./tools/desk/desk.config.json').port||4123" 2>/dev/null || echo 4123)
PIDS=$(lsof -ti tcp:"$PORT" -sTCP:LISTEN 2>/dev/null)
if [ -n "$PIDS" ]; then kill $PIDS && echo "[desk] 서버를 껐습니다 (포트 $PORT)"; else echo "[desk] 켜져 있는 서버가 없습니다"; fi
