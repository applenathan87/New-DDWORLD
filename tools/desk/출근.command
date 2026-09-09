#!/bin/bash
# 마왕성 인사팀 · 출근부 — 맥 실행 파일 (더블클릭 = 출근)
# 처음 한 번만: 터미널에서  chmod +x tools/desk/출근.command
cd "$(dirname "$0")/../.." || exit 1
echo "[desk] git pull ..."
git pull --ff-only || echo "[desk] pull 실패 (오프라인이거나 충돌). 그대로 계속합니다."
node tools/desk/server.js
