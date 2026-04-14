---
Day: 19
날짜: 2026-04-06
작업시간: 0.5
상태: 완료
---
[[데브로그/데브로그]]
# Day 19 — MacBook 세팅 가이드 작성

## 한 일
- Windows → MacBook 이전을 대비한 세팅 가이드 문서 작성 (`MacBook_Setup_Guide.md`)
- 백업 대상 파일 정리 (CLAUDE.md, settings.json, DDworld-vault)
- 환경별 세팅 순서 문서화: Homebrew → Node.js → Claude Code → Obsidian → VS Code → Unity
- Mac/Windows 경로 차이 및 주의사항 정리 (`C:\Users\apple\` → `~/`)
- Notion MCP 포함한 settings.json Mac 버전 작성 (cmd /c 없이 npx 직접 사용)
- 세팅 완료 체크리스트 작성

## 배운 것
- Mac은 `cmd /c` 없이 `npx` 바로 사용 가능 (settings.json 구조 차이)
- 한글 폴더명은 Mac 터미널에서 따옴표로 감싸야 안전
- Unity 버전 불일치 시 프로젝트 파일 깨짐 위험 → 반드시 동일 버전 설치

## 막힌 것
- 없음

## 메모
- Notion API 키(`ntn_...`)는 settings.json 복사 시 함께 이전 필요
- Google Drive 경로 바뀌면 CLAUDE.md 경로도 업데이트해야 함

## 다음 목표
- 게임개요.md / 핵심경험.md 채우기 시작
- GDC 강연 읽고 인사이트 MOC에 연결해보기
