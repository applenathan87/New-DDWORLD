---
Day: 11
날짜: 2026-03-17
작업시간: 3
상태: 완료
---
[[데브로그/데브로그]]
# Day 11 — Git & Notion 셋업 + Phase 1 완료 & 플레이어 이동 구현

## 한 일
- SampleScene에 Plane 오브젝트 추가 후 GitHub 푸시
- Git push/pull 워크플로우 학습
- CLAUDE.md 단축 명령어 설정
- Notion Dev Log 페이지 & 데이터베이스 생성, 기존 10일치 데이터 입력
- Phase 1-2 HD-2D 룩 테스트 완료 (Billboard, Bloom, Post-processing)
- 비주얼 방향 HD-2D 확정
- Phase 2-1 플레이어 이동 구현
  - Player 오브젝트 구성 (Rigidbody, Capsule Collider, 부모-자식 구조)
  - Unity 6 Input System 기반 8방향 이동 구현
  - 카메라 기준 이동 방향 보정
  - Idle / Run 애니메이션 연결
  - 좌우 방향 전환 시 페이퍼마리오 스타일 플립 효과 추가

## 배운 것
- git add → git commit → git push 흐름
- CLAUDE.md로 Claude 단축 명령어 등록하는 방법
- Rigidbody / Collider / 부모-자식 오브젝트 구조 개념
- Unity 6 Input System 방식 (OnMove 콜백)
- Animator Controller, Parameter, Transition 조건 설정
- 코루틴(IEnumerator)으로 애니메이션 효과 구현

## 막힌 것
- Orthographic 카메라에서 DOF가 동작하지 않음 → Tilt-Shift 방식으로 나중에 구현 예정
- 구버전 UnityEngine.Input 사용 오류 → Unity 6 Input System으로 전환

## 메모
- Claude Code로 첫 작업 시작
- 단순히 코드 작성만 하지 않고, 코드 리뷰를 할 수 있어야 할 것 같은데 방법을 고안해야 함

## 다음 목표
- Phase 2-2 공격 시스템 구현
