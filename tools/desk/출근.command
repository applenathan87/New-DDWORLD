#!/bin/bash
# 마왕성 인사팀 · 출근부 — 맥 실행 파일 (더블클릭 = 출근)
# git pull 뒤 서버를 백그라운드로 켜고(창 없음, 로그: tools/desk/desk.log) 이 창은 닫아도 된다.
cd "$(dirname "$0")/../.." || exit 1
echo "[desk] git pull ..."
git pull --ff-only || echo "[desk] pull 실패 (오프라인이거나 충돌). 그대로 계속합니다."
echo "[desk] 서버를 백그라운드로 켭니다 (끄기: tools/desk/출근부-끄기.command)"
nohup node tools/desk/server.js --log tools/desk/desk.log > /dev/null 2>&1 &
sleep 1
echo "[desk] 완료. 이 창은 닫아도 됩니다."
